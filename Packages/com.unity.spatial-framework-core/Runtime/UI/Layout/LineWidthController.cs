using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Rendering;
using UnityEngine;

namespace Unity.SpatialFramework.UI.Layout
{
    /// <summary>
    /// Component that controls the width of a XR line renderer.
    /// The line width will be adjusted based on the current viewer scale and a temporary width modifier that can be set by other scripts.
    /// </summary>
    [RequireComponent(typeof(XRLineRenderer))]
    public class LineWidthController : MonoBehaviour, IUsesViewerScale
    {
        XRLineRenderer m_LineRenderer;
        float m_DefaultWidth;
        Transform m_CameraTransform;

        /// <summary>
        /// A multiplier factor that will be applied to the final line renderer width.
        /// This can be changed to temporarily change the line separate from the viewer scale width multiplier.
        /// </summary>
        public float TemporaryWidthMultiplier { get; set; } = 1.0f;

        IProvidesViewerScale IFunctionalitySubscriber<IProvidesViewerScale>.provider { get; set; }

        void OnEnable()
        {
            m_LineRenderer = GetComponent<XRLineRenderer>();
            m_DefaultWidth = m_LineRenderer.widthMultiplier;
            var mainCamera = Camera.main;
            if (mainCamera != null)
                m_CameraTransform = mainCamera.transform;
        }

        void OnDisable()
        {
            m_LineRenderer.widthMultiplier = m_DefaultWidth;
        }

        void LateUpdate()
        {
            m_LineRenderer.widthMultiplier = m_DefaultWidth * this.TryGetViewerScale(m_CameraTransform) * TemporaryWidthMultiplier;
        }
    }
}
