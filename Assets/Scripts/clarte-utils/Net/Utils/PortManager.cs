using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;

namespace CLARTE.Net.Utils
{
	public class PortManager : Pattern.Singleton<PortManager>
	{
		[Serializable]
		public class Range
		{
			#region Members
			// Avoid IANA system or well-known ports that requires admin privileges
			public const ushort minAvailablePort = 4096;
			public const ushort maxAvailablePort = 65535;
			public const ushort maxRangeSize = 1024;

			public ushort minPort = minAvailablePort;
			public ushort maxPort = maxAvailablePort;
			#endregion
		}

		#region Members
		protected HashSet<ushort> reservedPorts;
		protected HashSet<ushort> occupiedPorts;
		protected HashSet<ushort> availablePorts;
		#endregion

		#region MonoBehaviour callbacks
		protected void Awake()
		{
			reservedPorts = new HashSet<ushort>();
			occupiedPorts = new HashSet<ushort>();
			availablePorts = new HashSet<ushort>();
		}
		#endregion

		#region Public methods
		public bool ReservePort(ushort port)
		{
			bool result;

			lock (reservedPorts)
			{
				result = reservedPorts.Add(port);
			}

			return result;
		}

		public ushort? ReserveRandomPort(IEnumerable<Range> constraint = null)
		{
			ushort? port = null;

			lock (availablePorts)
			{
				lock (reservedPorts)
				{
					lock (occupiedPorts)
					{
						GetAvailablePorts(constraint);

						if (availablePorts.Count > 0)
						{
							port = availablePorts.ElementAt(UnityEngine.Random.Range(0, availablePorts.Count - 1));

							reservedPorts.Add(port.Value);
						}
					}
				}
			}

			return port;
		}	

		public bool ReleasePort(ushort port)
		{
			bool result;

			lock (reservedPorts)
			{
				result = reservedPorts.Remove(port);
			}

			return result;
		}

		public bool IsPortAvailable(ushort port)
		{
			bool result;

			lock (reservedPorts)
			{
				result = !reservedPorts.Contains(port);
			}

			lock (occupiedPorts)
			{
				GetOccupiedPorts();

				result = result && !occupiedPorts.Contains(port);
			}

			return result;
		}
		#endregion

		#region Internal methods
		protected void GetOccupiedPorts()
		{
			occupiedPorts.Clear();

			IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

			occupiedPorts.UnionWith(properties.GetActiveTcpListeners().Select(endpoint => (ushort)endpoint.Port));
			occupiedPorts.UnionWith(properties.GetActiveUdpListeners().Select(endpoint => (ushort)endpoint.Port));
		}

		protected void GetAvailablePorts(IEnumerable<Range> constraint = null)
		{
			GetOccupiedPorts();

			availablePorts.Clear();

			if (constraint != null)
			{
				foreach (Range range in constraint)
				{
					AddRangeToAvailablePorts(range);
				}
			}
			else
			{
				AddRangeToAvailablePorts(new Range());
			}

			foreach (ushort p in reservedPorts)
			{
				availablePorts.Remove(p);
			}

			foreach (ushort p in occupiedPorts)
			{
				availablePorts.Remove(p);
			}
		}

		protected void AddRangeToAvailablePorts(Range range)
		{
			ushort min = Math.Min(range.minPort, range.maxPort);
			ushort max = Math.Max(range.minPort, range.maxPort);

			int dist = max - min;

			if (dist + 1 > Range.maxRangeSize)
			{
				min += (ushort)UnityEngine.Random.Range(0, dist - Range.maxRangeSize);
				max = (ushort)(min + Range.maxRangeSize);
			}

			for (ushort i = min; i <= max; i++)
			{
				availablePorts.Add(i);
			}
		}
		#endregion
	}
}
