using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace CLARTE.Input
{
	public class ClarteXRNodeState
	{

		#region Members
#if UNITY_2019_3_OR_NEWER
		InputDevice node;
		ulong hashedId;
#else
		XRNodeState node;
#endif
		#endregion

		#region Getter/Setter
#if UNITY_2019_3_OR_NEWER
		public InputDeviceCharacteristics nodeType
		{
			get
			{
				return node.characteristics;
			}
		}
#else
		public XRNode nodeType {
			get {
				return node.nodeType;
			}
		}
#endif
		public ulong uniqueID {
			get {
#if UNITY_2019_3_OR_NEWER
				return hashedId;
#else
				return node.uniqueID;
#endif
			}
		}
		public string name {
			get {
#if UNITY_2019_3_OR_NEWER
				return node.name;
#else
				return InputTracking.GetNodeName(node.uniqueID);
#endif
			}
		}
		public bool tracked {
			get {
#if UNITY_2019_3_OR_NEWER
				if (node.TryGetFeatureValue(CommonUsages.isTracked, out bool value))
				{
					return value;
				}
				return false;
#else
				return node.tracked;
#endif
			}
		}
#endregion

#region Public Methods
		public bool TryGetPosition(out Vector3 position)
		{
#if UNITY_2019_3_OR_NEWER
			return node.TryGetFeatureValue(CommonUsages.devicePosition, out position);
#else
			return node.TryGetPosition(out position);
#endif
		}

		public bool TryGetRotation(out Quaternion rotation)
		{
#if UNITY_2019_3_OR_NEWER
			return node.TryGetFeatureValue(CommonUsages.deviceRotation, out rotation);
#else
			return node.TryGetRotation(out rotation);
#endif
		}
		#endregion

		#region Static Methods
		public static void GetNodeStates(List<ClarteXRNodeState> nodes)
		{
			nodes.Clear();

#if UNITY_2019_3_OR_NEWER
			List<InputDevice> devices = new List<InputDevice>();
			InputDevices.GetDevices(devices);
			foreach (InputDevice node in devices)
			{
				nodes.Add(new ClarteXRNodeState() { node = node, hashedId = (ulong)node.name.GetHashCode() });
			}
#else
			List<XRNodeState> lnodes = new List<XRNodeState>();
			InputTracking.GetNodeStates(lnodes);
			foreach (XRNodeState node in lnodes)
			{
				nodes.Add(new ClarteXRNodeState() { node = node });
			}
#endif
		}
		#endregion
	}
}
