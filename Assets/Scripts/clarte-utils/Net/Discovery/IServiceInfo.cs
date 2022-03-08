using CLARTE.Serialization;

namespace CLARTE.Net.Discovery
{
	public abstract class IServiceInfo : IBinaryTypeMapped
	{
		#region Members
		protected bool active;
		#endregion

		#region Abstract methods
		public abstract uint BytesToInfo(Binary serializer, Binary.Buffer buffer, uint start);
		public abstract uint InfoToBytes(Binary serializer, ref Binary.Buffer buffer, uint start);
		#endregion

		#region Getters / Setters
		public bool Active
		{
			get
			{
				return active;
			}

			set
			{
				active = value;
			}
		}
		#endregion

		#region IBinaryTypeMapped implementation
		public uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			uint read = 0;

			read += serializer.FromBytes(buffer, start + read, out active);

			read += BytesToInfo(serializer, buffer, start + read);

			return read;
		}

		public uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			uint written = 0;

			written += serializer.ToBytes(ref buffer, start + written, active);

			written += InfoToBytes(serializer, ref buffer, start + written);

			return written;
		}
		#endregion
	}
}
