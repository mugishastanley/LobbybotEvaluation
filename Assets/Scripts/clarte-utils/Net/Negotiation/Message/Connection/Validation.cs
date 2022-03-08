#if !NETFX_CORE

using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message.Connection
{
	public class Validation : Base
	{
		#region Members
		public bool accepted;
		#endregion

		#region IBinarySerializable implementation
		public override uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			uint read = 0;

			read += serializer.FromBytes(buffer, start + read, out accepted);

			return read;
		}

		public override uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			uint written = 0;

			const uint message_size = sizeof(bool);

			serializer.ResizeBuffer(ref buffer, start + message_size);

			written += serializer.ToBytes(ref buffer, start + written, accepted);

			return written;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
