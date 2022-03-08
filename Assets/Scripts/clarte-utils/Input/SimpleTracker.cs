using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace CLARTE.Input
{
	public class SimpleTracker : Tracker
	{
		#region Members
		protected static Dictionary<ulong, ushort> trackedIds = new Dictionary<ulong, ushort>();

#if UNITY_2019_3_OR_NEWER
		public InputDeviceCharacteristics type;
#else
		public XRNode type;
#endif
		#endregion

		#region Tracker implementation
		protected override bool IsNode(ClarteXRNodeState node)
		{
			return (node.nodeType == type && !(trackedIds.ContainsKey(node.uniqueID) && trackedIds[node.uniqueID] > 0));
		}

		protected override bool IsSameNode(ClarteXRNodeState node) {
			return node.nodeType == type && trackedIds.ContainsKey(node.uniqueID) && trackedIds[node.uniqueID] > 0;
		}

		protected override void OnNodeAdded(ClarteXRNodeState node)
		{
			if(!trackedIds.ContainsKey(node.uniqueID))
			{
				trackedIds.Add(uniqueID, 0);
			}

			trackedIds[uniqueID]++;

			Debug.LogFormat("Tracker '{0}' of type '{1}' is associated to object '{2}'", uniqueID, type, gameObject.name);
		}

		protected override void OnNodeRemoved()
		{
			Debug.LogFormat("Tracker '{0}' of type '{1}' is removed from object '{2}'", uniqueID, type, gameObject.name);

			trackedIds[uniqueID]--;
		}

		protected override void OnNodeNotFound()
		{
			Debug.LogFormat("Tracker '{0}' of type '{1}' is not connected", uniqueID, type);
		}
		#endregion
	}
}
