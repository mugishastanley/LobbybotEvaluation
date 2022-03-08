#if !NETFX_CORE

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine.Events;
using CLARTE.Memory;

namespace CLARTE.Net.Negotiation.Connection
{
	public abstract class Base : IDisposable
	{
		protected class ChannelEvents : Channel
		{
			public class DisonnectionHandlerCallback : UnityEvent<Connection.Base>
			{

			}

			#region Members
			public DisonnectionHandlerCallback onDisconnectedHandler;
			#endregion
		}

		protected struct SendState
		{
			public Threads.Result result;
			public BufferPool.Buffer data;
			public bool dispose_buffer;

			public SendState(Threads.Result result, BufferPool.Buffer data, bool dispose_buffer)
            {
				this.result = result;
				this.data = data;
				this.dispose_buffer = dispose_buffer;
            }
		}

		protected struct ReceiveState
		{
			public IPEndPoint ip;
			public BufferPool.Buffer data;
			public int offset;

			public ReceiveState(IPEndPoint ip)
			{
				this.ip = ip;

				data = null;
				offset = 0;
			}

			public void Set(BufferPool.Buffer data, uint size)
			{
				this.data = data;

				this.data.Size = size;

				offset = 0;
			}

			public int MissingDataLength
			{
				get
				{
					return data != null ? (int) data.Size - offset : 0;
				}
			}
		}

		#region Members
		protected Negotiation.Base parent;
		protected Message.Negotiation.Parameters parameters;
		protected IPAddress address;
		protected ChannelEvents events;
		protected ManualResetEvent stopEvent;
		protected ManualResetEvent addEvent;
		protected Threads.Thread worker;
		protected Threads.Result sendResult;
		protected Queue<Threads.Task> sendQueue;
		protected bool listen;
		private bool disposed;
		#endregion

		#region Abstract methods
		public abstract bool Connected();
		public abstract IPAddress GetRemoteAddress();
		protected abstract void DisposeInternal(bool disposing);
		protected abstract void SendAsync(Threads.Result result, BufferPool.Buffer data, bool dispose_buffer = true);
		protected abstract void ReceiveAsync();
		#endregion

		#region Constructors
		public Base(Negotiation.Base parent, Message.Negotiation.Parameters parameters, UnityAction<Base> disconnection_handler)
		{
			this.parent = parent;
			this.parameters = parameters;

			events = new ChannelEvents();
			events.onDisconnectedHandler = new ChannelEvents.DisonnectionHandlerCallback();
			events.onDisconnectedHandler.AddListener(disconnection_handler);

			sendResult = null;

			sendQueue = new Queue<Threads.Task>();

			stopEvent = new ManualResetEvent(false);
			addEvent = new ManualResetEvent(false);

			worker = new Threads.Thread(Worker);
		}
		#endregion

		#region Getter / Setter
		public Message.Negotiation.Parameters Parameters
		{
			get
			{
				return parameters;
			}
		}

		public IPAddress Address
		{
			get
			{
				return address;
			}
		}

		public Guid Remote
		{
			get
			{
				return parameters.guid;
			}
		}

		public ushort Channel
		{
			get
			{
				return parameters.channel;
			}
		}

		public TimeSpan Heartbeat
		{
			get
			{
				return parameters.heartbeat;
			}
		}

		public bool AutoReconnect
		{
			get
			{
				return parameters.autoReconnect;
			}
		}
		#endregion

		#region IDisposable implementation
		protected void Dispose(bool disposing)
		{
			if(!disposed)
			{
				disposed = true;

				lock(sendQueue)
				{
					sendQueue.Clear();
				}

				DisposeInternal(disposing);

				if(disposing)
				{
					// TODO: delete managed state (managed objects).
					stopEvent.Set();

					if(listen)
					{
						worker.Join();
					}
				}

				// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
				// TODO: set fields of large size with null value.

				Threads.APC.MonoBehaviourCall.Instance.Call(() =>
				{
					if(events.onDisconnectedHandler != null)
					{
						events.onDisconnectedHandler.Invoke(this);
					}

					if(events.onDisconnected != null)
					{
						events.onDisconnected.Invoke(address, parameters.guid, parameters.channel);
					}
				});
			}
		}

		// TODO: replace finalizer only if the above Dispose(bool disposing) function as code to free unmanaged resources.
		~Base()
		{
			Dispose(/*false*/);
		}

