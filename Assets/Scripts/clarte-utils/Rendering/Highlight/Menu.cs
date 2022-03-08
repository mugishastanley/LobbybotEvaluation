using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CLARTE.Rendering.Highlight
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasGroup))]
    public class Menu : IHighlight
    {
        CanvasGroup m_canvas;

        public override void SetHighlightEnabled(bool enabled)
        {
            if(m_canvas)
            {
                m_canvas.alpha = enabled ? 1 : 0;
                m_canvas.blocksRaycasts = enabled;
                m_canvas.interactable = enabled;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            m_canvas = GetComponent<CanvasGroup>();

            SetHighlightEnabled(false);
        }
    }
}
