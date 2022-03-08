#if !NETFX_CORE

using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine.Events;
using CLARTE.Memory;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Connection
{
	public class Tcp : Socket
	{
		#region Members
		protected const ushort headerSize = 5;

		public Task initialization;
		public TcpClient client;
		public Stream stream;
		public uint version;

		protected BufferPool.Buffer headerBuffer;
		#endregion

		#region Constructors
		public Tcp(Negotiation.Base parent, Message.Negotiation.Parameters parameters, UnityAction<Base> disconnection_handler, TcpClient client) : base(parent, parameters, disconnection_handler, client.Client)
		{
			this.client = client;

			stream = null;

			headerBuffer = new BufferPool.Buffer(null, new byte[headerSize], false);
		}
		#endregion

		#region IDisposable implementation
		protected override void DisposeInternal(bool disposing)
		{
			base.DisposeInternal(disposing);

			if(disposing)
			{
				// TODO: delete managed state (managed objects).
				try
				{
					// Flush the stream to make sure that all sent data is effectively sent to the client
					if (stream != null)
					{
						stream.Flush();
					}
				}
				catch (ObjectDisposedException)
				{
					// Already closed
				}

				// Close the stream and client
				SafeDispose(stream);
				SafeDispose(client);

				SafeDispose(headerBuffer);
			}

			// TODO: free unmanaged resources (unmanaged objects) and replace finalizer below.
			// TODO: set fields of large size with null value.
		}
		#endregion

		#region Base class implementation
		public override bool Connected()
		{
			return client != null && stream != null && client.Connected;
		}

		protected override void BeginSend(byte[] data, int size, AsyncCallback on_completed, SendState state) => stream.BeginWrite(data, 0, size, on_completed, state);
		protected override void BeginReceive(byte[] data, int offset, int size, AsyncCallback on_completed, ReceiveState state) => stream.BeginRead(data, offset, size, on_completed, state);
		protected override void EndSend(IAsyncResult async_result) => stream.EndWrite(async_result);
		protected override int EndReceive(IAsyncResult async_result) => stream.EndRead(async_result);
		protected override void Flush() => stream.Flush();
		#endregion

		#region Helper serialization functions
		public void Send(Message.Base message)
		{
			if(stream != null && parent != null)
			{
				Binary.Buffer buffer = parent.Serializer.GetBuffer(256);

				uint written = parent.Serializer.ToBytes(ref buffer, 0, message);

				Converter32 converter = new Converter32(written);

				stream.WriteByte(Pattern.Factory<Message.Base, byte>.Get(message.GetType()));
				stream.WriteByte(converter.Byte1);
				stream.WriteByte(converter.Byte2);
				stream.WriteByte(converter.Byte3);
				stream.WriteByte(converter.Byte4);
				stream.Write(buffer.Data, 0, (int) written);
			}
		}

		public bool Receive(out Message.Base message)
		{
			uint read = 0;
			uint size = 0;

			message = null;

			if(stream != null && parent != null)
			{
				if(ReceiveData(stream, headerBuffer, headerSize))
				{
					message = Pattern.Factory<Message.Base, byte>.CreateInstance(headerBuffer.Data[0]);

					size = new Converter32(headerBuffer.Data[1], headerBuffer.Data[2], headerBuffer.Data[3], headerBuffer.Data[4]);

					Binary.Buffer buffer = parent.Serializer.GetBuffer(size);

					if(ReceiveData(stream, buffer, size))
					{
						read = parent.Serializer.FromBytesOverwrite(buffer, 0, message);
					}
				}
			}

			return (message != null && read == size);
		}

		protected bool ReceiveData(Stream stream, BufferPool.Buffer buffer, uint size)
		{
			int received = 0;

			try
			{
				while(received < size)
				{
					received += stream.Read(buffer.Data, received, (int) (size - received));
				}
			}
			catch(Exception e)
			{
				HandleException(e);

				return false;
			}

			return true;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
