using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.Rendering.Highlight
{
    public class GridProjector : IHighlight
    {
        #region Members
        public Projector projector;
        #endregion

        #region IHighlight implementation
        public override void SetHighlightEnabled(bool enabled)
        {
            projector?.gameObject.SetActive(enabled);
        }
        #endregion

        #region MonoBehaviour callbacks
        // Start is called before the first frame update
        void Awake()
        {
            if (projector != null)
            {
                projector?.gameObject.SetActive(false);
            }
        }
        #endregion
    }
}
