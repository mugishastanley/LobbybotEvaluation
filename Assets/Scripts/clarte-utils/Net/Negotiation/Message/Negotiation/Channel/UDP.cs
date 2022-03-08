#if !NETFX_CORE

using System;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message.Negotiation.Channel
{
	public class UDP : Base
	{
		#region Members
		public const uint messageSize = guidSize + 2 * sizeof(ushort);

		public Guid guid;
		public ushort channel;
		public ushort port;
		#endregion

		#region IBinarySerializable implementation
		public override uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			uint read = 0;

			byte[] raw_guid;

			read += serializer.FromBytes(buffer, start + read, out raw_guid);
			read += serializer.FromBytes(buffer, start + read, out channel);
			read += serializer.FromBytes(buffer, start + read, out port);

			guid = new Guid(raw_guid);

			return read;
		}

		public override uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			uint written = 0;

			serializer.ResizeBuffer(ref buffer, start + messageSize);

			written += serializer.ToBytes(ref buffer, start + written, guid.ToByteArray());
			written += serializer.ToBytes(ref buffer, start + written, channel);
			written += serializer.ToBytes(ref buffer, start + written, port);

			return written;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
