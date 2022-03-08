#if !NETFX_CORE

using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using CLARTE.Memory;

namespace CLARTE.Net.Negotiation
{
	public class Server : Base<ServerMonitorChannel, ServerChannel>
	{
		#region Members
		public const uint maxSupportedVersion = 1;

		public TextAsset certificate;
		public ushort port;

		protected Threads.Thread listenerThread;
		protected TcpListener listener;
		protected X509Certificate2 serverCertificate;
		protected ManualResetEvent stopEvent;
		#endregion

		#region IDisposable implementation
		protected override void Dispose(bool disposing)
		{
			if(state != State.DISPOSED)
			{
				state = State.CLOSING;

				if(disposing)
				{
					// TODO: delete managed state (managed objects).

					Disconnect();
				}

				// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
				// TODO: set fields of large size with null value.

				state = State.DISPOSED;
			}
		}
		#endregion

		#region Base implementation
		public override void Disconnect()
		{
			if(state == State.RUNNING || state == State.CLOSING)
			{
				if(listener != null)
				{
					listener.Stop();
				}

				if(stopEvent != null)
				{
					stopEvent.Set();
				}

				CloseInitializedConnections();

				CloseOpenedChannels();

				CloseMonitors();

				if(listenerThread != null)
				{
					listenerThread.Join();
				}

				if(stopEvent != null)
				{
					stopEvent.Close();
				}

				serverCertificate = null;
				listenerThread = null;
				stopEvent = null;

				state = State.STARTED;
			}
		}

		protected override void Reconnect(Connection.Base connection)
		{
			if(connection != null)
			{
				CloseMonitor(connection.Remote);
			}
		}

