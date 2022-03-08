#if !NETFX_CORE

using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message.Connection
{
	public class Request : Base
	{
		#region Members
		public uint version;
		public string username;
		public string password;
		#endregion

		#region IBinarySerializable implementation
		public override uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			uint read = 0;

			read += serializer.FromBytes(buffer, start + read, out version);
			read += serializer.FromBytes(buffer, start + read, out username);
			read += serializer.FromBytes(buffer, start + read, out password);

			return read;
		}

		public override uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			uint written = 0;

			uint message_size = (uint) (3 * sizeof(uint) + username.Length + password.Length);

			serializer.ResizeBuffer(ref buffer, start + message_size);

			written += serializer.ToBytes(ref buffer, start + written, version);
			written += serializer.ToBytes(ref buffer, start + written, username);
			written += serializer.ToBytes(ref buffer, start + written, password);

			return written;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
