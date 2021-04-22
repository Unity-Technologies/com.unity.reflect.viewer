using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.UI.Layout
{
    /// <summary>
    /// Component that controls the scale of the GameObject based on the distance from the main camera.
    /// The distance is divided into zones, and the transform scale is smoothly transitioned when the object moves into a different zone.
    /// The size of the zones are relative to the viewer's scale.
    /// </summary>
    public class ZoneScale : MonoBehaviour, IUsesViewerScale
    {
        const float k_SmoothTime = 0.3f;

#pragma warning disable 649
        [SerializeField]
        bool m_Clamp;

        [SerializeField]
        float m_ClampMax = 10f;

        [SerializeField]
        float m_ClampMin = 1f;

        [SerializeField]
        float m_ZoneSize = 0.2f;

        [SerializeField]
        float m_DefaultScale = 1f;
#pragma warning restore 649

        float m_YVelocity;
        float m_LastScale = 1.0f;
        bool m_Snap;
        Transform m_MainCameraTransform;
        int m_CurrentZone;
        static readonly HashSet<ZoneScale> k_EnabledInstances = new HashSet<ZoneScale>();

        IProvidesViewerScale IFunctionalitySubscriber<IProvidesViewerScale>.provider { get; set; }

        /// <summary>
        /// Skip the transition interpolation to the target scale immediately
        /// </summary>
        public void Snap()
        {
            m_Snap = true;
            SetScaleForCurrentDistance();
        }

        void OnEnable()
        {
            var mainCamera = Camera.main;
            if (mainCamera != null)
                m_MainCameraTransform = mainCamera.transform;

            Snap();
            k_EnabledInstances.Add(this);
        }

        void OnDisable()
        {
            k_EnabledInstances.Remove(this);
        }

        /// <summary>
        /// Causes all zone scale layout components to snap to the correct scale immediately.
        /// </summary>
        public static void SnapAll()
        {
            foreach (var zoneScale in k_EnabledInstances)
            {
                zoneScale.Snap();
            }
        }

        void LateUpdate()
        {
            SetScaleForCurrentDistance();
        }

        void SetScaleForCurrentDistance()
        {
            if (m_MainCameraTransform == null)
                return;

            var cameraPosition = m_MainCameraTransform.position;
            var deltaToCamera = cameraPosition - transform.position;
            var adjustedDistance = deltaToCamera.magnitude;
            var scaledZoneSize = m_ZoneSize * this.TryGetViewerScale(m_MainCameraTransform);
            var zone = Mathf.CeilToInt(adjustedDistance / scaledZoneSize);
            var bufferSize = scaledZoneSize * 0.5f;
            if (adjustedDistance > m_CurrentZone * scaledZoneSize + bufferSize ||
                adjustedDistance < m_CurrentZone * scaledZoneSize - bufferSize)
            {
                m_CurrentZone = zone;
            }

            var targetScale = m_CurrentZone * scaledZoneSize;
            var newScale = m_Snap ? targetScale : Mathf.SmoothDamp(m_LastScale, targetScale, ref m_YVelocity, k_SmoothTime);

            if (m_Snap)
                m_Snap = false;

            if (m_Clamp)
                newScale = Mathf.Clamp(newScale, m_ClampMin, m_ClampMax);

            transform.localScale = Vector3.one * (newScale * m_DefaultScale);
            m_LastScale = newScale;
        }
    }
}
