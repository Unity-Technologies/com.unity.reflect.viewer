using System;
using Unity.SpatialFramework.Interaction;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Handle that moves restricted to a sphere around the interaction pointer ray origin.
    /// </summary>
    public class SphereHandle : BaseHandle, IScrollHandler
    {
        /// <summary>
        /// Settings related to sphere handles. These values are serialized in the HandleSettings asset.
        /// </summary>
        [Serializable]
        public class Settings
        {
            [Tooltip("The maximum radius of the sphere that the handle will move along, relative to the scale of the viewer.")]
            public float MaxSphereRadius = 100f;

            [Tooltip("The initial rate that the radius will change at when scrolling.")]
            public float InitialScrollRate = 2f;

            [Tooltip("The acceleration that the scroll rate will change.")]
            public float ScrollAcceleration = 14f;
        }

        float m_ScrollRate;
        Vector3 m_LastPosition;
        float m_CurrentRadius;

        protected override void OnHandleDragStarted(HandleEventData eventData)
        {
            var rayOrigin = eventData.rayOrigin;
            m_CurrentRadius = Vector3.Distance(eventData.worldPosition, rayOrigin.position);
            m_ScrollRate = HandleSettings.instance.SphereHandleSettings.InitialScrollRate;
            m_LastPosition = GetRayPoint(eventData);
            base.OnHandleDragStarted(eventData);
        }

        protected override void OnHandleDragging(HandleEventData eventData)
        {
            var rayEndPoint = GetRayPoint(eventData);
            eventData.deltaPosition = rayEndPoint - m_LastPosition;
            m_LastPosition = rayEndPoint;
            base.OnHandleDragging(eventData);
        }

        /// <summary>
        /// Change the current radius of the sphere handle. This will push or pull the handle from the ray origin that is dragging it.
        /// </summary>
        /// <param name="delta"> The amount to change the radius. This will be scaled by the current viewer scale.</param>
        public void ChangeRadius(float delta)
        {
            var handleSettings = HandleSettings.instance;
            var viewerScale = this.GetViewerScale();
            m_CurrentRadius += delta * viewerScale;
            m_CurrentRadius = Mathf.Clamp(m_CurrentRadius, 0f, handleSettings.SphereHandleSettings.MaxSphereRadius * viewerScale);
        }

        /// <summary>
        /// Scrolling on this handle will change the current radius of the sphere
        /// </summary>
        /// <param name="eventData"></param>
        public void OnScroll(PointerEventData eventData)
        {
            if (!hasDragSource)
                return;

            var sphereHandleSettings = HandleSettings.instance.SphereHandleSettings;
            // Scrolling changes the radius of the sphere while dragging, and accelerates
            if (Mathf.Abs(eventData.scrollDelta.y) > 0.5f)
                m_ScrollRate += Mathf.Abs(eventData.scrollDelta.y) * sphereHandleSettings.ScrollAcceleration * Time.deltaTime;
            else
                m_ScrollRate = sphereHandleSettings.InitialScrollRate;

            ChangeRadius(m_ScrollRate * eventData.scrollDelta.y * Time.deltaTime);
        }

        Vector3 GetRayPoint(HandleEventData eventData)
        {
            return eventData.GetRay().GetPoint(m_CurrentRadius);
        }
    }
}