		protected override void OnMonitorReceive(IPAddress address, Guid guid, ushort channel, BufferPool.Buffer data)
		{
			Message.Base msg = ReceiveMonitorCommand(data);

			if(msg.IsType<Message.Negotiation.Channel.UDP>())
			{
				Message.Negotiation.Channel.UDP response = (Message.Negotiation.Channel.UDP) msg;

				ServerChannel channel_parameters = channels[response.channel];

				Message.Negotiation.Parameters param = new Message.Negotiation.Parameters
				{
					guid = response.guid,
					channel = response.channel,
					type = channel_parameters.type,
					heartbeat = channel_parameters.parameters.Heartbeat,
					autoReconnect = !channel_parameters.parameters.disableAutoReconnect
				};

				Connection.Tcp monitor;

				if(monitors.TryGetValue(param.guid, out monitor))
				{
					UdpConnectionParams udp_param = SendUdpParams(monitor, param);

					ConnectUdp(udp_param, response);
				}
				else
				{
					Debug.LogErrorFormat("No monitor channel with guid '{0}' to send UDP negotiation for channel {1}", param.guid, param.channel);
				}
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected void OnEnable()
		{
			if(state == Base.State.STARTED)
			{
				Listen();
			}
		}

		protected override void OnValidate()
		{
			base.OnValidate();

			if(channels != null)
			{
				foreach(ServerChannel channel in channels)
				{
					if(!channel.parameters.disableHeartbeat && channel.parameters.heartbeat == 0)
					{
						channel.type = Channel.Type.TCP;
						channel.parameters.heartbeat = 2f;
					}
				}
			}
		}
		#endregion

		#region Connection methods
		public void Listen()
		{
			if(state == State.STARTED)
			{
				state = State.INITIALIZING;

				Utils.PortManager.Instance.ReservePort(port);

				stopEvent = new ManualResetEvent(false);

				serverCertificate = null;

				// Should we use an encrypted channel?
				if(certificate != null && certificate.bytes.Length > 0)
				{
					string tmp_file = string.Format("{0}{1}{2}", Application.temporaryCachePath, Path.DirectorySeparatorChar, certificate.name);

					File.WriteAllBytes(tmp_file, certificate.bytes);

					try
					{
#if !UNITY_WSA
						// Import the certificate
						serverCertificate = new X509Certificate2(tmp_file);
#else
						// At the moment, SslStream is not working on Hololens platform.
						// Indeed, at the moment, player capabilities does not provide a way to authorize access to the trusted root certificates store.
						throw new NotSupportedException("SSL streams are not supported on Hololens.");
#endif
					}
					catch(Exception)
					{
						Debug.LogWarningFormat("Invalid certificate file '{0}'. Encryption is disabled.", certificate.name);

						serverCertificate = null;
					}

					File.Delete(tmp_file);
				}

				listener = new TcpListener(IPAddress.Any, port);
				listener.Start();

				listenerThread = new Threads.Thread(() =>
				{
					while(state < State.CLOSING && !stopEvent.WaitOne(0))
					{
						try
						{
							// Listen for new connections
							IAsyncResult context = listener.BeginAcceptTcpClient(AcceptClient, null);

							// Wait for next connection or exit signal
							if(WaitHandle.WaitAny(new[] { stopEvent, context.AsyncWaitHandle }) == 0)
							{
								return;
							}
						}
						catch(InvalidOperationException)
						{
							// Happens when disposing of listener while listening for new connection.
							// Nothing to do, just exit the thread to close cleanly.
						}
					}
				});
				listenerThread.Start();

				Debug.LogFormat("Started server on port {0}", port);

				state = State.RUNNING;
			}
			else
			{
				Debug.LogErrorFormat("Invalid initialization attempt of server when in state {0}.", state);
			}
		}

		protected void AcceptClient(IAsyncResult async_result)
		{
			try
			{
				if(state == State.RUNNING && listener != null && listener.Server != null && listener.Server.IsBound)
				{
					Message.Negotiation.Parameters param = new Message.Negotiation.Parameters
					{
						guid = Guid.Empty,
						channel = ushort.MaxValue,
						heartbeat = negotiation.parameters.Heartbeat,
						autoReconnect = !negotiation.parameters.disableAutoReconnect
					};

					// Get the new connection
					Connection.Tcp connection = new Connection.Tcp(this, param, DisconnectionHandler, listener.EndAcceptTcpClient(async_result));

					lock(initializedConnections)
					{
						initializedConnections.Add(connection);
					}

					connection.initialization = Task.Run(() => Connected(connection));
				}
			}
			catch(ObjectDisposedException)
			{
				// Can happen rarely because the atomicity between IsBound and EndAcceptTcpClient is not verified. Therefore the
				// listener can be disposed in this time frame. In this case, we have nothing to do. Just exit to close cleanly the listener.
			}
			catch(Exception exception)
			{
				Debug.LogErrorFormat("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
			}
		}

		protected void Connected(Connection.Tcp connection)
		{
			try
			{
				// We should be connected
				if(connection.client.Connected)
				{
					// Get the stream associated with this connection
					connection.stream = connection.client.GetStream();

					Message.Connection.Parameters header = new Message.Connection.Parameters
					{
						version = maxSupportedVersion,
						encrypted = (serverCertificate != null)
					};

					// Send greating message with protocol version and parameters
					connection.Send(header);

					if(serverCertificate != null)
					{
						// Create the SSL wraping stream
						connection.stream = new SslStream(connection.stream, false, new RemoteCertificateValidationCallback(
							(sender, certificate, chain, sslPolicyErrors) => (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable) == SslPolicyErrors.RemoteCertificateNotAvailable
						), null, EncryptionPolicy.RequireEncryption);

						// Authenticate with the client
						((SslStream) connection.stream).BeginAuthenticateAsServer(serverCertificate, Authenticated, connection);
					}
					else
					{
						// No encryption, the channel stay as is
						ValidateCredentials(connection);
					}	
				}
				else
				{
					Debug.LogError("The connection from the client failed.");
				}
			}
			catch(DropException)
			{
				throw;
			}
			catch(Exception exception)
			{
				Debug.LogErrorFormat("{0}: {1}\n{2}", exception.GetType(), exception.Message, exception.StackTrace);
			}
		}

		protected void Authenticated(IAsyncResult async_result)
		{
			Connection.Tcp connection = null;

			try
			{
				// Finalize the authentication as server for the SSL stream
				connection = (Connection.Tcp) async_result.AsyncState;

				((SslStream) connection.stream).EndAuthenticateAsServer(async_result);

				ValidateCredentials(connection);
			}
			catch(DropException)
			{
				throw;
			}
			catch(Exception)
			{
				Drop(connection, "Authentication failed.");
			}
		}

		protected void ValidateCredentials(Connection.Tcp connection)
		{
			Message.Base req;

			if(connection.Receive(out req) && req.IsType<Message.Connection.Request>())
			{
				Message.Connection.Request request = (Message.Connection.Request) req;

				connection.version = request.version;

				if(connection.version < maxSupportedVersion)
				{
					Debug.LogWarningFormat("Client does not support protocol version '{0}'. Using version '{1}' instead.", maxSupportedVersion, connection.version);
				}

				Message.Connection.Validation validation = new Message.Connection.Validation();

				// Check if the credentials are valid
				if(request.username == credentials.username && request.password == credentials.password)
				{
					validation.accepted = true;

					// Notify the client that the credentials are valid
					connection.Send(validation);

					NegotiateChannels(connection);
				}
				else
				{
					string error_message = string.Format("Invalid connection credentials for user '{0}'. Dropping connection.", request.username);

					Debug.LogWarning(error_message);

					validation.accepted = false;

					// Notify the client that the credentials are wrong
					connection.Send(validation);

					// Drop the connection
					Close(connection);

					throw new DropException(error_message);
				}
			}
			else
			{
				Drop(connection, "Expected to receive negotiation connection request.");
			}
		}

		protected void NegotiateChannels(Connection.Tcp connection)
		{
			Message.Base msg;

			if(connection.Receive(out msg))
			{
				if(msg.IsType<Message.Negotiation.Start>())
				{
					connection.Parameters.guid = Guid.NewGuid();

					SaveMonitor(connection);

					// Send a new Guid for these connections and the number of associated channels
					Message.Negotiation.New n = new Message.Negotiation.New
					{
						guid = connection.Parameters.guid,
						nbChannels = (ushort) Math.Min(channels != null ? channels.Count : 0, ushort.MaxValue)
					};

					connection.Send(n);

					if(n.nbChannels <= 0)
					{
						Drop(connection, "No channels configured.");
					}

					for(ushort i = 0; i < n.nbChannels; i++)
					{
						ServerChannel channel = channels[i];

						Message.Negotiation.Parameters param = new Message.Negotiation.Parameters
						{
							guid = n.guid,
							channel = i,
							type = channel.type,
							heartbeat = channel.parameters.Heartbeat,
							autoReconnect = !channel.parameters.disableAutoReconnect
						};

						connection.Send(param);
					}
				}
				else if(msg.IsType<Message.Negotiation.Channel.TCP>())
				{
					Message.Negotiation.Channel.TCP tcp = (Message.Negotiation.Channel.TCP) msg;

					connection.SetConfig(tcp.guid, tcp.channel, channels[tcp.channel].parameters.Heartbeat);

					SaveChannel(connection);
				}
				else
				{
					Drop(connection, "Unsupported negotiation command '{0}'.", msg.GetType());
				}
			}
			else
			{
				Drop(connection, "Expected to receive some negotiation command.");
			}
		}
		#endregion
	}
}

#endif // !NETFX_CORE
