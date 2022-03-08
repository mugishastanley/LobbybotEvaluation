using UnityEngine;

namespace CLARTE.Rendering.Highlight
{
	public abstract class IHighlight : MonoBehaviour
	{
		#region Abstract methods
		public abstract void SetHighlightEnabled(bool enabled);
		#endregion
	}
}
