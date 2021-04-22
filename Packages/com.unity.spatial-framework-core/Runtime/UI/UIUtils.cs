using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// UI related utilities
    /// </summary>
    public static class UIUtils
    {
        /// <summary>
        /// Maximum interval between clicks that count as a double-click
        /// </summary>
        public const float DoubleClickIntervalMax = 0.3f;

        const float k_DoubleClickIntervalMin = 0.15f;

        /// <summary>
        /// Returns whether the given time interval qualifies as a double-click
        /// </summary>
        /// <param name="timeSinceLastClick">Time interval between clicks</param>
        /// <returns></returns>
        public static bool IsDoubleClick(float timeSinceLastClick)
        {
            return timeSinceLastClick <= DoubleClickIntervalMax && timeSinceLastClick >= k_DoubleClickIntervalMin;
        }

        /// <summary>
        /// Utility method for getting the ray origin transform from event data, for example the PointerEventData argument to an IPointerEnter implementor.
        /// If the event data can be cast to TrackedDeviceEventData, then it was sent from the XR interaction system.
        /// If the interactor is a Ray Interactor then the ray origin is the interactor's attachTransform. Otherwise there is no ray origin.
        /// </summary>
        /// <param name="eventData">The base event data that might be a TrackedEventData</param>
        /// <param name="rayOrigin">The ray origin transform associated with this event, if it exists. Otherwise null.</param>
        /// <returns>Whether a ray origin was found for the event data.</returns>
        public static bool TryGetRayOrigin(this BaseEventData eventData, out Transform rayOrigin)
        {
            var interactor = (eventData as TrackedDeviceEventData)?.interactor;
            if (interactor is XRRayInteractor rayInteractor)
            {
                rayOrigin = rayInteractor.attachTransform;
                return true;
            }

            rayOrigin = null;
            return false;
        }

        /// <summary>
        /// Utility method for uses of IUsesViewerScale that may not have a provider, and might have a camera transform reference.
        /// If there is no provider, then it returns the lossyScale.x of the camera transform. If the transform is also null, it returns 1f.
        /// </summary>
        /// <param name="viewerScaleUser">The class that implements IUsesViewerScale and may or may not have a provider.</param>
        /// <param name="cameraTransform">The reference to a transform whose lossy scale can be used as a backup for the viewer scale.</param>
        /// <returns>The estimated scale of the current viewer. Use this to scale values that should match the scale of the viewer. </returns>
        public static float TryGetViewerScale(this IUsesViewerScale viewerScaleUser, Transform cameraTransform)
        {
            if (viewerScaleUser.HasProvider())
                return viewerScaleUser.GetViewerScale();

            return cameraTransform != null ? cameraTransform.lossyScale.x : 1f;
        }
    }
}
