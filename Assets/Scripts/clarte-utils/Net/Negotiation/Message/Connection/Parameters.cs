#if !NETFX_CORE

using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message.Connection
{
	public class Parameters : Base
	{
		#region Members
		public uint version;
		public bool encrypted;
		#endregion

		#region IBinarySerializable implementation
		public override uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			uint read = 0;

			read += serializer.FromBytes(buffer, start + read, out version);
			read += serializer.FromBytes(buffer, start + read, out encrypted);

			return read;
		}

		public override uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			uint written = 0;

			const uint message_size = sizeof(uint) + sizeof(bool);

			serializer.ResizeBuffer(ref buffer, start + message_size);

			written += serializer.ToBytes(ref buffer, start + written, version);
			written += serializer.ToBytes(ref buffer, start + written, encrypted);

			return written;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
