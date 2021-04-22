using UnityEngine;

namespace Unity.SpatialFramework.Avatar
{
    /// <summary>
    /// Controls the avatar level of detail when it is beyond a certain distance from the viewer.
    /// </summary>
    public class AvatarLOD : MonoBehaviour
    {
        [SerializeField, Tooltip("If the avatar is further than this distance (in meters), the avatar details will be deactivated.")]
        float m_DistanceThreshold = 3f;

        [SerializeField, Tooltip("The GameObjects that will be deactivated when the avatar is beyond the distance threshold.")]
        GameObject[] m_GameObjectsToDeactivate;

        [SerializeField, Tooltip("(Optional) The viewer transform. If not set, this will default to the main camera transform.")]
        Transform m_ViewerTransform;

        bool m_IsMinimized;

        /// <summary>
        /// If the avatar is further than this distance (in meters), the avatar details will be deactivated.
        /// </summary>
        public float distanceThreshold
        {
            get => m_DistanceThreshold;
            set => m_DistanceThreshold = value;
        }

        /// <summary>
        /// The GameObjects that will be deactivated when the avatar is beyond the distance threshold.
        /// </summary>
        public GameObject[] gameObjectsToDeactivate
        {
            get => m_GameObjectsToDeactivate;
            set => m_GameObjectsToDeactivate = value;
        }

        /// <summary>
        /// (Optional) The viewer transform. If not set, this will default to the main camera transform.
        /// </summary>
        public Transform viewerTransform
        {
            get => m_ViewerTransform;
            set => m_ViewerTransform = value;
        }

        void OnEnable()
        {
            if (m_ViewerTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    m_ViewerTransform = mainCamera.transform;
            }
        }

        void LateUpdate()
        {
            if (m_ViewerTransform == null)
                return;

            var viewerPosition = m_ViewerTransform.position;
            var viewerScale = m_ViewerTransform.lossyScale.x;
            var viewerDistance = Vector3.Distance(gameObject.transform.position, viewerPosition);
            var beyondDistanceThreshold = viewerDistance > m_DistanceThreshold * viewerScale;
            if (beyondDistanceThreshold)
            {
                if (!m_IsMinimized)
                {
                    foreach (var gameObjectToDeactivate in m_GameObjectsToDeactivate)
                        gameObjectToDeactivate.SetActive(false);

                    m_IsMinimized = true;
                }
            }
            else
            {
                if (m_IsMinimized)
                {
                    foreach (var gameObjectToActivate in m_GameObjectsToDeactivate)
                        gameObjectToActivate.SetActive(true);

                    m_IsMinimized = false;
                }
            }
        }
    }
}
