#if !NETFX_CORE

using System;
using System.IO;
using System.Net;
using UnityEngine.Events;
using CLARTE.Memory;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Connection
{
    public abstract class Socket : Base
    {
		#region Members
		protected System.Net.Sockets.Socket socket;
		protected BufferPool.Buffer readBuffer;
		protected BufferPool.Buffer writeBuffer;
		#endregion

		#region Abstract methods
		protected abstract void BeginSend(byte[] data, int size, AsyncCallback on_completed, SendState state);
		protected abstract void BeginReceive(byte[] data, int offset, int size, AsyncCallback on_completed, ReceiveState state);
		protected abstract void EndSend(IAsyncResult async_result);
		protected abstract int EndReceive(IAsyncResult async_result);
		protected abstract void Flush();
		#endregion

		#region Constructors
		public Socket(Negotiation.Base parent, Message.Negotiation.Parameters parameters, UnityAction<Base> disconnection_handler, System.Net.Sockets.Socket socket) : base(parent, parameters, disconnection_handler)
		{
			this.socket = socket;

			readBuffer = new BufferPool.Buffer(null, new byte[sizeof(int)], false);
			writeBuffer = new BufferPool.Buffer(null, new byte[sizeof(int)], false);
		}
		#endregion

		#region IDisposable implementation
		protected override void DisposeInternal(bool disposing)
		{
			if (disposing)
			{
				// TODO: delete managed state (managed objects).

				SafeDispose(readBuffer);
				SafeDispose(writeBuffer);
			}

			// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
			// TODO: set fields of large size with null value.
		}
		#endregion

		#region Base class implementation
		public override IPAddress GetRemoteAddress()
		{
			IPAddress address = null;

			if (socket != null)
			{
				address = ((IPEndPoint) socket.RemoteEndPoint).Address;
			}

			return address;
		}

		protected override void SendAsync(Threads.Result result, BufferPool.Buffer data, bool dispose_buffer = true)
		{
			try
			{
				if (Connected())
				{
					Converter32 c = new Converter32(data.Size);

					writeBuffer.Data[0] = c.Byte1;
					writeBuffer.Data[1] = c.Byte2;
					writeBuffer.Data[2] = c.Byte3;
					writeBuffer.Data[3] = c.Byte4;

					BeginSend(writeBuffer.Data, writeBuffer.Data.Length, FinalizeSendLength, new SendState(result, data, dispose_buffer));
				}
				else
				{
					throw new ArgumentNullException("client", "The connection is not opened.");
				}
			}
			catch (Exception e)
			{
				result.Complete(e);
			}
		}

		protected override void ReceiveAsync()
		{
			try
			{
				if (Connected())
				{
					if (parameters.guid != Guid.Empty)
					{
						IPEndPoint ip = (IPEndPoint) socket.RemoteEndPoint;

						ReceiveState state = new ReceiveState(ip);

						state.Set(readBuffer, (uint)readBuffer.Data.Length);

						BeginReceive(state.data.Data, state.offset, state.MissingDataLength, FinalizeReceiveLength, state);
					}
					else
					{
						throw new ArgumentNullException("remote", "The connection remote and channel are not defined.");
					}
				}
				else
				{
					throw new ArgumentNullException("client", "The connection is not opened.");
				}
			}
			catch (Exception e)
			{
				HandleException(e);
			}
		}
		#endregion

		#region Internal methods
		protected void FinalizeSendLength(IAsyncResult async_result)
		{
			SendState state = (SendState)async_result.AsyncState;

			try
			{
				if (state.data.Size > 0)
				{
					EndSend(async_result);

					BeginSend(state.data.Data, (int)state.data.Size, FinalizeSendData, state);
				}
				else
				{
					FinalizeSendData(async_result);
				}
			}
			catch (Exception e)
			{
				state.result.Complete(e);
			}
		}

		protected void FinalizeSendData(IAsyncResult async_result)
		{
			SendState state = (SendState)async_result.AsyncState;

			try
			{
				EndSend(async_result);

				Flush();

				state.result.Complete();
			}
			catch (Exception e)
			{
				state.result.Complete(e);
			}
			finally
			{
				if (state.dispose_buffer)
				{
					state.data?.Dispose();
				}
			}
		}

		protected void FinalizeReceive(IAsyncResult async_result, Action<ReceiveState> callback)
		{
			try
			{
				ReceiveState state = (ReceiveState)async_result.AsyncState;

				int read_length = EndReceive(async_result);

				int missing = state.MissingDataLength;

				state.offset += read_length;

				if (read_length == missing)
				{
					// We got all the data: pass it back to the application
					callback(state);
				}
				else if (read_length == 0)
				{
					// Connection is closed. Dispose of resources
					Threads.APC.MonoBehaviourCall.Instance.Call(Close);
				}
				else if (read_length < missing)
				{
					Threads.APC.MonoBehaviourCall.Instance.Call(() => events.onReceiveProgress.Invoke(address, parameters.guid, parameters.channel, ((float)state.offset) / state.data.Size));

					// Get the remaining data
					BeginReceive(state.data.Data, state.offset, state.MissingDataLength, FinalizeReceiveData, state);
				}
				else
				{
					throw new ProtocolViolationException(string.Format("Received too much bytes from message. Received {0} bytes instead of {1}.", state.offset + read_length, state.data.Size));
				}
			}
			catch (Exception e)
			{
				HandleException(e);
			}
		}

		protected void FinalizeReceiveLength(IAsyncResult async_result)
		{
			FinalizeReceive(async_result, state =>
			{
				Converter32 c = new Converter32(state.data.Data[0], state.data.Data[1], state.data.Data[2], state.data.Data[3]);

				if (c.Int > 0)
				{
					state.Set(parent.Serializer.GetBuffer((uint)c.Int), (uint)c.Int);

					BeginReceive(state.data.Data, state.offset, state.MissingDataLength, FinalizeReceiveData, state);
				}
				else // Notning to read anymore
				{
					state.Set(readBuffer, (uint)readBuffer.Data.Length);

					// Wait for next message
					BeginReceive(state.data.Data, state.offset, state.MissingDataLength, FinalizeReceiveLength, state);
				}
			});
		}

		protected void FinalizeReceiveData(IAsyncResult async_result)
		{
			FinalizeReceive(async_result, state =>
			{
				BufferPool.Buffer data = state.data; // Otherwise the call to state.data in unity thread will be evaluated to null, because of the weird catching of parameters of lambdas

				Threads.APC.MonoBehaviourCall.Instance.Call(() => events.onReceive.Invoke(state.ip.Address, parameters.guid, parameters.channel, data));

				state.Set(readBuffer, (uint)readBuffer.Data.Length);

				// Wait for next message
				BeginReceive(state.data.Data, state.offset, state.MissingDataLength, FinalizeReceiveLength, state);
			});
		}
		#endregion
	}
}

#endif // !NETFX_CORE
