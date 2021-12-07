using UnityEngine;

namespace Unity.SpatialFramework.UI.Layout
{
    /// <summary>
    /// Makes the object this is attached to follow a target with a delay and some other layout options.
    /// </summary>
    public class LazyFollow : MonoBehaviour
    {
        [SerializeField, Tooltip("The object being followed. If not set, this will default to the viewer camera when this component is enabled.")]
        Transform m_Target;

        [SerializeField, Tooltip("The amount to offsets the target's position when following. This position is relative/local to the target object.")]
        Vector3 m_TargetOffset = Vector3.forward;

        [SerializeField, Tooltip("The laziness or smoothing that is applied to the follow movement. 0 results in direct following, higher values will cause this object to follow more lazily.")]
        float m_MovementSmoothing = 0.3f;

        Vector3 m_Velocity = Vector3.zero;

        Vector3 targetPosition => m_Target.position + m_Target.TransformVector(m_TargetOffset);

        /// <summary>
        /// The object being followed. If not set, this will default to the main camera when this component is enabled.
        /// </summary>
        public Transform target
        {
            get => m_Target;
            set => m_Target = value;
        }

        /// <summary>
        /// The amount to offsets the target's position when following. This position is relative/local to the target object.
        /// </summary>
        public Vector3 targetOffset
        {
            get => m_TargetOffset;
            set => m_TargetOffset = value;
        }

        void OnEnable()
        {
            // Default to main camera
            if (m_Target == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    m_Target = mainCamera.transform;
            }

            var thisTransform = transform;
            thisTransform.position = targetPosition;
        }

        void Update()
        {
            var targetPos = targetPosition;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref m_Velocity, m_MovementSmoothing, Mathf.Infinity, Time.unscaledDeltaTime);
        }
    }
}
