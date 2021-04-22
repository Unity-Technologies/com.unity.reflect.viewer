using Unity.XRTools.Rendering;
using UnityEngine;

namespace Unity.SpatialFramework.UI.Layout
{
    /// <summary>
    /// Component that moves two specified transforms to the start and end of the XR line renderer on this GameObject.
    /// </summary>
    [RequireComponent(typeof(XRLineRenderer))]
    public class LineEndPoints : MonoBehaviour
    {
        [SerializeField, Tooltip("Transform that will be moved to the start of the line.")]
        Transform m_StartPoint;

        [SerializeField, Tooltip("Transform that will be moved to the end of the line.")]
        Transform m_EndPoint;

        [SerializeField, Tooltip("If enabled, the end point will used a fixed index for the end point and skip checking for changes in the number of line points.")]
        bool m_FixedEndpointIndex;

        [SerializeField, Tooltip("The index in the line that the end point transform will be moved to. If FixedEndpointIndex is set to false, this value will be overwritten by the line's last vertex index every frame")]
        int m_EndPointIndex = 1;

        /// <summary>
        /// The transform that will be moved to the start point of the line
        /// </summary>
        public Transform startPoint
        {
            get => m_StartPoint;
            set => m_StartPoint = value;
        }

        /// <summary>
        /// The transform that will be moved to the end point of the line
        /// </summary>
        public Transform endPoint
        {
            get => m_EndPoint;
            set => m_EndPoint = value;
        }

        /// <summary>
        /// If enabled, the end point will used a fixed index for the end point and skip checking for changes in the number of line points
        /// </summary>
        public bool fixedEndpointIndex
        {
            get => m_FixedEndpointIndex;
            set => m_FixedEndpointIndex = value;
        }

        /// <summary>
        /// The index in the line that the end point transform will be moved to.
        /// If fixedEndpointIndex is set to false, this value will be overwritten by the line's last vertex index every frame
        /// </summary>
        public int endPointIndex
        {
            get => m_EndPointIndex;
            set => m_EndPointIndex = value;
        }

        XRLineRenderer m_XRLineRenderer;

        void Awake()
        {
            m_XRLineRenderer = GetComponent<XRLineRenderer>();
        }

        void Update()
        {
            if (!m_FixedEndpointIndex)
                m_EndPointIndex = m_XRLineRenderer.GetVertexCount() - 1;

            if (m_XRLineRenderer.useWorldSpace)
            {
                m_StartPoint.position = m_XRLineRenderer.GetPosition(0);
                m_EndPoint.position = m_XRLineRenderer.GetPosition(m_EndPointIndex);
            }
            else
            {
                m_StartPoint.localPosition = m_XRLineRenderer.GetPosition(0);
                m_EndPoint.localPosition = m_XRLineRenderer.GetPosition(m_EndPointIndex);
            }
        }
    }
}
