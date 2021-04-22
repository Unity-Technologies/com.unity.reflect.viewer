using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Settings asset that contains values used for handles
    /// </summary>
    [ScriptableSettingsPath("Assets/SpatialFramework/Settings")]
    public class HandleSettings : ScriptableSettings<HandleSettings>
    {
        [SerializeField, Tooltip("The max distance in meters (relative to the viewer) that handles can be dragged in a single interaction.")]
        float m_MaxDragDistance = 100f;

        [SerializeField, Tooltip("Handles will not be interactable by a ray when the view angle is less than this (degrees).")]
        float m_ViewAngleRayThreshold = 12f;

        [SerializeField, Tooltip("Settings for the distance grab handle. The handle attaches to the end of the selection ray and can be pushed and pulled by translating the interaction origin forward and back.")]
        DistanceGrabHandle.Settings m_DistanceGrabSettings;

        [SerializeField, Tooltip("Settings for the sphere handle. The handle attaches to the end of the selection ray, and the attach distance can be adjusted by scrolling up and down.")]
        SphereHandle.Settings m_SphereHandleSettings;

        /// <summary>
        /// The max distance in meters (relative to the viewer) that handles can be dragged in a single interaction.
        /// Multiplying this by the ViewerScale property will determine the max distance a handle can be dragged from its initial position.
        /// </summary>
        public float MaxDragDistance
        {
            get => m_MaxDragDistance;
            set => m_MaxDragDistance = value;
        }

        /// <summary>
        /// Dot product threshold to check if handle axis is parallel to view
        /// </summary>
        public float ViewParallelDotThreshold => Mathf.Cos(Mathf.Deg2Rad * m_ViewAngleRayThreshold);

        /// <summary>
        /// Dot product threshold to check if handle normal is perpendicular to view
        /// </summary>
        public float ViewPerpendicularDotThreshold => Mathf.Cos(Mathf.Deg2Rad * (90f - m_ViewAngleRayThreshold));

        /// <summary>
        /// Settings for the distance grab handle.
        /// </summary>
        public DistanceGrabHandle.Settings DistanceGrabSettings
        {
            get => m_DistanceGrabSettings;
            set => m_DistanceGrabSettings = value;
        }

        /// <summary>
        /// Settings for the sphere handle.
        /// </summary>
        public SphereHandle.Settings SphereHandleSettings
        {
            get => m_SphereHandleSettings;
            set => m_SphereHandleSettings = value;
        }
    }
}
