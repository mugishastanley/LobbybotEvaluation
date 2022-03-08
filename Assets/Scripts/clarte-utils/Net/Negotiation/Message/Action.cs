#if !NETFX_CORE

using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation.Message
{
	public class Action : Base
	{
		#region IBinarySerializable implementation
		public override uint FromBytes(Binary serializer, Binary.Buffer buffer, uint start)
		{
			return 0;
		}

		public override uint ToBytes(Binary serializer, ref Binary.Buffer buffer, uint start)
		{
			return 0;
		}
		#endregion
	}
}

#endif // !NETFX_CORE
