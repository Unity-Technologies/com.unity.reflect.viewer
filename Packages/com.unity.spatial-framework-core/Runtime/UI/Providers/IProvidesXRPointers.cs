using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Provide the ability to create or access info about XR pointers
    /// XR pointers are able to interact with UI
    /// </summary>
    public interface IProvidesXRPointers : IFunctionalityProvider
    {
        /// <summary>
        /// Creates an XR Pointer. An XRPointer allows for UI, and general interaction and manipulation. It is bound to an InputDevice for input, and a transform parent for tracking.
        /// This system uses the XR Interaction Toolkit under the hood, and is driven by an XR Interactor.
        /// </summary>
        /// <param name="device">The input device to drive the ray casting.</param>
        /// <param name="parent">The transform that the ray interactor will be created under</param>
        /// <param name="rayOrigin">An optional custom origin object used for the raycast origin.</param>
        /// <param name="validationCallback">An optional method that will be called to check whether the raycast source is currently valid.</param>
        /// <param name="existingController">An optional gameObject that already has a controller and ray interactor component to use instead of creating a new one.</param>
        /// <returns>The gameObject for the created pointer</returns>
        GameObject CreateXRPointer(InputDevice device, Transform parent, Transform rayOrigin = null, Func<IRaycastSource, bool> validationCallback = null, GameObject existingController = null);

        /// <summary>
        /// Gets the device for a particular ray origin
        /// </summary>
        /// <param name="rayOrigin">The ray origin transform</param>
        /// <returns>The input device associated with the ray origin, or null if no device is known</returns>
        InputDevice GetDeviceForRayOrigin(Transform rayOrigin);

        /// <summary>
        /// Returns whether the specified ray origin is hovering over a UI element
        /// </summary>
        /// <param name="rayOrigin">The ray origin that is being checked</param>
        /// <returns>Whether the ray is hovering over UI</returns>
        bool IsHoveringOverUI(Transform rayOrigin);

        /// <summary>
        /// Returns the distance from which the ray origin is hovering over a UI element.
        /// If it is not currently hovering, then returns the max hover distance
        /// </summary>
        /// <param name="rayOrigin">The ray origin</param>
        /// <returns>The distance to the UI</returns>
        float GetHoverOverUIDistance(Transform rayOrigin);

        /// <summary>
        /// Prevent UI interaction for a given rayOrigin
        /// </summary>
        /// <param name="rayOrigin">The rayOrigin that is being checked</param>
        /// <param name="blocked">If true, UI interaction will be blocked for the rayOrigin.  If false, the ray origin will be removed from the blocked collection.</param>
        void SetUIBlockedForRayOrigin(Transform rayOrigin, bool blocked);
    }
}
