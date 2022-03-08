using System;
using System.Net;
using UnityEngine.Events;

namespace CLARTE.Net.Discovery
{
	public static class Events
	{
		[Serializable]
		public class OnDiscoveredCallback : UnityEvent<IPEndPoint, IServiceInfo>
		{

		}

		[Serializable]
		public class OnLostCallback : UnityEvent<IPEndPoint, IServiceInfo>
		{

		}

		[Serializable]
		public class OnActiveCallback : UnityEvent<IPEndPoint, IServiceInfo>
		{

		}

		[Serializable]
		public class OnInactiveCallback : UnityEvent<IPEndPoint, IServiceInfo>
		{

		}
	}
}
