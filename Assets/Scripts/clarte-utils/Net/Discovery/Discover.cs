using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using CLARTE.Memory;
using CLARTE.Serialization;

namespace CLARTE.Net.Discovery
{
	[RequireComponent(typeof(Broadcaster))]
	public class Discover : MonoBehaviour, IEnumerable<KeyValuePair<IPEndPoint, IServiceInfo>>
	{
		[Serializable]
		public class Service
		{
			#region Members
			public Negotiation.Server server;
			public IServiceInfoProvider info;
			#endregion
		}

		protected class Info
		{
			#region Members
			public IServiceInfo info;
			public long lastSeen;
			#endregion
		}

		[Serializable]
		public class Peer
		{
			#region Members
			public string ip;
			public ushort port;
			#endregion
		}

		protected class Datagram : IBinaryTypeMapped
		{
			#region Members
			public bool valid;
			public bool exist;
			public ushort port;
			public IServiceInfo info;
			#endregion

			#region Constructors
			public Datagram() // Required for Binary deserialization
			{
				valid = false;
				exist = false;
				port = 0;
				info = null;
			}

			public Datagram(bool exist, ushort port, IServiceInfo info)
			{
				this.exist = exist;
				this.port = port;
				this.info = info;

				valid = true;
			}
			#endregion

			#region IBinaryTypeMapped implementation
			public uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
			{
				uint read = 0;

				IBinaryTypeMapped i;

				read += serializer.FromBytes(buffer, start + read, out exist);
				read += serializer.FromBytes(buffer, start + read, out port);
				read += serializer.FromBytes(buffer, start + read, out i);

				info = (IServiceInfo) i;

				byte computed_checksum = ComputeControlSum(buffer.Data, start, read);

				byte received_checksum;

				read += serializer.FromBytes(buffer, start + read, out received_checksum);

				valid = (received_checksum == computed_checksum);

				return read;
			}

			public uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
			{
				uint written = 0;

				written += serializer.ToBytes(ref buffer, start + written, exist);
				written += serializer.ToBytes(ref buffer, start + written, port);
				written += serializer.ToBytes(ref buffer, start + written, info);
				written += serializer.ToBytes(ref buffer, start + written, ComputeControlSum(buffer.Data, start, written));

				return written;
			}
			#endregion

			#region Internal methods
			protected static byte ComputeControlSum(byte[] data, uint start, uint size)
			{
				byte result = 0;

				uint end = start + size;

				for(uint i = start; i < end; i++)
				{
					result ^= data[i];
				}

				return result;
			}
			#endregion
		}

		#region Members
		public Events.OnDiscoveredCallback onDiscovered;
		public Events.OnLostCallback onLost;
		public Events.OnActiveCallback onActive;
		public Events.OnInactiveCallback onInactive;
		public List<Service> advertise;
		public List<Peer> manualPeers;
		[Range(0.1f, 300f)]
		public float heartbeat = 2f; // In seconds
		[Range(1, 100)]
		public ushort lostAfterMissedHeartbeat = 15;

		protected Binary serializer;
		protected Threads.Thread sender;
		protected Threads.Thread cleaner;
		protected ManualResetEvent stopSender;
		protected ManualResetEvent stopCleaner;
		protected Broadcaster broadcast;
		protected Dictionary<IPEndPoint, Info> discovered;
		protected List<KeyValuePair<IPEndPoint, IServiceInfo>> remotesEnumerator;
		protected List<KeyValuePair<IPEndPoint, IServiceInfo>> pendingLost;
		#endregion

		#region IEnumerable implementation
		public IEnumerator<KeyValuePair<IPEndPoint, IServiceInfo>> GetEnumerator()
		{
			remotesEnumerator.Clear();

			lock(discovered)
			{
				foreach(KeyValuePair<IPEndPoint, Info> pair in discovered)
				{
					remotesEnumerator.Add(new KeyValuePair<IPEndPoint, IServiceInfo>(pair.Key, pair.Value.info));
				}
			}

			return remotesEnumerator.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			serializer = new Binary();

			discovered = new Dictionary<IPEndPoint, Info>();
			remotesEnumerator = new List<KeyValuePair<IPEndPoint, IServiceInfo>>();
			pendingLost = new List<KeyValuePair<IPEndPoint, IServiceInfo>>();

			broadcast = GetComponent<Broadcaster>();
		}

		protected void OnEnable()
		{
			if(sender == null)
			{
				stopSender = new ManualResetEvent(false);
				stopCleaner = new ManualResetEvent(false);

				broadcast.onReceive.AddListener(OnReceive);

				sender = new Threads.Thread(Sender);
				cleaner = new Threads.Thread(Cleaner);

				sender.Start();
				cleaner.Start();
			}
		}

