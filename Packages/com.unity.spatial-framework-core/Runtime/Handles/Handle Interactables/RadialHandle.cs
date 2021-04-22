using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Handle that rotates in its local Y axis
    /// Interaction rays intersect the handles XZ plane, and the drag is converted into a change of angle around the handle's position.
    /// </summary>
    public class RadialHandle : BaseHandle
    {
        /// <summary>
        /// Event data for interactions with radial handles
        /// </summary>
        public class RadialHandleEventData : HandleEventData
        {
            /// <summary>
            /// The point on the interaction plane that the raycast hits
            /// </summary>
            public Vector3 raycastHitWorldPosition;

            public RadialHandleEventData(Transform rayOrigin, bool direct) : base(rayOrigin, direct) {}
        }

        Plane m_Plane;
        readonly Dictionary<Transform, Vector3> m_LastPositions = new Dictionary<Transform, Vector3>(k_DefaultCapacity);

        // Local method use only -- created here to reduce garbage collection
        static readonly RadialHandleEventData k_RadialHandleEventData = new RadialHandleEventData(null, false);

        protected override HandleEventData GetHandleEventData(XRBaseInteractor interactor)
        {
            var eventData = base.GetHandleEventData(interactor);
            k_RadialHandleEventData.rayOrigin = eventData.rayOrigin;
            k_RadialHandleEventData.direct = eventData.direct;
            k_RadialHandleEventData.raycastHitWorldPosition = eventData.worldPosition;
            return k_RadialHandleEventData;
        }

        protected override void OnHandleDragStarted(HandleEventData eventData)
        {
            var rayOrigin = eventData.rayOrigin;
            var radialEventData = (RadialHandleEventData)eventData;
            m_LastPositions[rayOrigin] = radialEventData.raycastHitWorldPosition;
            var thisTransform = transform;
            m_Plane.SetNormalAndPosition(thisTransform.up, thisTransform.position);

            base.OnHandleDragStarted(eventData);
        }

        protected override void OnHandleDragging(HandleEventData eventData)
        {
            var rayOrigin = eventData.rayOrigin;
            var lastPosition = m_LastPositions[rayOrigin];
            var worldPosition = lastPosition;

            float distance;
            var ray = eventData.GetRay();
            if (m_Plane.Raycast(ray, out distance))
                worldPosition = ray.GetPoint(Mathf.Abs(distance));

            var thisTransform = transform;
            var up = thisTransform.up;
            var pivot = thisTransform.parent.position;
            var angle = Vector3.SignedAngle(worldPosition - pivot, m_LastPositions[rayOrigin] - pivot, -up);
            eventData.deltaRotation = Quaternion.AngleAxis(angle, up);

            m_LastPositions[rayOrigin] = worldPosition;
            base.OnHandleDragging(eventData);
        }
    }
}
