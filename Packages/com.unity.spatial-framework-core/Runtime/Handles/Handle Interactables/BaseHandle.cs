using System;
using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils.GUI;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Unity.SpatialFramework.Handles
{
    /// <summary>
    /// Base class for providing draggable handles in 3D
    /// </summary>
    public class BaseHandle : XRBaseInteractable, ISelectionFlags, IDropReceiver, IDroppable, IUsesViewerScale
    {
        protected const int k_DefaultCapacity = 2; // i.e. 2 controllers
        static readonly List<XRBaseInteractable> k_Targets = new List<XRBaseInteractable>();

        /// <summary>
        /// Specifies the kinds of selection this handle allows
        /// </summary>
        public SelectionFlags selectionFlags
        {
            get { return m_SelectionFlags; }
            set { m_SelectionFlags = value; }
        }

        [SerializeField, FlagsProperty, Tooltip("Specifies the kinds of selection this handle allows. By default, both ray and direct interaction are allowed.")]
        SelectionFlags m_SelectionFlags = SelectionFlags.Ray | SelectionFlags.Direct;

        protected readonly List<XRBaseInteractor> m_SelectingInteractors = new List<XRBaseInteractor>(k_DefaultCapacity);
        protected readonly List<XRBaseInteractor> m_HoveringInteractors = new List<XRBaseInteractor>(k_DefaultCapacity);
        readonly Dictionary<XRBaseInteractor, HandleEventData> m_HandleEventData = new Dictionary<XRBaseInteractor, HandleEventData>();

        /// <summary>
        /// Whether the handle is currently being hovered
        /// </summary>
        public bool hasHoverSource => m_HoveringInteractors.Count > 0;

        /// <summary>
        /// Whether the handle is currently being dragged
        /// </summary>
        public bool hasDragSource => m_SelectingInteractors.Count > 0;

        /// <summary>
        /// Function that determines whether an object can be dropped onto this handle
        /// </summary>
        public Func<BaseHandle, object, bool> canDrop { private get; set; }

        /// <summary>
        /// Action when the handle receives a dropped object
        /// </summary>
        public Action<BaseHandle, object> receiveDrop { private get; set; }

        /// <summary>
        /// Function that returns the data object that is dropped when this handle is dragged
        /// </summary>
        public Func<BaseHandle, object> getDropObject { private get; set; }

        /// <summary>
        /// Event when a droppable object starts hovering over the handle
        /// </summary>
        public event Action<BaseHandle> dropHoverStarted;

        /// <summary>
        /// Event when a droppable object stops hovering over the handle
        /// </summary>
        public event Action<BaseHandle> dropHoverEnded;

        /// <summary>
        /// Event when the handle starts being dragged
        /// </summary>
        public event Action<BaseHandle, HandleEventData> dragStarted;

        /// <summary>
        /// Event every frame that the handle is being dragged
        /// </summary>
        public event Action<BaseHandle, HandleEventData> dragging;

        /// <summary>
        /// Event when the handle stops being dragged
        /// </summary>
        public event Action<BaseHandle, HandleEventData> dragEnded;

        /// <summary>
        /// Event when a pointer presses down on the handle
        /// </summary>
        public event Action<BaseHandle, HandleEventData> pointerDown;

        /// <summary>
        /// Event when a pointer stops being pressed on the handle
        /// </summary>
        public event Action<BaseHandle, HandleEventData> pointerUp;

        /// <summary>
        /// Event when the handle starts being hovered
        /// </summary>
        public event Action<BaseHandle, HandleEventData> hoverStarted;

        /// <summary>
        /// Event every frame that the handle is being hovered
        /// </summary>
        public event Action<BaseHandle, HandleEventData> hovering;

        /// <summary>
        /// Event when the handle stops being hovered
        /// </summary>
        public event Action<BaseHandle, HandleEventData> hoverEnded;

        IProvidesViewerScale IFunctionalitySubscriber<IProvidesViewerScale>.provider { get; set; }

        protected override void OnDisable()
        {
            if (m_HoveringInteractors.Count > 0 || m_SelectingInteractors.Count > 0)
            {
                var sources = new List<XRBaseInteractor>(m_HoveringInteractors);
                m_HoveringInteractors.Clear();
                foreach (var interactor in sources)
                {
                    var eventData = GetHandleEventData(interactor);
                    OnHandleHoverEnded(eventData);
                }

                sources.Clear();
                sources.AddRange(m_SelectingInteractors);
                m_SelectingInteractors.Clear();
                foreach (var interactor in sources)
                {
                    var eventData = GetHandleEventData(interactor);
                    OnHandleDragEnded(eventData);
                    OnHandlePointerUp(eventData);
                }
            }

            base.OnDisable();
        }

        protected virtual HandleEventData GetHandleEventData(XRBaseInteractor interactor)
        {
            var rayOrigin = GetRayOriginFromInteractor(interactor);

            if (!m_HandleEventData.TryGetValue(interactor, out var eventData))
                m_HandleEventData.Add(interactor, eventData = new HandleEventData(null, false));

            eventData.rayOrigin = rayOrigin;

            if (interactor is ILineRenderable lineRenderable && lineRenderable.TryGetHitInfo(out var position, out _, out _, out _))
            {
                eventData.direct = false;
                eventData.worldPosition = position;
            }
            else
            {
                eventData.direct = true;
                eventData.worldPosition = rayOrigin.position;
            }

            return eventData;
        }

        static Transform GetRayOriginFromInteractor(XRBaseInteractor interactor)
        {
            return interactor.attachTransform;
        }

        /// <inheritdoc />
        public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
        {
            base.ProcessInteractable(updatePhase);
            if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
                return;

            foreach (var interactor in m_HoveringInteractors)
            {
                var handleEventData = GetHandleEventData(interactor);
                OnHandleHovering(handleEventData);
            }

            foreach (var interactor in m_SelectingInteractors)
            {
                var handleEventData = GetHandleEventData(interactor);
                OnHandleDragging(handleEventData);
            }
        }

        /// <inheritdoc />
        public override bool IsHoverableBy(XRBaseInteractor interactor)
        {
            if (!enabled || !base.IsHoverableBy(interactor))
                return false;

            // Check if hover is too far (not direct and ray selection is not allowed)
            var eventData = GetHandleEventData(interactor);
            if (!eventData.direct && (selectionFlags & SelectionFlags.Ray) == 0)
                return false;

            // Check if hover is too close (direct event and direct selection is not allowed)
            if (eventData.direct && (selectionFlags & SelectionFlags.Direct) == 0)
                return false;

            // Target list is cleared by GetValidTargets
            interactor.GetValidTargets(k_Targets);

            // Only hover if the handle is the first (nearest) target
            var nearest = k_Targets.IndexOf(this) == 0;

            // Only hover if the interactor select is not active yet, or it's already hovering or selecting this
            var canHover = !interactor.isSelectActive || m_HoveringInteractors.Contains(interactor) || m_SelectingInteractors.Contains(interactor);
            return nearest && canHover;
        }

        /// <inheritdoc />
        public override bool IsSelectableBy(XRBaseInteractor interactor)
        {
            if (!base.IsSelectableBy(interactor))
                return false;

            // Only select if already hovering or selecting
            return m_HoveringInteractors.Contains(interactor) || m_SelectingInteractors.Contains(interactor);
        }

        /// <inheritdoc />
        protected override void OnHoverEntered(HoverEnterEventArgs args)
        {
            base.OnHoverEntered(args);
            var handleEventData = GetHandleEventData(args.interactor);
            m_HoveringInteractors.Add(args.interactor);
            OnHandleHoverStarted(handleEventData);
        }

        /// <inheritdoc />
        protected override void OnHoverExited(HoverExitEventArgs args)
        {
            base.OnHoverExited(args);
            var handleEventData = GetHandleEventData(args.interactor);
            if (m_HoveringInteractors.Remove(args.interactor))
                OnHandleHoverEnded(handleEventData);
        }

        /// <inheritdoc />
        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            base.OnSelectEntered(args);
            var handleEventData = GetHandleEventData(args.interactor);
            m_SelectingInteractors.Add(args.interactor);

            OnHandleDragStarted(handleEventData); //TODO add min threshold to start dragging
            OnHandlePointerDown(handleEventData);
        }

        /// <inheritdoc />
        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            base.OnSelectExited(args);

            if (m_SelectingInteractors.Remove(args.interactor))
            {
                var handleEventData = GetHandleEventData(args.interactor);
                OnHandleDragEnded(handleEventData);
                OnHandlePointerUp(handleEventData);
            }
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandleHoverStarted(HandleEventData eventData)
        {
            if (hoverStarted != null)
                hoverStarted(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandleHovering(HandleEventData eventData)
        {
            if (hovering != null)
                hovering(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandleHoverEnded(HandleEventData eventData)
        {
            if (hoverEnded != null)
                hoverEnded(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandlePointerDown(HandleEventData eventData)
        {
            if (pointerDown != null)
                pointerDown(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandleDragStarted(HandleEventData eventData)
        {
            if (dragStarted != null)
                dragStarted(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandleDragging(HandleEventData eventData)
        {
            if (dragging != null)
                dragging(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandleDragEnded(HandleEventData eventData)
        {
            if (dragEnded != null)
                dragEnded(this, eventData);
        }

        /// <summary>
        /// Override to modify event data prior to raising event (requires calling base method at the end)
        /// </summary>
        protected virtual void OnHandlePointerUp(HandleEventData eventData)
        {
            if (pointerUp != null)
                pointerUp(this, eventData);
        }

        object IDroppable.GetDropObject()
        {
            if (!this) // If this handle has been destroyed, return null;
                return null;

            if (getDropObject != null)
                return getDropObject(this);

            return null;
        }

        bool IDropReceiver.CanDrop(object dropObject)
        {
            if (canDrop != null)
                return canDrop(this, dropObject);

            return false;
        }

        void IDropReceiver.ReceiveDrop(object dropObject)
        {
            if (receiveDrop != null)
                receiveDrop(this, dropObject);
        }

        void IDropReceiver.OnDropHoverStarted()
        {
            if (dropHoverStarted != null)
                dropHoverStarted(this);
        }

        void IDropReceiver.OnDropHoverEnded()
        {
            if (dropHoverEnded != null)
                dropHoverEnded(this);
        }
    }
}
