using UnityEngine;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// A cursor renderer that uses a ray interactor to drive its visuals.
    /// </summary>
    public class RayInteractionCursor : RayInteractionRenderer
    {
#pragma warning disable 649
        [SerializeField, Tooltip("Whether this game object should scale 1-1 with its distance from the camera. " +
             "Useful for keeping the cursor from popping to a larger or smaller size after it leaves a surface.")]
        bool m_ScaleWithDistanceFromCamera = true;

        [SerializeField, Tooltip("Whether this object's forward vector should mirror the normal of any surface it hits.")]
        bool m_AlignToHitNormal = true;

        [SerializeField, Tooltip("Whether this object's up vector should follow the up vector of the ray origin.")]
        bool m_AlignToRayOriginUp;

        [SerializeField, Tooltip("If true, this cursor will not position, rotate, or scale itself based on hit surfaces while fading out.")]
        bool m_IgnoreHitSurfacesWhenFadingOut = true;
#pragma warning restore 649

        Vector3 m_OriginalScale;

        /// <summary>
        /// Whether this game object should scale 1-1 with its distance from the camera.
        /// Useful for keeping the cursor from popping to a larger or smaller size after it leaves a surface.
        /// </summary>
        public bool scaleWithDistanceFromCamera
        {
            get => m_ScaleWithDistanceFromCamera;
            set => m_ScaleWithDistanceFromCamera = value;
        }

        /// <summary>
        /// Whether this object's forward vector should mirror the normal of any surface it hits.
        /// </summary>
        public bool alignToHitNormal
        {
            get => m_AlignToHitNormal;
            set => m_AlignToHitNormal = value;
        }

        /// <summary>
        /// Whether this object's up vector should follow the up vector of the ray origin.
        /// </summary>
        public bool alignToRayOriginUp
        {
            get => m_AlignToRayOriginUp;
            set => m_AlignToRayOriginUp = value;
        }

        protected override void Awake()
        {
            base.Awake();
            m_OriginalScale = transform.localScale;
            transform.SetParent(null);
        }

        /// <summary>
        /// Updates the cursor based on the state of the target ray detector.
        /// Called every frame in which the target ray detector is non-null.
        /// </summary>
        protected override void UpdateVisuals()
        {
            if (visibility == Visibility.FadingOut && m_IgnoreHitSurfacesWhenFadingOut)
                return;

            var rayOrigin = rayInteractor.attachTransform;

            var thisTransform = transform;
            thisTransform.position = CurrentHitOrSelectPoint;

            var mainCamera = Camera.main;
            if (scaleWithDistanceFromCamera && mainCamera != null)
            {
                var distanceFromCamera = (thisTransform.position - mainCamera.transform.position).magnitude;
                thisTransform.localScale = m_OriginalScale * distanceFromCamera;
            }

            var newForward = alignToHitNormal ? -CurrentHitNormal : rayOrigin.forward;

            if (alignToRayOriginUp)
            {
                var newRot = thisTransform.rotation;
                newRot.SetLookRotation(newForward, rayOrigin.up);
                thisTransform.rotation = newRot;
            }
            else
            {
                thisTransform.forward = newForward;
            }
        }
    }
}
