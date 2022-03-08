using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

namespace CLARTE.Input
{
	public abstract class Tracker : MonoBehaviour
	{
		#region Members
		public bool ShowNodes = false;
		public bool InverseTransform = false;
		protected List<ClarteXRNodeState> nodes;
#if UNITY_2019_3_OR_NEWER
		protected InputDeviceCharacteristics currentType;
#else
		protected XRNode currentType;
#endif
		protected ulong uniqueID;
		protected bool tracked;
		protected bool firstCheck;
#endregion

#region Abstract methods
		protected abstract bool IsNode(ClarteXRNodeState node);
		protected abstract bool IsSameNode(ClarteXRNodeState node);
		protected abstract void OnNodeAdded(ClarteXRNodeState node);
		protected abstract void OnNodeRemoved();
		protected abstract void OnNodeNotFound();
#endregion

#region Getter / Setter
		public bool Tracked
		{
			get
			{
				return tracked;
			}

			protected set
			{
				tracked = value;

				EnableComponents(tracked);
			}
		}

		public ulong UniqueID
		{
			get { return uniqueID; }
		}
#endregion

#region MonoBehaviour callbacks
		protected virtual void Awake()
		{
			nodes = new List<ClarteXRNodeState>();

			uniqueID = 0;

			Tracked = false;
			firstCheck = true;
		}

		protected virtual void OnDisable()
		{
			RemoveNode();
		}

		protected virtual void Update()
		{
			ClarteXRNodeState.GetNodeStates(nodes);

			if (ShowNodes) {
				string log = "Devices:";
				foreach (ClarteXRNodeState nd in nodes) {
					log += "\n" + nd.nodeType + " " + nd.uniqueID + " '" + nd.name +"'";
				}
				Debug.Log(log);
				ShowNodes = false;
			}

			if (uniqueID == 0)
			{
				SearchValidNode(nodes);
			}
			else if(!CheckConnectedNode(nodes))
			{
				RemoveNode();
				SearchValidNode(nodes);
			}
  
			if(uniqueID != 0)
			{
				foreach(ClarteXRNodeState node in nodes)
				{
					if(node.uniqueID == uniqueID)
					{
						if(node.tracked != Tracked)
						{
							Tracked = node.tracked;
						}

						if(Tracked)
						{
							Vector3 pos;
							Quaternion rot;

							if (node.TryGetPosition(out pos))
							{
								if (node.TryGetRotation(out rot))
								{
									if (InverseTransform)
									{
										rot = Quaternion.Inverse(rot);
										pos = rot * -pos;
									}
									transform.localRotation = rot;
									transform.localPosition = pos;
								}
								else
								{
									if (InverseTransform)
									{
										pos = -pos;
									}
									transform.localPosition = pos;
								}
							}
						}
						break;
					}
				}
			}
		}
#endregion

#region Helper methods
		protected bool CheckConnectedNode(List<ClarteXRNodeState> nodes)
		{
			bool found = false;

			foreach(ClarteXRNodeState node in nodes)
			{
				if(node.uniqueID == uniqueID)
				{
					found = (node.nodeType == currentType) && IsSameNode(node);

					break;
				}
			}

			return found;
		}

		protected void SearchValidNode(List<ClarteXRNodeState> nodes)
		{
			foreach(ClarteXRNodeState node in nodes)
			{
				if(uniqueID == 0 && IsNode(node))
				{
					uniqueID = node.uniqueID;
					currentType = node.nodeType;

					Tracked = node.tracked;

					OnNodeAdded(node);

					break;
				}
			}

			if (uniqueID == 0 && firstCheck)
			{
				OnNodeNotFound();
				firstCheck = false;
			}
		}

		protected void RemoveNode()
		{
			if(uniqueID != 0)
			{
				OnNodeRemoved();

				uniqueID = 0;
				
			}
		}

		protected void EnableComponents(bool enable)
		{
			foreach(Collider collider in GetComponents<Collider>())
			{
				collider.enabled = enable;
			}

			foreach(Behaviour component in GetComponents<Behaviour>())
			{
				if(component != this)
				{
					component.enabled = enable;
				}
			}

			foreach(Transform child in transform)
			{
				child.gameObject.SetActive(enable);
			}
		}
#endregion
	}
}
