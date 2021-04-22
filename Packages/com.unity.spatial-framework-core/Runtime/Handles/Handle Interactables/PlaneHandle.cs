using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using Unity.XRTools.Utils.GUI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Handle that is constrained to its local XY plane.
    /// </summary>
    public class PlaneHandle : BaseHandle
    {
        /// <summary>
        /// Event data for interactions with plane handles
        /// </summary>
        public class PlaneHandleEventData : HandleEventData
        {
            /// <summary>
            /// The point on the interaction plane that the raycast hits
            /// </summary>
            public Vector3 raycastHitWorldPosition;

            public PlaneHandleEventData(Transform rayOrigin, bool direct) : base(rayOrigin, direct) {}
        }

#pragma warning disable 649
        [SerializeField, FlagsProperty, Tooltip("The axes that the plane handle is constrained in relative to the target being handled.")]
        AxisFlags m_Constraints;
#pragma warning restore 649

        Plane m_Plane;
        readonly Dictionary<Transform, Vector3> m_LastPositions = new Dictionary<Transform, Vector3>(k_DefaultCapacity);

        /// <summary>
        /// The axes that the handle is constrained in. This is relative to the object being handled, not the local handle's rotation.
        /// </summary>
        public AxisFlags constraints { get { return m_Constraints; } }

        // Local method use only -- created here to reduce garbage collection
        static readonly PlaneHandleEventData k_PlaneHandleEventData = new PlaneHandleEventData(null, false);

        protected override HandleEventData GetHandleEventData(XRBaseInteractor interactor)
        {
            var eventData = base.GetHandleEventData(interactor);
            k_PlaneHandleEventData.rayOrigin = eventData.rayOrigin;
            k_PlaneHandleEventData.direct = eventData.direct;
            k_PlaneHandleEventData.raycastHitWorldPosition = eventData.worldPosition;
            return k_PlaneHandleEventData;
        }

        protected override void OnHandleDragStarted(HandleEventData eventData)
        {
            var planeEventData = (PlaneHandleEventData)eventData;
            var position = planeEventData.raycastHitWorldPosition;

            // Force local z to 0 because hit point will be on the handle mesh, the drag plane
            var handleTransform = transform;
            position = handleTransform.InverseTransformPoint(position);
            position.z = 0;
            position = handleTransform.TransformPoint(position);

            m_LastPositions[eventData.rayOrigin] = position;
            m_Plane.SetNormalAndPosition(handleTransform.forward, handleTransform.position);

            base.OnHandleDragStarted(eventData);
        }

        protected override void OnHandleDragging(HandleEventData eventData)
        {
            var rayOrigin = eventData.rayOrigin;
            var lastPosition = m_LastPositions[rayOrigin];
            var worldPosition = lastPosition;

            // Raycast against the plane and get the hit point
            var ray = eventData.GetRay();
            if (m_Plane.Raycast(ray, out var distance))
                worldPosition = ray.GetPoint(Mathf.Min(Mathf.Abs(distance), HandleSettings.instance.MaxDragDistance * this.GetViewerScale()));

            var deltaPosition = worldPosition - lastPosition;
            m_LastPositions[rayOrigin] = worldPosition;

            eventData.deltaPosition = deltaPosition;
            base.OnHandleDragging(eventData);
        }
    }
}
