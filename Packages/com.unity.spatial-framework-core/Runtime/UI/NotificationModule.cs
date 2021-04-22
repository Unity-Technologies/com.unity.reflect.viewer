using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Module for popping information up to the user.
    /// </summary>
    [ScriptableSettingsPath("Assets/SpatialFramework/Settings")]
    public class NotificationModule : ScriptableSettings<NotificationModule>, IProvidesNotifications, IModuleBehaviorCallbacks
    {
#pragma warning disable 649
        [SerializeField, Tooltip("Desired default screen space location of the notification.")]
        Vector2 m_DisplayPosition = new Vector2(0.5f, 0.25f);

        [SerializeField, Tooltip("Default distance in front of the camera to display a notification.")]
        float m_DisplayDistance = 1.5f;

        [SerializeField]
        GameObject m_NotificationPrefab;
#pragma warning restore 649

        // TODO, Recycle Notifications
        readonly List<INotification> m_ActiveNotifications = new List<INotification>();

        void IFunctionalityProvider.ConnectSubscriber(object obj) { this.TryConnectSubscriber<IProvidesNotifications>(obj); }

        /// <inheritdoc />
        public INotification SendNotification(string text, Transform parent = null)
        {
            if (m_NotificationPrefab == null)
                return null;

            var newNotificationObject = Instantiate(m_NotificationPrefab);
            var notificationTransform = newNotificationObject.transform;
            if (parent != null)
                notificationTransform.SetParent(parent);

            // TODO: Move to CameraUtils/external set once it's migrated down the the Spatial Framework.
            var camera = Camera.main;
            if (camera != null)
            {
                var displayRay = camera.ViewportPointToRay(m_DisplayPosition);
                var notificationPosition = displayRay.origin + (displayRay.direction * m_DisplayDistance);

                notificationTransform.position = notificationPosition;
                notificationTransform.forward = displayRay.direction;
            }

            var notification = newNotificationObject.GetComponent<INotification>();
            notification.text = text;

            m_ActiveNotifications.Add(notification);
            return notification;
        }

        void IModuleBehaviorCallbacks.OnBehaviorUpdate()
        {
            for (var i = 0; i < m_ActiveNotifications.Count; i++)
            {
                var activeNotification = m_ActiveNotifications[i];
                if (activeNotification.isDone)
                {
                    if (activeNotification is Component notificationComponent)
                        UnityObjectUtils.Destroy(notificationComponent.gameObject);
                    m_ActiveNotifications.RemoveAt(i--);
                }
            }
        }

        void IModule.LoadModule() { }
        void IModule.UnloadModule() { }

        void IFunctionalityProvider.LoadProvider() { }
        void IFunctionalityProvider.UnloadProvider() { }

        void IModuleBehaviorCallbacks.OnBehaviorAwake() { }
        void IModuleBehaviorCallbacks.OnBehaviorEnable() { }
        void IModuleBehaviorCallbacks.OnBehaviorStart() { }
        void IModuleBehaviorCallbacks.OnBehaviorDisable() { }
        void IModuleBehaviorCallbacks.OnBehaviorDestroy() { }
    }
}
