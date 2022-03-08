#if !NETFX_CORE

using System;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message.Negotiation
{
	public class New : Base
	{
		#region Members
		public Guid guid;
		public ushort nbChannels;
		#endregion

		#region IBinarySerializable implementation
		public override uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			uint read = 0;

			byte[] raw_guid;

			read += serializer.FromBytes(buffer, start + read, out raw_guid);
			read += serializer.FromBytes(buffer, start + read, out nbChannels);

			guid = new Guid(raw_guid);

			return read;
		}

		public override uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			uint written = 0;

			const uint message_size = guidSize + sizeof(ushort);

			serializer.ResizeBuffer(ref buffer, start + message_size);

			written += serializer.ToBytes(ref buffer, start + written, guid.ToByteArray());
			written += serializer.ToBytes(ref buffer, start + written, nbChannels);

			return written;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
