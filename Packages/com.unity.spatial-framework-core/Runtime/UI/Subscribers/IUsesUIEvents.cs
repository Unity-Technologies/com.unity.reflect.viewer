using System;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Gives decorated class access to UI events
    /// </summary>
    public interface IUsesUIEvents : IFunctionalitySubscriber<IProvidesUIEvents> { }

    public static class UseUIEventsMethods
    {
        /// <summary>
        /// Subscribe to the dragStarted event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be called when the event occurs</param>
        public static void SubscribeToDragStarted(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.dragStarted += action;
        }

        /// <summary>
        /// Unsubscribe from the dragStarted event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be unsubscribed from the event</param>
        public static void UnsubscribeFromDragStarted(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.dragStarted -= action;
        }

        /// <summary>
        /// Subscribe to the dragEnded event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be called when the event occurs</param>
        public static void SubscribeToDragEnded(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.dragEnded += action;
        }

        /// <summary>
        /// Unsubscribe from the dragEnded event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be unsubscribed from the event</param>
        public static void UnsubscribeFromDragEnded(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.dragEnded -= action;
        }

        /// <summary>
        /// Subscribe to the rayEntered event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be called when the event occurs</param>
        public static void SubscribeToRayEntered(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.rayEntered += action;
        }

        /// <summary>
        /// Unsubscribe from the rayEntered event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be unsubscribed from the event</param>
        public static void UnsubscribeFromRayEntered(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.rayEntered -= action;
        }

        /// <summary>
        /// Subscribe to the rayExited event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be called when the event occurs</param>
        public static void SubscribeToRayExited(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.rayExited += action;
        }

        /// <summary>
        /// Unsubscribe from the rayExited event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be unsubscribed from the event</param>
        public static void UnsubscribeFromRayExited(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.rayExited -= action;
        }

        /// <summary>
        /// Subscribe to the rayHovering event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be called when the event occurs</param>
        public static void SubscribeToRayHovering(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.rayHovering += action;
        }

        /// <summary>
        /// Unsubscribe from the rayHovering event
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="action">The action which will be unsubscribed from the event</param>
        public static void UnsubscribeFromRayHovering(this IUsesUIEvents user, Action<GameObject, TrackedDeviceEventData> action)
        {
            user.provider.rayHovering -= action;
        }
    }
}
