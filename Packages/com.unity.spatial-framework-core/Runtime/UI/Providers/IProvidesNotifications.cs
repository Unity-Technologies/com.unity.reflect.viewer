using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Provides the ability to send notifications to the user.
    /// </summary>
    public interface IProvidesNotifications : IFunctionalityProvider
    {
        /// <summary>
        /// Sends a Notification to the user
        /// </summary>
        /// <param name="text">The text content of the notification.</param>
        /// <param name="parent">Optional attach parent, for specifying a custom location.</param>
        /// <returns>The created notification.</returns>
        INotification SendNotification(string text, Transform parent = null);
    }
}
