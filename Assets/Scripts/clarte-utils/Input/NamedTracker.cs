using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace CLARTE.Input
{
	public class NamedTracker : Tracker
	{
		#region Members
		public string deviceName; // Sample: "OpenVR Controller(Oculus Rift CV1 (Right Controller)) - Right"
		#endregion

		#region Tracker implementation
		protected override bool IsNode(ClarteXRNodeState node)
		{
			return node.name.Trim().ToUpper() == deviceName.Trim().ToUpper();
		}

		protected override bool IsSameNode(ClarteXRNodeState node) {
			return node.name.Trim().ToUpper() == deviceName.Trim().ToUpper();
		}

		protected override void OnNodeAdded(ClarteXRNodeState node)
		{
			Debug.LogFormat("Named tracker '{0}' is associated to object '{1}'", deviceName, gameObject.name);
		}

		protected override void OnNodeRemoved()
		{
			Debug.LogFormat("Named tracker '{0}' is removed from object '{1}'", deviceName, gameObject.name);
		}

		protected override void OnNodeNotFound()
		{
			Debug.LogFormat("Named tracker '{0}' is not connected", deviceName);
		}
		#endregion
	}
}
