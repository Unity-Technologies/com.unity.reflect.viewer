using System.Collections.Generic;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Sends drop events to droppables and drop receivers.
    /// Drop receivers must be able to receive UI hover events. Droppables must be able to receive UI drags.
    /// When a droppable is dragged and the pointer hovers a drop receiver, a drop is attempted. The receiver must validate the object via
    /// the CanDrop method, and the droppable must provide a data object that is the thing being dropped.
    ///
    /// The drop receiver is chosen based on ray hover, so the droppable should not follow in front of the ray when being dragged if it will block the ray.
    /// </summary>
    public class DragAndDropModule : IModule, IUsesUIEvents
    {
        readonly Dictionary<IUIInteractor, IDroppable> m_Droppables = new Dictionary<IUIInteractor, IDroppable>();
        readonly Dictionary<IUIInteractor, IDropReceiver> m_DropReceivers = new Dictionary<IUIInteractor, IDropReceiver>();
        readonly Dictionary<IUIInteractor, GameObject> m_HoverObjects = new Dictionary<IUIInteractor, GameObject>();

        IProvidesUIEvents IFunctionalitySubscriber<IProvidesUIEvents>.provider { get; set; }

        /// <summary>
        /// Returns the current droppable data object for a particular UI interactor.
        /// This is the generic object data associated with an IDroppable being dragged by the UI interactor.
        /// </summary>
        /// <param name="uiInteractor">A UI interactor from the interaction system.</param>
        /// <returns>The droppable object data from the interactor's current IDroppable. If there is none, returns null.</returns>
        public object GetCurrentDropObject(IUIInteractor uiInteractor)
        {
            return m_Droppables.TryGetValue(uiInteractor, out var droppable) ? droppable.GetDropObject() : null;
        }

        /// <summary>
        /// Return the current drop receiver for a particular UI interactor.
        /// This is an IDropReceiver object that the UI interactor is currently hovering over while dragging an IDroppable object.
        /// </summary>
        /// <param name="uiInteractor">A UI interactor from the interaction system.</param>
        /// <returns>The IDropReceiver object that is currently being targeted by the interactor. If there is none, returns null.</returns>
        public IDropReceiver GetCurrentDropReceiver(IUIInteractor uiInteractor)
        {
            if (m_DropReceivers.TryGetValue(uiInteractor, out var dropReceiver))
                return dropReceiver;

            return null;
        }

        void SetCurrentDropReceiver(IUIInteractor uiInteractor, IDropReceiver dropReceiver)
        {
            if (dropReceiver == null)
                m_DropReceivers.Remove(uiInteractor);
            else
                m_DropReceivers[uiInteractor] = dropReceiver;
        }

        void OnRayEntered(GameObject gameObject, TrackedDeviceEventData eventData)
        {
            var dropReceiver = ComponentUtils<IDropReceiver>.GetComponent(gameObject);
            if (dropReceiver != null)
            {
                var rayOrigin = eventData.interactor;
                if (dropReceiver.CanDrop(GetCurrentDropObject(rayOrigin)))
                {
                    dropReceiver.OnDropHoverStarted();
                    m_HoverObjects[rayOrigin] = gameObject;
                    SetCurrentDropReceiver(rayOrigin, dropReceiver);
                }
            }
        }

        void OnRayExited(GameObject gameObject, TrackedDeviceEventData eventData)
        {
            if (!gameObject)
                return;

            var dropReceiver = ComponentUtils<IDropReceiver>.GetComponent(gameObject);
            if (dropReceiver != null)
            {
                var rayOrigin = eventData.interactor;
                if (m_HoverObjects.Remove(rayOrigin))
                {
                    dropReceiver.OnDropHoverEnded();
                    SetCurrentDropReceiver(rayOrigin, null);
                }
            }
        }

        void OnDragStarted(GameObject gameObject, TrackedDeviceEventData eventData)
        {
            var droppable = ComponentUtils<IDroppable>.GetComponent(gameObject);
            if (droppable != null)
                m_Droppables[eventData.interactor] = droppable;
        }

        void OnDragEnded(GameObject gameObject, TrackedDeviceEventData eventData)
        {
            var droppable = ComponentUtils<IDroppable>.GetComponent(gameObject);
            if (droppable != null)
            {
                var rayOrigin = eventData.interactor;
                m_Droppables.Remove(rayOrigin);

                var dropReceiver = GetCurrentDropReceiver(rayOrigin);
                var dropObject = droppable.GetDropObject();
                if (dropReceiver != null && dropReceiver.CanDrop(dropObject))
                    dropReceiver.ReceiveDrop(droppable.GetDropObject());
            }
        }

        void IModule.LoadModule()
        {
            this.SubscribeToRayEntered(OnRayEntered);
            this.SubscribeToRayExited(OnRayExited);
            this.SubscribeToDragStarted(OnDragStarted);
            this.SubscribeToDragEnded(OnDragEnded);
        }

        void IModule.UnloadModule()
        {
            this.UnsubscribeFromRayEntered(OnRayEntered);
            this.UnsubscribeFromRayExited(OnRayExited);
            this.UnsubscribeFromDragStarted(OnDragStarted);
            this.UnsubscribeFromDragEnded(OnDragEnded);
        }
    }
}
