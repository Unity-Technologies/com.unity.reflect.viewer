using System;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Notification information for editing returned notifications.
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// The local position of the notification for custom movement.
        /// </summary>
        Vector3 position { set; }

        /// <summary>
        /// The displayed text of the notification at the current time.
        /// </summary>
        string text { set; }

        /// <summary>
        /// Boolean value notifying that this notification is done presenting its information, and can be destroyed.
        /// </summary>
        bool isDone { get; }
    }
}
