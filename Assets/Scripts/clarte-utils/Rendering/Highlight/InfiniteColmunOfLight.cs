using UnityEngine;

namespace CLARTE.Rendering.Highlight
{
    public class InfiniteColmunOfLight : IHighlight
    {
        #region Members
        public GameObject column;
        public float minDisplayDist = 4;
        public float speed = 10;
        
        protected Vector3 startScale;
        protected Transform cam;
        #endregion

        #region IHighlight implementation
        public override void SetHighlightEnabled(bool enabled)
        {
            column?.SetActive(enabled);
        }
        #endregion

        #region MonoBehaviour callbacks
        // Start is called before the first frame update
        protected void Awake()
        {
            if (column != null)
            {
                cam = Camera.main.transform;
                column.transform.rotation = Quaternion.identity;
                column.SetActive(false);
                startScale = new Vector3(.1f, 1, .1f);
            }
        }

        protected void Update()
        {
            if (!column || !column.activeSelf)
                return;

            float fact = (cam.position - column.transform.position).magnitude;

            column.transform.localScale = startScale * Mathf.Max(fact * (1 / (1 + Mathf.Exp(speed * (minDisplayDist - fact)))), 0);
        }
        #endregion
    }
}
