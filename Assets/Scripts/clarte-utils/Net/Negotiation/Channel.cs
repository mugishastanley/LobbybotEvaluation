#if !NETFX_CORE

using System;
using UnityEngine;
using UnityEngine.Events;

namespace CLARTE.Net.Negotiation
{
	[Serializable]
	public class ChannelParameters
	{
		#region Members
		[Range(0.1f, 300f)]
		public float heartbeat = 2f; // In seconds
		public bool disableHeartbeat;
		public bool disableAutoReconnect;
		#endregion

		#region Public methods
		public TimeSpan Heartbeat
		{
			get
			{
				if(disableHeartbeat || heartbeat < 0.1f)
				{
					return new TimeSpan(0, 0, 0, 0, -1);
				}
				else
				{
					return new TimeSpan(((long) (heartbeat * 10)) * 100 * TimeSpan.TicksPerMillisecond);
				}
			}
		}
		#endregion
	}

	[Serializable]
	public class BaseChannel
	{
		#region Members
		public Events.ConnectionCallback onConnected;
		public Events.DisconnectionCallback onDisconnected;
		public Events.ExceptionCallback onException;
		#endregion
	}

	[Serializable]
	public class Channel : BaseChannel
	{
		public enum Type : ushort
		{
			TCP,
			UDP,
		}

		#region Members
		public Events.ReceiveCallback onReceive;
		public Events.ReceiveProgressCallback onReceiveProgress;
		#endregion
	}

	[Serializable]
	public class MonitorChannel : BaseChannel
	{

	}

	[Serializable]
	public class ServerMonitorChannel : MonitorChannel
	{
		#region Members
		public ChannelParameters parameters;
		#endregion
	}

	[Serializable]
	public class ServerChannel : Channel
	{
		#region Members
		public Type type;
		public ChannelParameters parameters;
		#endregion
	}
}

#endif // !NETFX_CORE
