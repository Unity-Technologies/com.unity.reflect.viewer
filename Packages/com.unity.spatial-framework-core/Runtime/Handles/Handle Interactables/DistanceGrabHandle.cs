using System;
using Unity.SpatialFramework.Interaction;
using UnityEngine;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// A handle that can be moved at the end of a ray, and can be pushed and pulled
    /// </summary>
    public class DistanceGrabHandle : BaseHandle
    {
        /// <summary>
        /// Settings related to the push and pull behaviour of all Distance Grab Handles. These values are serialized in the HandleSettings asset.
        /// </summary>
        [Serializable]
        public class Settings
        {
            [Tooltip("The max amount the handle can be pushed. This will be multiplied by the starting distance (Default 5x)")]
            public float MaxPushDistanceMultiplier = 5f;

            [Tooltip("The max amount the handle can be pulled. This will be multiplied by the starting distance (Default 1/10x)")]
            public float MaxPullDistanceMultiplier = 0.1f;

            [Tooltip("Pushing or pulling less than this amount will not result in any pushing or pulling (Default 0.05m)")]
            public float PushPullThreshold = 0.05f;

            [Tooltip("Amount pushed or pulled past the PushPullThreshold will be result in the maximum push or pull (Default 0.25m)")]
            public float MaxPushPullDistance = 0.25f;
        }

#pragma warning disable 649
        [SerializeField, Tooltip("Curve determines how much physical distance will be factored into pushing the handle.")]
        AnimationCurve m_PushDistanceCurve;

        [SerializeField, Tooltip("Curve determines how much physical distance will be factored into pulling the handle.")]
        AnimationCurve m_PullDistanceCurve;
#pragma warning restore 649

        float m_DefaultHoldingDistance;
        float m_InitialPushPullAmount;
        Vector3 m_RelativeStartOrigin;
        Vector3 m_GrabOffset;

        protected override void Reset()
        {
            base.Reset();
            m_PushDistanceCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 2));
            m_PullDistanceCurve = new AnimationCurve(new Keyframe(0, 0, 0, 0), new Keyframe(1, 1, 2, 2));
        }

        float CalculatePushPullAmount(Transform rayOrigin)
        {
            // Determines how much the ray origin transform has been pushed or pulled in the direction it is facing from its initial position.
            // The amount factors in the scale of the main camera and compares the ray origin local position to its initial position
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return 0f;

            var handPosition = rayOrigin.position;
            var handForward = rayOrigin.forward;

            var viewerScale = this.GetViewerScale();
            var cameraRig = mainCamera.transform.parent;
            var referencePosition = cameraRig != null ? cameraRig.TransformPoint(m_RelativeStartOrigin) : m_RelativeStartOrigin;
            var headToHand = (handPosition - referencePosition) / viewerScale;

            return Vector3.Dot(headToHand, handForward);
        }

        protected override void OnHandleDragStarted(HandleEventData eventData)
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
                return;

            var rayOrigin = eventData.rayOrigin;
            var position = rayOrigin.position;
            var rayOriginPosition = position;
            var rigidBodyPosition = transform.position;
            m_DefaultHoldingDistance = Vector3.Distance(rayOriginPosition, eventData.worldPosition);
            var cameraRig = mainCamera.transform.parent;
            m_RelativeStartOrigin = cameraRig.InverseTransformPoint(position);
            m_InitialPushPullAmount = CalculatePushPullAmount(rayOrigin);
            m_GrabOffset = eventData.worldPosition - rigidBodyPosition;
            base.OnHandleDragStarted(eventData);
        }

        protected override void OnHandleDragging(HandleEventData eventData)
        {
            var rayOrigin = eventData.rayOrigin;
            var pushPullAmount = CalculatePushPullAmount(rayOrigin);
            var pushPullDiff = pushPullAmount - m_InitialPushPullAmount;
            var targetHoldDistance = m_DefaultHoldingDistance;

            var settings = HandleSettings.instance.DistanceGrabSettings;
            if (pushPullDiff > settings.PushPullThreshold)
            {
                var pushDistance = pushPullDiff - settings.PushPullThreshold;
                var pushPercent = Mathf.Clamp01(pushDistance / settings.MaxPushPullDistance);
                var pushFactor = m_PushDistanceCurve.Evaluate(pushPercent);
                targetHoldDistance = Mathf.Lerp(m_DefaultHoldingDistance, settings.MaxPushDistanceMultiplier * m_DefaultHoldingDistance, pushFactor);
            }
            else if (pushPullDiff < -settings.PushPullThreshold)
            {
                var pullDistance = -pushPullDiff - settings.PushPullThreshold;
                var pullPercent = Mathf.Clamp01(pullDistance / settings.MaxPushPullDistance);
                var pullFactor = m_PullDistanceCurve.Evaluate(pullPercent);
                targetHoldDistance = Mathf.Lerp(m_DefaultHoldingDistance, settings.MaxPullDistanceMultiplier * m_DefaultHoldingDistance, pullFactor);
            }

            var targetPosition = rayOrigin.position + rayOrigin.forward * targetHoldDistance;
            var grabPosition = (transform.position + m_GrabOffset);
            var delta = targetPosition - grabPosition;
            eventData.deltaPosition = delta;
            base.OnHandleDragging(eventData);
        }
    }
}
