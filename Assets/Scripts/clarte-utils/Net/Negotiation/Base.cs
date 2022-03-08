#if !NETFX_CORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using CLARTE.Memory;
using CLARTE.Net.Utils;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation
{
	public abstract class Base : MonoBehaviour
	{
		public enum State
		{
			STARTED,
			INITIALIZING,
			RUNNING,
			CLOSING,
			DISPOSED
		}

		protected class DropException : Exception
		{
			public DropException(string message) : base(message)
			{

			}
		}

		[Serializable]
		public class Credentials
		{
			#region Members
			public string username;
			public string password;
			#endregion
		}

		protected class UdpConnectionParams
		{
			public Message.Negotiation.Parameters param;
			public UdpClient udp;
			public IPAddress remote;
			public ushort localPort;
		}

		#region Members
		protected static readonly TimeSpan defaultHeartbeat = new TimeSpan(5 * TimeSpan.TicksPerSecond);

		public List<PortManager.Range> openPorts;

		protected Binary serializer;
		protected Dictionary<Guid, Connection.Tcp> monitors;
		protected Dictionary<Guid, Connection.Base[]> openedChannels;
		protected HashSet<Connection.Tcp> initializedConnections;
		protected State state;
		#endregion

		#region Abstract methods
		protected abstract void Dispose(bool disposing);
		protected abstract void Reconnect(Connection.Base connection);
		protected abstract void OnMonitorReceive(IPAddress address, Guid guid, ushort channel, BufferPool.Buffer data);
		public abstract void Disconnect();
		public abstract ushort NbChannels { get; }
		public abstract IEnumerable<Channel> Channels { get; }
		#endregion

		#region Clean-up helpers
		protected void CloseMonitors()
		{
			if(monitors != null)
			{
				foreach(KeyValuePair<Guid, Connection.Tcp> pair in monitors)
				{
					if(pair.Value.initialization != null)
					{
						pair.Value.initialization.Wait();
					}

					pair.Value.Close();
				}

				monitors.Clear();
			}
		}

		protected bool CloseMonitor(Guid guid)
		{
			if(monitors.TryGetValue(guid, out Connection.Tcp monitor) && monitor != null)
			{
				if(monitor.initialization != null)
				{
					monitor.initialization.Wait();
				}

				monitor.Close();

				monitors.Remove(guid);

				return true;
			}

			return false;
		}

		protected void CloseInitializedConnections()
		{
			lock(initializedConnections)
			{
				foreach(Connection.Tcp connection in initializedConnections)
				{
					if(connection != null)
					{
						if(connection.initialization != null)
						{
							connection.initialization.Wait();
						}

						connection.Close();
					}
				}

				initializedConnections.Clear();
			}
		}

		protected void CloseOpenedChannels()
		{
			lock(openedChannels)
			{
				foreach(KeyValuePair<Guid, Connection.Base[]> pair in openedChannels)
				{
					foreach(Connection.Base connection in pair.Value)
					{
						if(connection != null)
						{
							connection.Close();
						}
					}
				}

				openedChannels.Clear();
			}
		}

		protected void Close(Connection.Base connection)
		{
			if(connection != null)
			{
				if(connection is Connection.Tcp)
				{
					lock(initializedConnections)
					{
						initializedConnections.Remove(connection as Connection.Tcp);
					}
				}

				if(connection.Channel != ushort.MaxValue)
				{
					lock(openedChannels)
					{
						Connection.Base[] connections;

						if(openedChannels.TryGetValue(connection.Remote, out connections))
						{
							connections[connection.Channel] = null;
						}

						openedChannels.Remove(connection.Remote);
					}
				}

				connection.Close();
			}
		}

		protected void DisconnectionHandler(Connection.Base connection)
		{
			Close(connection);

			if(state == State.RUNNING)
			{
				Reconnect(connection);
			}
		}

		protected void Drop(Connection.Tcp connection, string message, params object[] values)
		{
			string error_message = string.Format(message, values);

			if(!error_message.EndsWith("."))
			{
				error_message += ".";
			}

			error_message += " Dropping connection.";

			Debug.LogError(error_message);

			Close(connection);

			throw new DropException(error_message);
		}
		#endregion

		#region MonoBehaviour callbacks
		protected virtual void Awake()
		{
			state = State.STARTED;

			// Initialize singletons while in unity thread, if necessary
			PortManager.Instance.GetType();
			Threads.APC.MonoBehaviourCall.Instance.GetType();
			Pattern.Factory<Message.Base, byte>.Initialize(Pattern.Factory.ByteConverter);

			serializer = new Binary();

			monitors = new Dictionary<Guid, Connection.Tcp>();
			openedChannels = new Dictionary<Guid, Connection.Base[]>();

			initializedConnections = new HashSet<Connection.Tcp>();
		}

		protected void OnDisable()
		{
			if(state == State.RUNNING)
			{
				Disconnect();
			}
		}

		protected void OnDestroy()
		{
			Dispose(true);
		}

		protected virtual void OnValidate()
		{
			if(openPorts == null)
			{
				openPorts = new List<PortManager.Range>();
			}

			if(openPorts.Count <= 0)
			{
				openPorts.Add(new PortManager.Range());
			}

			foreach(PortManager.Range range in openPorts)
			{
				if(range.minPort == 0 && range.maxPort == 0)
				{
					range.minPort = PortManager.Range.minAvailablePort;
					range.maxPort = PortManager.Range.maxAvailablePort;
				}
			}
		}
		#endregion

		#region Public methods
		public State CurrentState
		{
			get
			{
				return state;
			}
		}

		public Binary Serializer
		{
			get
			{
				return serializer;
			}
		}

		public bool Ready(Guid remote, ushort channel)
		{
			Connection.Base[] channels;
			bool result;

			lock(openedChannels)
			{
				result = state == State.RUNNING && openedChannels.TryGetValue(remote, out channels) && channel < channels.Length && channels[channel] != null && channels[channel].Connected();
			}

			return result;
		}

		public bool Ready(Guid remote)
		{
			Connection.Base[] channels;
			bool result;

			lock(openedChannels)
			{
				result = state == State.RUNNING && openedChannels.TryGetValue(remote, out channels) && channels.All(x => x != null && x.Connected());
			}

			return result;
		}

		public bool Ready()
		{
			bool result;

			lock(openedChannels)
			{
				result = state == State.RUNNING && openedChannels.All(p => p.Value.All(x => x != null && x.Connected()));
			}

			return result;
		}

		public void Send(Guid remote, ushort channel, BufferPool.Buffer data, bool dispose_buffer = true)
		{
			if(state == State.RUNNING)
			{
				Connection.Base[] client_channels;
				Connection.Base client_channel;

				lock(openedChannels)
				{
					if(!openedChannels.TryGetValue(remote, out client_channels))
					{
						throw new ArgumentException(string.Format("No connection with remote '{0}'. Nothing sent.", remote), "remote");
					}

					if(channel >= client_channels.Length || client_channels[channel] == null)
					{
						throw new ArgumentException(string.Format("Invalid channel. No channel with index '{0}'", channel), "channel");
					}

					client_channel = client_channels[channel];
				}

				client_channel.SendAsync(data, dispose_buffer);
			}
			else
			{
				Debug.LogWarningFormat("Can not send data when in state {0}. Nothing sent.", state);
			}
		}

		public void SendOthers(Guid remote, ushort channel, BufferPool.Buffer data, bool dispose_buffer = true)
		{
			if(state == State.RUNNING)
			{
				lock(openedChannels)
				{
					if (dispose_buffer)
					{
						data.SetReferencesCount((uint)openedChannels.Count);
					}

					foreach (KeyValuePair<Guid, Connection.Base[]> pair in openedChannels)
					{
						if(remote == Guid.Empty || pair.Key != remote)
						{
							if(channel >= pair.Value.Length || pair.Value[channel] == null)
							{
								throw new ArgumentException(string.Format("Invalid channel. No channel with index '{0}'", channel), "channel");
							}

							pair.Value[channel].SendAsync(data, dispose_buffer);
						}
						else
						{
							if (dispose_buffer)
							{
								data.DecrementReferencesCount();
							}
						}
					}
				}
			}
			else
			{
				Debug.LogWarningFormat("Can not send data when in state {0}. Nothing sent.", state);
			}
		}

		public void SendAll(ushort channel, BufferPool.Buffer data, bool dispose_buffer = true)
		{
			SendOthers(Guid.Empty, channel, data, dispose_buffer);
		}
		#endregion
	}

	public abstract class Base<T, U> : Base where T : MonitorChannel where U : Channel
	{
		#region Members
		public T negotiation;
		public List<U> channels;
		public Credentials credentials;
		#endregion

		#region Public methods
		public override ushort NbChannels
		{
			get
			{
				return (ushort) (channels != null ? channels.Count : 0);
			}
		}

		public override IEnumerable<Channel> Channels
		{
			get
			{
				return (IEnumerable<Channel>) channels;
			}
		}
		#endregion

		#region Shared network methods
		protected void SendMonitorCommand(Connection.Tcp connection, Message.Base message, uint message_size = 0)
		{
			Binary.Buffer buffer = serializer.GetBuffer(message_size != 0 ? message_size : 256);

			uint written = serializer.ToBytes(ref buffer, 0, Pattern.Factory<Message.Base, byte>.Get(message.GetType()));

			written += serializer.ToBytes(ref buffer, written, message);

			buffer.Size = written;

			// Send the selected port. A value of 0 means that no port are available.
			connection.SendAsync(buffer, true);
		}

		protected Message.Base ReceiveMonitorCommand(BufferPool.Buffer data)
		{
			const ushort type_nb_bytes = 1;

			Binary.Buffer buffer = new Binary.Buffer(data, serializer);

			Message.Base message = Pattern.Factory<Message.Base, byte>.CreateInstance(buffer.Data[0]);

			uint read = Serializer.FromBytesOverwrite(buffer, type_nb_bytes, message);

			if(read != buffer.Size - type_nb_bytes)
			{
				Debug.LogErrorFormat("Some received data was not read. Read '{0}' bytes instead of '{1}'.", read, buffer.Size - type_nb_bytes);
			}

			return message;
		}

		protected UdpConnectionParams SendUdpParams(Connection.Tcp connection, Message.Negotiation.Parameters param)
		{
			UdpConnectionParams udp_param = new UdpConnectionParams
			{
				param = param,
				udp = null,
				remote = connection.GetRemoteAddress(),
				localPort = 0
			};

			Message.Negotiation.Channel.UDP msg = new Message.Negotiation.Channel.UDP
			{
				guid = param.guid,
				channel = param.channel,
				port = 0
			};

			if(channels != null && param.channel < channels.Count)
			{
				ushort? port = null;

				while (!port.HasValue)
				{
					port = PortManager.Instance.ReserveRandomPort(openPorts);

					if(port.HasValue)
					{
						udp_param.localPort = port.Value;

						try
						{
							udp_param.udp = new UdpClient(udp_param.localPort, AddressFamily.InterNetwork);
						}
						catch(SocketException)
						{
							// Port unavailable. Remove it definitively from the list and try another port.
							udp_param.udp = null;

							udp_param.localPort = 0;

							port = null;
						}
					}
				}
			}

			msg.port = udp_param.localPort;

			SendMonitorCommand(connection, msg, Message.Negotiation.Channel.UDP.messageSize);

			return udp_param;
		}

		protected void ConnectUdp(UdpConnectionParams param, Message.Negotiation.Channel.UDP response)
		{ 
			if(param.udp != null && param.localPort > 0)
			{
				if(response.port > 0)
				{
					param.udp.Connect(param.remote, response.port);

					SaveChannel(new Connection.Udp(this, param.param, DisconnectionHandler, param.udp, param.localPort, response.port));
				}
				else
				{
					Debug.LogError("No available remote port for UDP connection.");
				}
			}
			else
			{
				Debug.LogError("No available local port for UDP connection.");
			}
	
		}

		protected void SaveMonitor(Connection.Tcp connection)
		{
			if(monitors != null)
			{
				if(monitors.ContainsKey(connection.Remote))
				{
					Debug.LogErrorFormat("A monitor channel already exist for guid '{0}'. Replacing the monitor channel.", connection.Remote);

					CloseMonitor(connection.Remote);
				}

				monitors[connection.Remote] = connection;

				connection.SetEvents(negotiation);
				connection.SetEvents(OnMonitorReceive);

				connection.Listen();
			}
		}

		protected void SaveChannel(Connection.Base connection)
		{
			// Remove initialized TCP connection from the pool of connections in initialization
			if(connection is Connection.Tcp)
			{
				lock(initializedConnections)
				{
					initializedConnections.Remove((Connection.Tcp) connection);
				}
			}

			if(connection.Channel < channels.Count)
			{
				Channel channel = channels[connection.Channel];

				// Save callbacks for the connection
				connection.SetEvents(channel);

				// Save the connection
				lock(openedChannels)
				{
					Connection.Base[] client_channels;

					if(!openedChannels.TryGetValue(connection.Remote, out client_channels))
					{
						client_channels = new Connection.Base[channels.Count];

						openedChannels.Add(connection.Remote, client_channels);
					}

					client_channels[connection.Channel] = connection;
				}

				connection.Listen();
			}
			else
			{
				// No channel defined for this index. This should never happen as index are checked during port negotiation
				Debug.LogErrorFormat("No channel defined with index '{0}'.", connection.Channel);

				connection.Close();
			}
		}
		#endregion
	}
}

#endif //!NETFX_CORE
