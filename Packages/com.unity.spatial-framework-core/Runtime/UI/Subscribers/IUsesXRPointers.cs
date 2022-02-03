using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Gives decorated class the ability to create XR pointers
    /// </summary>
    public interface IUsesXRPointers : IFunctionalitySubscriber<IProvidesXRPointers> { }

    /// <summary>
    /// Extension methods for implementors of IUsesCreateXRPointer
    /// </summary>
    public static class UsesXRPointersMethods
    {
        /// <summary>
        /// Adds a new XR pointer for a particular device. A Ray Interactor and the default UI actions will be added for the device.
        /// </summary>
        /// <param name="user">The object that has this functionality provided to it</param>
        /// <param name="device">The input device to drive the ray casting.</param>
        /// <param name="parent">The transform that the ray interactor will be created under</param>
        /// <param name="rayOrigin">An optional custom origin object used for the raycast origin.</param>
        /// <param name="validationCallback">An optional method that will be called to check whether the raycast source is currently valid.</param>
        /// <param name="existingController">An optional gameObject that already has a controller and ray interactor component to use instead of creating a new one.</param>
        /// <returns>The gameObject for the created pointer</returns>
        public static GameObject CreateXRPointer(this IUsesXRPointers user, InputDevice device, Transform parent, Transform rayOrigin = null, Func<IRaycastSource, bool> validationCallback = null, GameObject existingController = null)
        {
            return user.provider.CreateXRPointer(device, parent, rayOrigin, validationCallback, existingController);
        }

        /// <summary>
        /// Gets the device for a particular ray origin
        /// </summary>
        /// <param name="user">The object that has this functionality provided to it</param>
        /// <param name="rayOrigin">The ray origin transform</param>
        /// <returns>The input device associated with the ray origin, or null if no device is known</returns>
        public static InputDevice GetDeviceForRayOrigin(this IUsesXRPointers user, Transform rayOrigin)
        {
            return user.provider.GetDeviceForRayOrigin(rayOrigin);
        }

        /// <summary>
        /// Prevent UI interaction for a given rayOrigin
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="rayOrigin">The rayOrigin that is being checked</param>
        /// <param name="blocked">If true, UI interaction will be blocked for the rayOrigin.  If false, the ray origin will be removed from the blocked collection.</param>
        public static void SetUIBlockedForRayOrigin(this IUsesXRPointers user, Transform rayOrigin, bool blocked)
        {
            user.provider.SetUIBlockedForRayOrigin(rayOrigin, blocked);
        }

        /// <summary>
        /// Returns whether the specified ray origin is hovering over a UI element
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="rayOrigin">The rayOrigin that is being checked</param>
        /// <returns>Whether the ray is hovering over UI</returns>
        public static bool IsHoveringOverUI(this IUsesXRPointers user, Transform rayOrigin)
        {
            return user.provider.IsHoveringOverUI(rayOrigin);
        }

        /// <summary>
        /// Get the distance between the ray origin and the UI
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="rayOrigin">The ray origin for this pointer</param>
        /// <returns></returns>
        public static float GetHoverOverUIDistance(this IUsesXRPointers user, Transform rayOrigin)
        {
            return user.provider.GetHoverOverUIDistance(rayOrigin);
        }
    }
}
