using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Handle that is constrained in its local Z axis.
    /// Interaction rays intersect with a plane that passes through the axis, and the drag is projected along the constrained axis.
    /// </summary>
    public class LinearHandle : BaseHandle
    {
        /// <summary>
        /// Event data for interactions with linear handles
        /// </summary>
        public class LinearHandleEventData : HandleEventData
        {
            /// <summary>
            /// The point on the interaction plane that the raycast hits
            /// </summary>
            public Vector3 raycastHitWorldPosition;

            public LinearHandleEventData(Transform rayOrigin, bool direct) : base(rayOrigin, direct) {}
        }

#pragma warning disable 649
        [SerializeField, Tooltip("If enabled, drag interactions will act the same from any angles. If false, drags interactions act by intersecting the handle's local XZ plane")]
        bool m_OrientDragPlaneToRay = true;
#pragma warning restore 649

        readonly Dictionary<Transform, Vector3> m_LastPositions = new Dictionary<Transform, Vector3>(k_DefaultCapacity);

        Plane m_Plane;

        // Local method use only -- created here to reduce garbage collection
        static readonly LinearHandleEventData k_LinearHandleEventData = new LinearHandleEventData(null, false);

        protected override HandleEventData GetHandleEventData(XRBaseInteractor interactor)
        {
            var eventData = base.GetHandleEventData(interactor);
            k_LinearHandleEventData.rayOrigin = eventData.rayOrigin;
            k_LinearHandleEventData.direct = eventData.direct;
            k_LinearHandleEventData.raycastHitWorldPosition = eventData.worldPosition;
            return k_LinearHandleEventData;
        }

        void UpdateEventData(LinearHandleEventData eventData, bool setLastPosition = true)
        {
            var rayOrigin = eventData.rayOrigin;
            var lastPosition = m_LastPositions[rayOrigin];
            var worldPosition = lastPosition;

            var thisTransform = transform;
            if (m_OrientDragPlaneToRay)
            {
                // Orient a plane through the axis.
                // To avoid the plane being at a glancing angle from the ray, extend the plane perpendicular to the direction towards the ray origin.
                var rotation = thisTransform.rotation;
                var position = thisTransform.position;
                var forward = Quaternion.Inverse(rotation) * (rayOrigin.position - position);
                forward.z = 0;
                m_Plane.SetNormalAndPosition(rotation * forward.normalized, position);
            }
            else
            {
                // Orient the plane with the handle's XZ axis, do not account for the direction the ray is starting from
                m_Plane.SetNormalAndPosition(thisTransform.up, thisTransform.position);
            }

            float distance;
            var ray = eventData.GetRay();
            if (m_Plane.Raycast(ray, out distance))
                worldPosition = ray.GetPoint(Mathf.Min(distance, HandleSettings.instance.MaxDragDistance * this.GetViewerScale()));

            eventData.raycastHitWorldPosition = worldPosition;

            eventData.deltaPosition = Vector3.Project(worldPosition - lastPosition, thisTransform.forward);
            if (setLastPosition)
                m_LastPositions[rayOrigin] = worldPosition;
        }

        protected override void OnHandleHoverStarted(HandleEventData eventData)
        {
            var linearEventData = (LinearHandleEventData)eventData;
            m_LastPositions[eventData.rayOrigin] = linearEventData.raycastHitWorldPosition;

            if (!hasDragSource)
                UpdateEventData(linearEventData);

            base.OnHandleHoverStarted(eventData);
        }

        protected override void OnHandleHovering(HandleEventData eventData)
        {
            if (!hasDragSource)
                UpdateEventData((LinearHandleEventData)eventData);

            base.OnHandleHovering(eventData);
        }

        protected override void OnHandleDragStarted(HandleEventData eventData)
        {
            var linearEventData = (LinearHandleEventData)eventData;
            m_LastPositions[eventData.rayOrigin] = linearEventData.raycastHitWorldPosition;
            UpdateEventData(linearEventData);

            base.OnHandleDragStarted(eventData);
        }

        protected override void OnHandleDragging(HandleEventData eventData)
        {
            UpdateEventData((LinearHandleEventData)eventData);

            base.OnHandleDragging(eventData);
        }
    }
}
