#if !NETFX_CORE

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using UnityEngine;
using CLARTE.Memory;

namespace CLARTE.Net.Negotiation
{
	public class Client : Base<MonitorChannel, Channel>
	{
		[Serializable]
		public class CertificateValidation
		{
			public bool allowSelfSigned = false;
			public bool allowInvalidHostname = false;
		}

		#region Members
		public const uint maxSupportedVersion = 1;

		public CertificateValidation certificateValidation;
		public string hostname = "localhost";
		public ushort port;

		protected HashSet<UdpConnectionParams> pendingUdpConnection;
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
				CloseInitializedConnections();

				CloseOpenedChannels();

				CloseMonitors();

				lock(pendingUdpConnection)
				{
					pendingUdpConnection.Clear();
				}

				state = State.STARTED;
			}
		}

		protected override void Reconnect(Connection.Base connection)
		{
			if(state == State.RUNNING && connection != null && connection.AutoReconnect)
			{
				if(connection != null && monitors.ContainsKey(connection.Remote))
				{
					Disconnect();

					Connect();
				}
				else
				{
					Type type = connection.GetType();

					if(typeof(Connection.Tcp).IsAssignableFrom(type))
					{
						ConnectTcp(connection.Parameters);
					}
					else if(typeof(Connection.Udp).IsAssignableFrom(type))
					{
						if(monitors.TryGetValue(connection.Remote, out Connection.Tcp monitor))
						{
							ConnectUdp(monitor, connection.Parameters);
						}
						else
						{
							Debug.LogErrorFormat("No monitor channel with guid '{0}' to reconnect UDP channel {1}", connection.Remote, connection.Channel);
						}
					}
				}
			}
		}

		protected override void OnMonitorReceive(IPAddress address, Guid guid, ushort channel, BufferPool.Buffer data)
		{
			Message.Base msg = ReceiveMonitorCommand(data);

			if(msg.IsType<Message.Negotiation.Channel.UDP>())
			{
				Message.Negotiation.Channel.UDP response = (Message.Negotiation.Channel.UDP) msg;

				UdpConnectionParams udp_params = null;

				lock(pendingUdpConnection)
				{
					foreach(UdpConnectionParams pending_udp_params in pendingUdpConnection)
					{
						if(pending_udp_params.param.guid == response.guid && pending_udp_params.param.channel == response.channel)
						{
							udp_params = pending_udp_params;

							break;
						}
					}

					if(udp_params != null)
					{
						pendingUdpConnection.Remove(udp_params);
					}
				}

				if(udp_params != null)
				{
					ConnectUdp(udp_params, (Message.Negotiation.Channel.UDP) msg);
				}
				else
				{
					Debug.LogError("Reiceved UDP connection parameters for unrequested connection.");
				}
			}
		}
		#endregion

		#region MonoBehaviour callbacks
		protected override void Awake()
		{
			pendingUdpConnection = new HashSet<UdpConnectionParams>();

			base.Awake();
		}

		protected void OnEnable()
		{
			if(state == Base.State.STARTED && !string.IsNullOrEmpty(hostname) && port != 0)
			{
				Connect();
			}
		}
		#endregion

		#region Public methods
		public void Connect()
		{
			if(state == State.STARTED)
			{
				state = State.INITIALIZING;

				ConnectTcp(new Message.Negotiation.Parameters
				{
					guid = Guid.Empty,
					channel = ushort.MaxValue,
					heartbeat = defaultHeartbeat,
					autoReconnect = false
				});
			}
			else
			{
				Debug.LogErrorFormat("Invalid connection attempt to server when in state {0}.", state);
			}
		}
		#endregion

		#region Connection methods
		protected void ConnectTcp(Message.Negotiation.Parameters param)
		{
			// Create a new TCP client
			Connection.Tcp connection = new Connection.Tcp(this, param, DisconnectionHandler, new TcpClient());

			if(param.guid != Guid.Empty)
			{
				lock(initializedConnections)
				{
					initializedConnections.Add(connection);
				}
			}

			// Start asynchronous connection to server
			connection.initialization = Task.Run(() => connection.client.BeginConnect(hostname, port, Connected, connection));
		}

		protected void Connected(IAsyncResult async_result)
		{
			try
			{
				// Finalize connection to server
				Connection.Tcp connection = (Connection.Tcp) async_result.AsyncState;

				connection.client.EndConnect(async_result);

				// We should be connected
				if(connection.client.Connected)
				{
					// Get the stream associated with this connection
					connection.stream = connection.client.GetStream();

					Message.Base h;

					if(connection.Receive(out h) && h.IsType<Message.Connection.Parameters>())
					{
						Message.Connection.Parameters header = (Message.Connection.Parameters) h;

						connection.version = header.version;

						if(connection.version > maxSupportedVersion)
						{
							Debug.LogWarningFormat("Usupported protocol version '{0}'. Using version '{1}' instead.", connection.version, maxSupportedVersion);

							connection.version = maxSupportedVersion;
						}

						if(header.encrypted)
						{
							// Create the SSL wraping stream
							connection.stream = new SslStream(connection.stream, false, new RemoteCertificateValidationCallback(ValidateServerCertificate), null, EncryptionPolicy.RequireEncryption);

							// Authenticate with the server
							((SslStream) connection.stream).BeginAuthenticateAsClient(hostname, Authenticated, connection);
						}
						else
						{
							// No encryption, the channel stay as is
							ValidateCredentials(connection);
						}
					}
					else
					{
						Drop(connection, "Expected to receive connection greetings and parameters.");
					}
				}
				else
				{
					Drop(connection, "The connection to {0}:{1} failed.", hostname, port);
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
			try
			{
				// Finalize the authentication as client for the SSL stream
				Connection.Tcp connection = (Connection.Tcp) async_result.AsyncState;

				((SslStream) connection.stream).EndAuthenticateAsClient(async_result);

				ValidateCredentials(connection);
			}
			catch(DropException)
			{
				throw;
			}
			catch(Exception)
			{
				Debug.LogError("Authentication failed");
			}
		}

		protected void ValidateCredentials(Connection.Tcp connection)
		{
			Message.Connection.Request request = new Message.Connection.Request
			{
				version = connection.version,
				username = credentials.username,
				password = credentials.password
			};

			connection.Send(request);

			Message.Base v;

			if(connection.Receive(out v) && v.IsType<Message.Connection.Validation>())
			{
				Message.Connection.Validation validation = (Message.Connection.Validation) v;

				// Check if the sent credentials are OK
				if(validation.accepted)
				{
					NegotiateChannels(connection);
				}
				else
				{
					Drop(connection, "Invalid credentials.");
				}
			}
			else
			{
				Drop(connection, "Expected to receive credentials validation.");
			}
		}

		protected void NegotiateChannels(Connection.Tcp connection)
		{
			// Check if we must negotiate other channel or just open the current one
			if(connection.Remote == Guid.Empty)
			{
				connection.Send(new Message.Negotiation.Start());

				Message.Base msg;

				if(connection.Receive(out msg) && msg.IsType<Message.Negotiation.New>())
				{
					Message.Negotiation.New n = (Message.Negotiation.New) msg;

					connection.Parameters.guid = n.guid;

					ushort nb_channels = n.nbChannels;

					if(nb_channels > 0)
					{
						List<Message.Negotiation.Parameters> parameters = new List<Message.Negotiation.Parameters>(nb_channels);

						for(ushort i = 0; i < nb_channels; i++)
						{
							if(connection.Receive(out msg) && msg.IsType<Message.Negotiation.Parameters>())
							{
								Message.Negotiation.Parameters param = (Message.Negotiation.Parameters) msg;

								parameters.Add(param);
							}
							else
							{
								Drop(connection, "Expected to receive channel parameterts for channel {0}.", i);
							}
						}

						SaveMonitor(connection);

						foreach(Message.Negotiation.Parameters param in parameters)
						{
							if(!Ready(param.guid, param.channel))
							{
								switch(param.type)
								{
									case Channel.Type.TCP:
										ConnectTcp(param);
										break;
									case Channel.Type.UDP:
										ConnectUdp(connection, param);
										break;
								}
							}	
						}

						state = State.RUNNING;
					}
					else
					{
						Drop(connection, "No channels configured.");
					}
				}
				else
				{
					Drop(connection, "Expected to receive the new connection negotiation parameters.");
				}
			}
			else
			{
				Message.Negotiation.Channel.TCP tcp = new Message.Negotiation.Channel.TCP
				{
					guid = connection.Remote,
					channel = connection.Channel
				};

				connection.Send(tcp);

				SaveChannel(connection);
			}
		}
		#endregion

		#region Internal methods
		protected bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			SslPolicyErrors handled = SslPolicyErrors.None;

			if(sslPolicyErrors == SslPolicyErrors.None)
			{
				return true;
			}

			if((sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch) != 0)
			{
				if(certificateValidation.allowInvalidHostname)
				{
					Debug.LogWarningFormat("The name of the certificate does not match the hostname. Certificate = '{0}', hostname = '{1}'.", certificate.Subject, hostname);
				}
				else
				{
					return false;
				}

				handled |= SslPolicyErrors.RemoteCertificateNameMismatch;
			}

			if((sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors) != 0)
			{
				if(certificateValidation.allowSelfSigned)
				{
					foreach(X509ChainStatus chainStatus in chain.ChainStatus)
					{
						if(chainStatus.Status != X509ChainStatusFlags.NoError && chainStatus.Status != X509ChainStatusFlags.UntrustedRoot)
						{
							return false;
						}
					}

					Debug.LogWarning("The root certificate is untrusted.");
				}
				else
				{
					return false;
				}

				handled |= SslPolicyErrors.RemoteCertificateChainErrors;
			}

			if((sslPolicyErrors & handled) == handled)
			{
				return true;
			}

			return false;
		}

		protected void ConnectUdp(Connection.Tcp connection, Message.Negotiation.Parameters param)
		{
			UdpConnectionParams udp_params = SendUdpParams(connection, param);

			lock(pendingUdpConnection)
			{
				pendingUdpConnection.Add(udp_params);
			}
		}
		#endregion
	}
}

#endif // !NETFX_CORE
