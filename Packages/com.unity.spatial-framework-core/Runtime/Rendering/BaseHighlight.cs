using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Base abstract class that all highlights derive from. Manages the renderers to be highlighted.
    /// </summary>
    public abstract class BaseHighlight : MonoBehaviour
    {
        [SerializeField, Tooltip("Used to set the mode of capturing renderers on an object or to use only manually set renderers.")]
        protected RendererCaptureDepth m_RendererCaptureDepth = RendererCaptureDepth.AllChildRenderers;

        [SerializeField, Tooltip("Manually set renderers to be affected by the highlight")]
        protected Renderer[] m_ManuallySetRenderers = new Renderer[0];

        /// <summary>
        /// Cached set of renderers including those set manually in the inspector.
        /// </summary>
        protected Renderer[] m_Renderers;

        protected bool m_HighlightActive;

        /// <summary>
        /// Used to set the mode of capturing renderers on an object or to use only manually set renderers.
        /// </summary>
        public RendererCaptureDepth rendererCaptureDepth
        {
            get => m_RendererCaptureDepth;
            set => m_RendererCaptureDepth = value;
        }

        /// <summary>
        /// Manually set renderers to be affected by the highlight
        /// </summary>
        public Renderer[] manuallySetRenderers
        {
            get => m_ManuallySetRenderers;
            set => m_ManuallySetRenderers = value;
        }

        protected virtual void OnEnable()
        {
            if (RefreshHighlightRenderers() < 1)
            {
                enabled = false;
            }
        }

        protected virtual void OnDisable()
        {
            if (m_HighlightActive)
            {
                UnHighlight(true);
            }

            m_HighlightActive = false;
        }

        protected virtual void OnDestroy()
        {
            if (m_HighlightActive)
            {
                UnHighlight(true);
            }

            m_HighlightActive = false;
        }

        /// <summary>
        /// Used to cache and verify number of renderers used in the highlight.
        /// </summary>
        /// <returns>Number of cached highlight renderers</returns>
        protected int RefreshHighlightRenderers()
        {
            var renderers = new HashSet<Renderer>(m_ManuallySetRenderers.Where(r => r != null));
            switch (m_RendererCaptureDepth)
            {
                case RendererCaptureDepth.AllChildRenderers:
                    var getComponentsInChildren = new List<Renderer>();
                    foreach (var meshRenderer in GetComponentsInChildren<MeshRenderer>())
                    {
                        if (meshRenderer.gameObject.GetComponent<TextMesh>() == null)
                            getComponentsInChildren.Add(meshRenderer);
                    }

                    renderers.UnionWith(getComponentsInChildren);
                    renderers.UnionWith(GetComponentsInChildren<SkinnedMeshRenderer>());
                    renderers.UnionWith(GetComponentsInChildren<SpriteRenderer>());
                    break;
                case RendererCaptureDepth.CurrentRenderer:
                    var getComponents = new List<Renderer>();
                    foreach (var meshRenderer in GetComponents<MeshRenderer>())
                    {
                        if (meshRenderer.gameObject.GetComponent<TextMesh>() == null)
                            getComponents.Add(meshRenderer);
                    }

                    renderers.UnionWith(getComponents);
                    renderers.UnionWith(GetComponents<SkinnedMeshRenderer>());
                    renderers.UnionWith(GetComponents<SpriteRenderer>());
                    break;
                case RendererCaptureDepth.ManualOnly:
                    break;
                default:
                    Debug.LogError($"{gameObject.name} highlight has an invalid renderer capture mode {m_RendererCaptureDepth}.", this);
                    enabled = false;
                    break;
            }

            if (renderers.Count > 0)
            {
                m_Renderers = renderers.ToArray();
            }
            else
            {
                m_Renderers = new Renderer[0];
                Debug.LogWarning($"{gameObject.name} highlight has no renderers set.", this);
            }

            OnRenderersRefreshed();
            return m_Renderers.Length;
        }

        /// <summary>
        /// Activates the highlight after refreshing cached renderers.
        /// </summary>
        public virtual void Activate(bool instant = false)
        {
            if (m_HighlightActive || RefreshHighlightRenderers() == 0)
                return;

            m_HighlightActive = true;
            Highlight(instant);
        }

        /// <summary>
        /// Deactivates the highlight after refreshing cached renderers.
        /// </summary>
        public virtual void Deactivate(bool instant = false)
        {
            if (!m_HighlightActive || RefreshHighlightRenderers() == 0)
                return;

            m_HighlightActive = false;
            UnHighlight(instant);
        }

        protected virtual void Highlight(bool instant) { }

        protected virtual void UnHighlight(bool instant) { }

        protected virtual void OnRenderersRefreshed() { }
    }
}