		public void OnDisable()
		{
			if(sender != null)
			{
				SendBeacon(false);
			}

			stopSender?.Set();
			stopCleaner?.Set();

			if(broadcast != null)
			{
				broadcast.onReceive.RemoveListener(OnReceive);
			}

			sender?.Join();
			cleaner?.Join();

			stopSender?.Dispose();
			stopCleaner?.Dispose();

			discovered.Clear();

			sender = null;
			cleaner = null;
			stopSender = null;
			stopCleaner = null;
		}
		#endregion

		#region Internal methods
		protected void OnReceive(IPEndPoint endpoint, byte[] datagram)
		{
			if(datagram != null && datagram.Length > 0)
			{
				try
				{
					Datagram deserialized = serializer.Deserialize(datagram) as Datagram;

					if(deserialized != null)
					{
						endpoint.Port = deserialized.port;

						bool already_discovered;

						lock(discovered)
						{
							already_discovered = discovered.ContainsKey(endpoint);
						}

						if(deserialized.exist)
						{
							if(already_discovered)
							{
								lock(discovered)
								{
									Info info = discovered[endpoint];

									bool active = info.info.Active;

									info.info = deserialized.info;
									info.lastSeen = GetCurrentTime();

									if(deserialized.info.Active != active)
									{
										if (deserialized.info.Active)
										{
											Threads.APC.MonoBehaviourCall.Instance.Call(() => onActive.Invoke(endpoint, deserialized.info));
										}
										else
										{
											Threads.APC.MonoBehaviourCall.Instance.Call(() => onInactive.Invoke(endpoint, deserialized.info));
										}
									}
								}
							}
							else
							{
								lock(discovered)
								{
									discovered.Add(endpoint, new Info { info = deserialized.info, lastSeen = GetCurrentTime() });
								}

								Threads.APC.MonoBehaviourCall.Instance.Call(() => onDiscovered.Invoke(endpoint, deserialized.info));
							}
						}
						else if(!deserialized.exist && already_discovered)
						{
							lock(discovered)
							{
								discovered.Remove(endpoint);
							}

							Threads.APC.MonoBehaviourCall.Instance.Call(() => onLost.Invoke(endpoint, deserialized.info));
						}
					}
				}
				catch(Binary.DeserializationException) { }
			}
		}

		protected void SendBeacon(bool exist)
		{
			if(advertise != null)
			{
				foreach(Service service in advertise)
				{
					if(service.server != null)
					{
						try
						{
							IServiceInfo info = service.info.ServiceInfo;

							if(info != null)
							{
								info.Active = exist && service.server.CurrentState == Negotiation.Base.State.RUNNING;

								BufferPool.Buffer data = serializer.Serialize(new Datagram(exist, service.server.port, info));

								if(data != null && data.Size > 0)
								{
									broadcast.Send(data);

									if(manualPeers != null)
									{
										foreach(Peer peer in manualPeers)
										{
											if(IPAddress.TryParse(peer.ip, out IPAddress ip))
											{
												broadcast.Send(new IPEndPoint(ip, peer.port), data);
											}
										}
									}
								}
							}
						}
						catch(Binary.SerializationException e) {
							Debug.LogErrorFormat("{1}: {2}\n{3}", e.GetType(), e.Message, e.StackTrace);
						}
					}
				}
			}
		}

		protected void Sender()
		{
			while(!stopSender.WaitOne((int) (heartbeat * 1000)))
			{
				SendBeacon(true);
			}
		}

		protected void Cleaner()
		{
			while(!stopCleaner.WaitOne((int) (heartbeat * 1000)))
			{
				pendingLost.Clear();

				lock(discovered)
				{
					foreach(KeyValuePair<IPEndPoint, Info> pair in discovered)
					{
						if(GetCurrentTime() - pair.Value.lastSeen >= (lostAfterMissedHeartbeat + 1) * 1000 * heartbeat)
						{
							pendingLost.Add(new KeyValuePair<IPEndPoint, IServiceInfo>(pair.Key, pair.Value.info));
						}
					}

					foreach(KeyValuePair<IPEndPoint, IServiceInfo> lost in pendingLost)
					{
						discovered.Remove(lost.Key);
					}
				}

				foreach(KeyValuePair<IPEndPoint, IServiceInfo> lost in pendingLost)
				{
					Threads.APC.MonoBehaviourCall.Instance.Call(() => onLost.Invoke(lost.Key, lost.Value));
				}
			}
		}

		protected long GetCurrentTime()
		{
			return DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		}
		#endregion
	}
}
