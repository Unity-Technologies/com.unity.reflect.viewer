using System;
using Unity.XRTools.ModuleLoader;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Enum representing the dominant hand for an XR User.
    /// Used by classes implementing <see cref="IUsesDeviceHandedness"/> to get and set a user's handedness.
    /// </summary>
    public enum XRControllerHandedness
    {
        RightHanded,
        LeftHanded
    }

    /// <summary>
    /// Gives a decorated class access to a user's dominant hand information. Allows implementors to get and set the current handedness as well as listen for handedness changes.
    /// </summary>
    public interface IUsesDeviceHandedness : IFunctionalitySubscriber<IProvidesDeviceHandedness>
    {}

    /// <summary>
    /// Extension methods for implementors of IUsesDeviceHandedness
    /// </summary>
    public static class UsesDeviceHandednessMethods
    {
        /// <summary>
        /// Subscribes to get callbacks whenever a primary user's handedness changes. This will not notify the caller of the current handedness.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="action">The method that will be called on handedness changes.</param>
        public static void SubscribeToHandednessChanged(this IUsesDeviceHandedness user, Action<XRControllerHandedness> action)
        {
            user.provider.handednessChanged += action;
        }

        /// <summary>
        /// Unsubscribes to handedness changes. If the callback was not subscribed, does nothing.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="action">The method that was registered via <see cref="SubscribeToHandednessChanged"/>.</param>
        public static void UnsubscribeToHandednessChanged(this IUsesDeviceHandedness user, Action<XRControllerHandedness> action)
        {
            user.provider.handednessChanged -= action;
        }

        /// <summary>
        /// Sets the handedness of the primary user. If the handedness changes from current will trigger events registered via <see cref="UsesDeviceHandednessMethods.SubscribeToHandednessChanged"/>.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <param name="value">The desired handedness for the primary user.</param>
        public static void SetHandedness(this IUsesDeviceHandedness user, XRControllerHandedness value)
        {
            user.provider.handedness = value;
        }

        /// <summary>
        /// Returns the current handedness set for this user.
        /// </summary>
        /// <param name="user">The functionality user.</param>
        /// <returns>The current handedness of the primary user.</returns>
        public static XRControllerHandedness GetHandedness(this IUsesDeviceHandedness user)
        {
            return user.provider.handedness;
        }
    }
}
