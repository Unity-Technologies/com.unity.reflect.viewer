using Unity.XRTools.ModuleLoader;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    public interface IUsesNotifications : IFunctionalitySubscriber<IProvidesNotifications> { }

    /// <summary>
    /// Extension methods for implementors of IUsesNotifications
    /// </summary>
    public static class UsesNotificationsMethods
    {
        /// <summary>
        /// Sends a Notification to the user
        /// </summary>
        /// <param name="user">The functionality user</param>
        /// <param name="text">The text content of the notification.</param>
        /// <param name="parent">Optional attach parent, for specifying a custom location.</param>
        /// <returns>The created notification.</returns>
        public static INotification SendNotification(this IUsesNotifications user, string text, Transform parent = null)
        {
            return user.provider.SendNotification(text, parent);
        }
    }
}
