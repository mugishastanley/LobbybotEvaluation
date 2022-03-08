#if !NETFX_CORE

using System;
using System.Net;
using System.Net.Sockets;
using UnityEngine.Events;

namespace CLARTE.Net.Negotiation.Connection
{
	public class Udp : Socket
	{
		#region Members
		protected UdpClient client;
		protected ushort localPort;
		protected ushort remotePort;
		#endregion

		#region Constructors
		public Udp(Negotiation.Base parent, Message.Negotiation.Parameters parameters, UnityAction<Base> disconnection_handler, UdpClient client, ushort local_port, ushort remote_port) : base(parent, parameters, disconnection_handler, client.Client)
		{
			this.client = client;

			localPort = local_port;
			remotePort = remote_port;
		}
		#endregion

		#region Getter / Setter
		public ushort LocalPort
		{
			get
			{
				return localPort;
			}
		}

		public ushort RemotePort
		{
			get
			{
				return remotePort;
			}
		}
		#endregion

		#region IDisposable implementation
		protected override void DisposeInternal(bool disposing)
		{
			base.DisposeInternal(disposing);

			if(disposing)
			{
				// TODO: delete managed state (managed objects).

				ushort port = 0;

				if(client != null)
				{
					port = (ushort) ((IPEndPoint) client.Client.LocalEndPoint).Port;
				}

				// Close the client
				SafeDispose(client);

				// Release the used port
				if (parent != null && port != 0)
				{
					Utils.PortManager.Instance.ReleasePort(port);
				}
			}

			// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
			// TODO: set fields of large size with null value.
		}
		#endregion

		#region Base class implementation
		public override bool Connected()
		{
			return client != null;
		}

		protected override void BeginSend(byte[] data, int size, AsyncCallback on_completed, SendState state) => client.Client.BeginSend(data, 0, size, SocketFlags.None, on_completed, state);
		protected override void BeginReceive(byte[] data, int offset, int size, AsyncCallback on_completed, ReceiveState state) => client.Client.BeginReceive(data, offset, size, SocketFlags.None, on_completed, state);
		protected override void EndSend(IAsyncResult async_result) => client.Client.EndSend(async_result);
		protected override int EndReceive(IAsyncResult async_result) => client.Client.EndReceive(async_result);
		protected override void Flush() { }
		#endregion
	}
}

#endif // !NETFX_CORE
