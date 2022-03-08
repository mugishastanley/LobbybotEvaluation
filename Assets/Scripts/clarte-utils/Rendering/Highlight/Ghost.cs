using UnityEngine;

namespace CLARTE.Rendering.Highlight
{
    public class Ghost : IHighlight
    {
        public enum BlendMode
        {
            OPAQUE,
            CUTOUT,
            FADE,
            TRANSPARENT
        }

        #region Members
        public GameObject ghost;
        [Range(0f, 1f)]
        public float opacity = 0.2f;
        [Range(0f, 1f)]
        public float saturation = 0.2f;
        #endregion

        #region IHighlight implementation
        public override void SetHighlightEnabled(bool enabled)
        {
            ghost?.SetActive(enabled);
        }
        #endregion

        #region MonoBehaviour callbacks
        protected void Awake()
        {
            Shader shader = null;

            if (ghost != null)
            {
                foreach (MeshRenderer renderer in ghost.GetComponentsInChildren<MeshRenderer>())
                {
                    if (!shader)
                        shader = renderer.material.shader;
                    foreach (Material material in renderer.materials)
                    {
                        material.shader = shader;

                        Color c = material.color;

                        c.r *= saturation;
                        c.g *= saturation;
                        c.b *= saturation;
                        c.a *= opacity;

                        material.color = c;

                        SetupMaterialWithBlendMode(material, BlendMode.FADE);
                    }
                }

                ghost.SetActive(false);
            }
        }
        #endregion

        #region Internal methods
        private static void SetupMaterialWithBlendMode(Material material, BlendMode blend_mode)
        {
            material.SetFloat("_Mode", (float)blend_mode);

            switch (blend_mode)
            {
                case BlendMode.OPAQUE:
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.CUTOUT:
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 0);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 2450;
                    break;
                case BlendMode.FADE:
                    material.SetInt("_SrcBlend", 5);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
                case BlendMode.TRANSPARENT:
                    material.SetInt("_SrcBlend", 1);
                    material.SetInt("_DstBlend", 10);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = 3000;
                    break;
            }
        }
        #endregion
    }
}
