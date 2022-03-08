using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using CLARTE.Memory;

namespace CLARTE.Net.Discovery
{
	public class Broadcaster : MonoBehaviour
	{
		[Serializable]
		public class ReceiveCallback : UnityEvent<IPEndPoint, byte[]>
		{

		}

		#region Members
		public ReceiveCallback onReceive;
		public ushort port = 65535;
		public bool broadcastToLocalhost = false;

		protected UdpClient udp;
		protected HashSet<IPAddress> localAddresses;
		protected List<IPEndPoint> broadcastAddresses;
		protected Threads.Thread thread;
		protected ManualResetEvent stop;
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			// Make sure that singleton is initialized in Unity thread before first use
			Threads.APC.MonoBehaviourCall.Instance.GetType();

			if (Utils.PortManager.Instance.ReservePort(port))
			{
				localAddresses = new HashSet<IPAddress>();
				broadcastAddresses = new List<IPEndPoint>();

				foreach (NetworkInterface net in NetworkInterface.GetAllNetworkInterfaces())
				{
					if (net != null && (net.NetworkInterfaceType == NetworkInterfaceType.Wireless80211 || net.NetworkInterfaceType == NetworkInterfaceType.Ethernet))
					{
						foreach (UnicastIPAddressInformation unicast_info in net.GetIPProperties().UnicastAddresses)
						{
							if (unicast_info.Address.AddressFamily == AddressFamily.InterNetwork)
							{
								byte[] address = unicast_info.Address.GetAddressBytes();
								byte[] mask = unicast_info.IPv4Mask.GetAddressBytes();

								for (int i = 0; i < address.Length && i < mask.Length; i++)
								{
									address[i] |= (byte)~mask[i];
								}

								IPAddress broadcast = new IPAddress(address);

								localAddresses.Add(unicast_info.Address);
								broadcastAddresses.Add(new IPEndPoint(broadcast, port));
							}
						}
					}
				}

				udp = new UdpClient()
				{
					ExclusiveAddressUse = !broadcastToLocalhost,
					EnableBroadcast = true
				};

				// To send/receive on the same host.
				udp.Client.MulticastLoopback = broadcastToLocalhost;
				udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, broadcastToLocalhost);

				udp.Client.Bind(new IPEndPoint(IPAddress.Any, port));

				stop = new ManualResetEvent(false);

				thread = new Threads.Thread(Listener);

				thread.Start();
			}
		}

		protected void OnDestroy()
		{
			stop.Set();

			thread.Join();

			udp.Dispose();

			stop.Dispose();

			udp = null;
			thread = null;
			stop = null;
		}
		#endregion

		#region Public methods
		public void Send(BufferPool.Buffer datagram)
		{
			foreach(IPEndPoint broadcast in broadcastAddresses)
			{
				Send(broadcast, datagram);
			}
		}

		public void Send(IPEndPoint endpoint, BufferPool.Buffer datagram)
		{
			if(datagram.Size > 0 && datagram.Size <= datagram.Data.Length)
			{
				udp.SendAsync(datagram.Data, (int) datagram.Size, endpoint);
			}
		}
		#endregion

		#region Internal methods
		protected void Listener()
		{
			while(!stop.WaitOne(0))
			{
				System.Threading.Tasks.Task<UdpReceiveResult> t = udp.ReceiveAsync();

				while(!t.Wait(100) && !stop.WaitOne(0)) { }

				if(t.Wait(0))
				{
					byte[] datagram = t.Result.Buffer;
					IPEndPoint from = t.Result.RemoteEndPoint;

					if(datagram.Length > 0 && (broadcastToLocalhost || !localAddresses.Contains(from.Address)))
					{
						Threads.APC.MonoBehaviourCall.Instance.Call(() => onReceive.Invoke(from, datagram));
					}
				}
			}
		}
		#endregion
	}
}
