#if !NETFX_CORE

using System;
using System.Net;
using UnityEngine.Events;
using CLARTE.Memory;
using CLARTE.Serialization;

namespace CLARTE.Net.Negotiation
{
	public static class Events
	{
		[Serializable]
		public class ConnectionCallback : UnityEvent<IPAddress, Guid, ushort>
		{

		}

		[Serializable]
		public class DisconnectionCallback : UnityEvent<IPAddress, Guid, ushort>
		{

		}

		[Serializable]
		public class ExceptionCallback : UnityEvent<IPAddress, Guid, ushort, Exception>
		{

		}

		[Serializable]
		public class ReceiveCallback : UnityEvent<IPAddress, Guid, ushort, BufferPool.Buffer>
		{

		}

		[Serializable]
		public class ReceiveDeserializedCallback : UnityEvent<IPAddress, Guid, ushort, IBinarySerializable>
		{

		}

		[Serializable]
		public class ReceiveProgressCallback : UnityEvent<IPAddress, Guid, ushort, float>
		{

		}
	}
}

#endif // !NETFX_CORE
