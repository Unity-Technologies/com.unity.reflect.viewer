using Unity.XRTools.Rendering;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// A line renderer that uses a ray interactor to drive its visuals.
    /// The line will bend if there are more than 2 vertices.
    /// </summary>
    [RequireComponent(typeof(XRLineRenderer))]
    public class RayInteractionLine : RayInteractionRenderer
    {
        [SerializeField, Tooltip("Specifies the settings scriptable object that defines the visual style of this line.")]
        InteractionLineSettings m_LineSettings;

        XRLineRenderer m_LineRenderer;
        Vector3 m_StraightLineEndPoint;
        Vector3[] m_NewPoints;
        Vector3 m_CurrentEndPoint;
        bool m_SnapEndPoint = true;
        Vector3[] m_RaycastLinePoints = new Vector3[2];

        /// <summary>
        /// Reference to the lineSettings scriptableObject that contains the visual properties for this line.
        /// </summary>
        public InteractionLineSettings lineSettings
        {
            get => m_LineSettings;
            set
            {
                m_LineSettings = value;
                UpdateSettings();
            }
        }

        XRLineRenderer lineRenderer
        {
            get
            {
                if (m_LineRenderer == null)
                    m_LineRenderer = GetComponent<XRLineRenderer>();

                return m_LineRenderer;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            lineRenderer.enabled = true;
            m_SnapEndPoint = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            lineRenderer.enabled = false;
        }

        protected virtual void Reset()
        {
            // Initialize positions array, otherwise it will be null when trying to check the vertex count in ValidateVertexCount.
            lineRenderer.SetPositions(new[] { Vector3.zero, Vector3.zero }, true);
            UpdateSettings();
        }

        protected override void Awake()
        {
            base.Awake();
            if (m_LineSettings == null)
            {
                Debug.LogError("Ray Interaction Line does not have a line settings asset reference.", this);
            }
            else
            {
                m_NewPoints = new Vector3[m_LineSettings.minimumVertexCount];
                UpdateSettings();
            }
        }

        /// <summary>
        /// Updates the line based on the state of the target ray detector.
        /// Called every frame in which the target ray detector is non-null.
        /// </summary>
        protected override void UpdateVisuals()
        {
            var lineRenderable = rayInteractor as ILineRenderable;
            if (lineRenderable == null)
            {
                // Hide the renderer if the current hover is not using a ray
                lineRenderer.enabled = false;
                return;
            }

            lineRenderer.enabled = true;

            lineRenderable.GetLinePoints(ref m_RaycastLinePoints, out var noPoints);

            if (noPoints <= 0)
                return;

            var startPosition = m_RaycastLinePoints[0];
            m_StraightLineEndPoint = startPosition + rayInteractor.attachTransform.forward * currentRayLength;

            if (selectedObjectTransform != null && m_LineSettings.bendable)
            {
                m_CurrentEndPoint = CurrentHitOrSelectPoint;
            }
            else
            {
                m_CurrentEndPoint = m_LineSettings.smoothEndpoint ? Vector3.Lerp(m_CurrentEndPoint, m_StraightLineEndPoint, 1 - Mathf.Exp(-m_LineSettings.followTightness * Time.deltaTime)) : m_StraightLineEndPoint;
            }

            if (m_SnapEndPoint)
            {
                m_CurrentEndPoint = m_StraightLineEndPoint;
                m_SnapEndPoint = false;
            }

            for (var i = 0; i < m_NewPoints.Length; i++)
            {
                var normalizedPointValue = (float)(i) / (m_NewPoints.Length - 1);
                if (m_LineSettings.bendable)
                {
                    var manipToEndPoint = Vector3.Lerp(startPosition, m_CurrentEndPoint, normalizedPointValue);
                    var manipToAnchor = Vector3.Lerp(startPosition, m_StraightLineEndPoint, normalizedPointValue);
                    m_NewPoints[i] = Vector3.Lerp(manipToAnchor, manipToEndPoint, normalizedPointValue);
                }
                else
                {
                    m_NewPoints[i] = Vector3.Lerp(startPosition, m_CurrentEndPoint, normalizedPointValue);
                }
            }

            lineRenderer.SetPositions(m_NewPoints);
        }

        [ContextMenu("Force Update Line Renderer")]
        void ForceUpdate()
        {
            UpdateSettings();

            for (var i = 0; i < m_NewPoints.Length; i++)
            {
                var normalizedPointValue = (float)(i) / (m_NewPoints.Length - 1);

                m_NewPoints[i] = Vector3.Lerp(transform.position, transform.TransformPoint(new Vector3(0, 0, m_DefaultLineLength)), normalizedPointValue);
            }

            lineRenderer.SetPositions(m_NewPoints);

        }

        void UpdateSettings()
        {
            lineRenderer.SetVertexCount(m_LineSettings.minimumVertexCount);
            m_NewPoints = new Vector3[m_LineSettings.minimumVertexCount];
            lineRenderer.widthMultiplier = m_LineSettings.lineWidth;
            lineRenderer.widthCurve = m_LineSettings.widthCurve;
            lineRenderer.colorGradient = m_LineSettings.lineColorGradient;
            m_SnapEndPoint = true;
        }
    }
}