		/// <summary>
		/// Dispose of the HTTP server.
		/// </summary>
		public void Dispose()
		{
			// Pass true in dispose method to clean managed resources too and say GC to skip finalize in next line.
			Dispose(true);

			// If dispose is called already then say GC to skip finalize on this instance.
			// TODO: uncomment next line if finalizer is replaced above.
			GC.SuppressFinalize(this);
		}
		#endregion

		#region Public methods
		public void SetConfig(Guid remote, ushort channel, TimeSpan heartbeat)
		{
			parameters.guid = remote;
			parameters.channel = channel;
			parameters.heartbeat = heartbeat;
		}

		public void SetEvents(BaseChannel channel)
		{
			events.onConnected = channel.onConnected;
			events.onDisconnected = channel.onDisconnected;
			events.onException = channel.onException;
		}

		public void SetEvents(Channel channel)
		{
			SetEvents((BaseChannel) channel);

			events.onReceive = channel.onReceive;
			events.onReceiveProgress = channel.onReceiveProgress;
		}

		public void SetEvents(UnityAction<IPAddress, Guid, ushort, BufferPool.Buffer> on_receive)
		{
			events.onReceive = new Events.ReceiveCallback();
			events.onReceive.AddListener(on_receive);
		}

		public void Close()
		{
			if(!disposed)
			{
				Dispose();
			}
		}

		public void Listen()
		{
			if(!listen)
			{
				listen = true;

				address = GetRemoteAddress();

				ReceiveAsync();

				worker.Start();

				Threads.APC.MonoBehaviourCall.Instance.Call(() => events.onConnected.Invoke(address, parameters.guid, parameters.channel));
			}
		}

		public void SendAsync(BufferPool.Buffer data, bool dispose_buffer = true)
		{
			if(!disposed)
			{
				lock(sendQueue)
				{
					Threads.Result result = CreateResult();

					sendQueue.Enqueue(new Threads.Task(() => SendAsync(result, data, dispose_buffer), result));
				}

				lock(addEvent)
				{
					addEvent.Set();
				}
			}
		}
		#endregion

		#region Helper functions
		public static void SafeDispose<T>(T value) where T : IDisposable
		{
			try
			{
				value?.Dispose();
			}
			catch(ObjectDisposedException)
			{
				// Already done
			}
		}

		protected Threads.Result CreateResult()
		{
			return new Threads.Result(e =>
			{
				lock(addEvent)
				{
					addEvent.Set();
				}

				HandleException(e);
			});
		}

		protected void HandleException(Exception e)
		{
			if(e != null)
			{
				Type type = e.GetType();

				if(typeof(System.IO.IOException).IsAssignableFrom(type) || typeof(System.Net.Sockets.SocketException).IsAssignableFrom(type))
				{
					Close();
				}
				else if(typeof(ObjectDisposedException).IsAssignableFrom(type))
				{
					// Nothing to do, the connection was closed and the receiving methods are shutting down
				}
				else
				{
					Threads.APC.MonoBehaviourCall.Instance.Call(() => events.onException.Invoke(address, parameters.guid, parameters.channel, e));
				}
			}
		}
		#endregion

		#region Thread background worker
		protected void Worker()
		{
			WaitHandle[] wait = new WaitHandle[] { stopEvent, addEvent };

			BufferPool.Buffer heartbeat_data = new BufferPool.Buffer(null, new byte[0], false);

            int event_idx;

			while((event_idx = WaitHandle.WaitAny(wait, parameters.heartbeat)) != 0)
			{
				if(event_idx == WaitHandle.WaitTimeout)
				{
					// Handle heartbeat
					if(sendResult == null || sendResult.Done)
					{
						sendResult = CreateResult();

						SendAsync(sendResult, heartbeat_data, false);
					}
				}
				else
				{
					Threads.Task task = null;

					lock(addEvent)
					{
						if(sendResult == null || sendResult.Done)
						{
							sendResult = null;

							lock(sendQueue)
							{
								if(sendQueue.Count > 0)
								{
									task = sendQueue.Dequeue();
								}
								else
								{
									// Nothing to do anymore, go to sleep
									addEvent.Reset();
								}
							}
						}
						else
						{
							// Not done yet, go to sleep
							addEvent.Reset();
						}
					}

					if(task != null)
					{
						sendResult = (Threads.Result) task.result;

						task.callback();
					}
				}
			}
		}
		#endregion
	}
}

#endif // !NETFX_CORE
