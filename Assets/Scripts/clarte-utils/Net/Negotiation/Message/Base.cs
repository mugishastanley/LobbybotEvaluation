#if !NETFX_CORE

using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message
{
	public abstract class Base : IBinarySerializable
	{
		#region Members
		public const uint guidSize = sizeof(uint) + 16;
		#endregion

		#region Abstract methods
		public abstract uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start);
		public abstract uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start);
		#endregion

		#region Public methods
		public bool IsType<T>()
		{
			return typeof(T).IsAssignableFrom(GetType());
		}
		#endregion
	}
}

#endif // !NETFX_CORE
