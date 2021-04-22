using System;
using System.Collections.Generic;
using Unity.SpatialFramework.Interaction;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.SpatialFramework.Input
{
    static class ModuleOrders
    {
        const int k_DefaultEarlyOrder = int.MinValue / 2;
        const int k_DefaultLateOrder = int.MaxValue / 2;
        public const int DeviceInputModuleOrder = k_DefaultEarlyOrder;
        public const int DeviceInputModuleBehaviorOrder = k_DefaultLateOrder + 1;
    }

    /// <summary>
    /// Module that manages input devices and sharing controls between different actions. Input actions are added to the module in the form of an InputActionAsset
    /// that is copied and returned to the user. Controls can be consumed and will disable other action maps bound to the same control. They will be consumed until the
    /// control is no longer actuated.
    /// Action listeners can be added for an action, so that their callbacks will only be invoked if the control is unconsumed or being invoked on the current consumer.
    /// </summary>
    [ModuleOrder(ModuleOrders.DeviceInputModuleOrder)]
    [ModuleBehaviorCallbackOrder(ModuleOrders.DeviceInputModuleBehaviorOrder)]
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public sealed class DeviceInputModule : MonoBehaviour, IModule, IProvidesInputActions, IProvidesDeviceHandedness
    {
        class InputActionsProcessor
        {
            public IUsesInputActions user;
            public InputActionAsset actionsAsset;
            public InputActionAsset inputActions;
            public int order;
        }

        class ActionListenerCollection
        {
            public IUsesInputActions user;
            public List<ActionEventCallback> startedCallbacks = new List<ActionEventCallback>();
            public List<ActionEventCallback> performedCallbacks = new List<ActionEventCallback>();
            public List<ActionEventCallback> cancelledCallbacks = new List<ActionEventCallback>();
        }

        struct EventContext
        {
            public InputAction action;
            public InputControl control;
        }

        const string k_BaseLayoutName = "XRController";
        const string k_LayoutOverridePrefix = "AppendHandUsagesFor";
        const string k_LayoutOverride = "{{\n\"name\" : \"{0}\",\n\"extend\" : \"{1}\",\n\"commonUsages\" : [\n\"DominantHand\",\n\"OffHand\"\n]\n}}";

        static readonly InternedString k_PrimaryUsage = new InternedString("DominantHand");
        static readonly InternedString k_SecondaryUsage = new InternedString("OffHand");

#pragma warning disable 649
        [SerializeField, Tooltip("The starting, and current handedness of the primary user. This configures the dominant and off hands for two handed actions an systems.")]
        XRControllerHandedness m_Handedness = XRControllerHandedness.RightHanded;
#pragma warning restore 649

        readonly Dictionary<InputControl, IUsesInputActions> m_LockedControls = new Dictionary<InputControl, IUsesInputActions>();
        readonly Dictionary<InputAction, ActionListenerCollection> m_ActionListeners = new Dictionary<InputAction, ActionListenerCollection>();
        readonly Dictionary<IUsesInputActions, List<EventContext>> m_AccumulatedStartedEvents = new Dictionary<IUsesInputActions, List<EventContext>>();
        readonly Dictionary<IUsesInputActions, List<EventContext>> m_AccumulatedPerformedEvents = new Dictionary<IUsesInputActions, List<EventContext>>();
        readonly Dictionary<IUsesInputActions, List<EventContext>> m_AccumulatedCanceledEvents = new Dictionary<IUsesInputActions, List<EventContext>>();

        readonly List<InputActionsProcessor> m_InputProcessors = new List<InputActionsProcessor>();

        readonly Dictionary<InputControl, List<InputAction>> m_ConsumedActions = new Dictionary<InputControl, List<InputAction>>();

        // Local method use only -- created here to reduce garbage collection
        readonly List<InputActionsProcessor> m_InputProcessorsCopy = new List<InputActionsProcessor>();
        readonly List<InputActionsProcessor> m_RemoveInputProcessorsCopy = new List<InputActionsProcessor>();
        readonly List<InputControl> k_RemoveList = new List<InputControl>();

        /// <inheritdoc />
        public event Action<XRControllerHandedness> handednessChanged;

        /// <inheritdoc />
        public XRControllerHandedness handedness
        {
            get { return m_Handedness; }
            set
            {
                if (m_Handedness != value)
                {
                    m_Handedness = value;
                    UpdateAllUsages();
                    handednessChanged?.Invoke(m_Handedness);
                }
            }
        }

#if UNITY_EDITOR
        static DeviceInputModule()
        {
            string baseLayoutOverrideName = $"{k_LayoutOverridePrefix}{k_BaseLayoutName}";
            InputSystem.RegisterLayoutOverride(string.Format(k_LayoutOverride, baseLayoutOverrideName, k_BaseLayoutName), baseLayoutOverrideName);
            InputSystem.onLayoutChange += OnLayoutChange;
        }

        static void OnLayoutChange(string layout, InputControlLayoutChange change)
        {
            if (change == InputControlLayoutChange.Added)
            {
                if (!layout.StartsWith(k_LayoutOverridePrefix) && InputSystem.IsFirstLayoutBasedOnSecond(layout, k_BaseLayoutName))
                {
                    var layoutName = $"{k_LayoutOverridePrefix}{layout}";

                    // This format is presuming a correct layoutOverride string that uses the right format tags.
                    var layoutJson = string.Format(k_LayoutOverride, layoutName, layout);
                    InputSystem.RegisterLayoutOverride(layoutJson, layoutName);
                }
            }
        }
#endif

        void IModule.LoadModule()
        {
            m_InputProcessors.Clear();
            InputSystem.onAfterUpdate += ProcessInput;
            InputSystem.onDeviceChange += OnDeviceChange;
            UpdateAllUsages();
        }

        void IModule.UnloadModule()
        {
            InputSystem.onAfterUpdate -= ProcessInput;
            InputSystem.onDeviceChange -= OnDeviceChange;
            m_InputProcessors.Clear();
        }

        InputActionAsset IProvidesInputActions.CreateActions(IUsesInputActions user, InputDevice device)
        {
            var actionsAsset = user.inputActionsAsset;
            if (actionsAsset == null)
                return null;

            var inputActions = Instantiate(actionsAsset);
            if (inputActions == null)
                return null;

            if (device != null)
                inputActions.devices = new[] { device };

            inputActions.Enable();

            var order = 0;

            var attributes = user.GetType().GetCustomAttributes(typeof(ProcessInputAttribute), true);
            var processInputAttribute = attributes.Length > 0 ? (ProcessInputAttribute)attributes[0] : null;
            if (processInputAttribute != null)
                order = processInputAttribute.order;

            var inputActionsProcessor = new InputActionsProcessor { user = user, inputActions = inputActions, order = order, actionsAsset = actionsAsset };
            m_InputProcessors.Add(inputActionsProcessor);
            m_InputProcessors.Sort((a, b) => a.order.CompareTo(b.order)); //TODO Implicit ordering tool stack priority

            // New action map needs to disable any actions bound to currently locked controls
            foreach (var lockedControl in m_LockedControls)
            {
                DisableActionsForControl(lockedControl.Key, inputActionsProcessor);
            }

            user.OnActionsCreated(inputActions);

            return inputActions;
        }

        /// <inheritdoc />
        public void RemoveActions(IUsesInputActions user)
        {
            var actionsAsset = user.inputActionsAsset;
            m_RemoveInputProcessorsCopy.Clear();
            m_RemoveInputProcessorsCopy.AddRange(m_InputProcessors);
            foreach (var processor in m_RemoveInputProcessorsCopy)
            {
                if (processor.user != user || processor.actionsAsset != actionsAsset)
                    continue;

                m_InputProcessors.Remove(processor);
                var input = processor.inputActions;

                foreach (var action in input)
                {
                    var actionActiveControl = action.activeControl;
                    if (actionActiveControl == null || !m_LockedControls.Remove(actionActiveControl))
                        continue;

                    if (m_ConsumedActions.ContainsKey(actionActiveControl))
                    {
                        foreach (var otherAction in m_ConsumedActions[actionActiveControl])
                        {
                            otherAction.Enable();
                        }

                        m_ConsumedActions.Remove(actionActiveControl);
                    }
                }

                UnityObjectUtils.Destroy(input);
            }
        }

        void IProvidesInputActions.ConsumeControl(InputControl control, IUsesInputActions consumer)
        {
            if (control == null)
                return;

            if (m_LockedControls.ContainsKey(control)) //TODO handle multiple things trying to lock the same control
                return;

            m_LockedControls.Add(control, consumer);
            foreach (var inputProcessor in m_InputProcessors)
            {
                var input = inputProcessor.inputActions;
                if (input == null)
                    continue;

                if (inputProcessor.user != consumer)
                    DisableActionsForControl(control, inputProcessor);
            }
        }

        void IProvidesInputActions.AddActionListeners(IUsesInputActions user, InputAction action, ActionEventCallback onStart, ActionEventCallback onPerformed, ActionEventCallback onCancel)
        {
            if (!m_ActionListeners.TryGetValue(action, out var listeners))
            {
                listeners = new ActionListenerCollection();
                listeners.user = user;
                m_ActionListeners.Add(action, listeners);

                action.started += OnActionStartedEvent;
                action.performed += OnActionPerformedEvent;
                action.canceled += OnActionCanceledEvent;
            }

            if (user != listeners.user)
            {
                Debug.LogError("Cannot add action listeners because another user is already listening to this action.");
                return;
            }

            if (onStart != null)
                listeners.startedCallbacks.Add(onStart);

            if (onPerformed != null)
                listeners.performedCallbacks.Add(onPerformed);

            if (onCancel != null)
                listeners.cancelledCallbacks.Add(onCancel);
        }

        void OnActionStartedEvent(InputAction.CallbackContext context)
        {
            SaveEventContext(context, m_AccumulatedStartedEvents);
        }

        void OnActionPerformedEvent(InputAction.CallbackContext context)
        {
            SaveEventContext(context, m_AccumulatedPerformedEvents);
        }

        void OnActionCanceledEvent(InputAction.CallbackContext context)
        {
            SaveEventContext(context, m_AccumulatedCanceledEvents);
        }

        void SaveEventContext(InputAction.CallbackContext context, Dictionary<IUsesInputActions, List<EventContext>> accumulatedEvents)
        {
            if (!m_ActionListeners.TryGetValue(context.action, out var listenerCollection))
                return;

            var user = listenerCollection.user;
            if (!accumulatedEvents.TryGetValue(user, out var events))
            {
                events = new List<EventContext>();
                accumulatedEvents.Add(user, events);
            }

            var eventContext = new EventContext { action = context.action, control = context.control };
            events.Add(eventContext);
        }

        void ExecuteActionListeners(EventContext context, List<ActionEventCallback> callbacks)
        {
            IUsesInputActions owner = null;
            if (context.control != null)
                m_LockedControls.TryGetValue(context.control, out owner);

            foreach (var listener in callbacks)
            {
                if (context.action == null)
                {
                    Debug.LogError("Context action is null when executing action listeners.");
                    continue;
                }

                var listenerTarget = listener.Target as IUsesInputActions; // TODO maybe allow for listeners to be methods that are not part of an IUsesDeviceInput

                // If the control has an owner that is not this listener, "consume" and do not invoke listener
                if (owner != null && listenerTarget != owner)
                    continue;

                // Event is not consumed, so try to invoke. Catch exceptions to prevent one listener from interrupting all other actions.
                try
                {
                    listener.Invoke();
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            }
        }

        void ProcessInput()
        {
            UnlockControls();
            m_InputProcessorsCopy.Clear();
            m_InputProcessorsCopy.AddRange(m_InputProcessors);
            foreach (var inputProcessor in m_InputProcessorsCopy)
            {
                //Check if processor has been destroyed
                if (inputProcessor.user == null)
                {
                    RemoveActions(inputProcessor.user);
                    continue;
                }

                SendEventsToInputProcessor(inputProcessor);
                inputProcessor.user.ProcessInput(inputProcessor.inputActions);
            }
        }

        void SendEventsToInputProcessor(InputActionsProcessor inputActionsProcessor)
        {
            if (m_AccumulatedStartedEvents.TryGetValue(inputActionsProcessor.user, out var startEvents))
            {
                foreach (var eventContext in startEvents)
                {
                    if (m_ActionListeners.TryGetValue(eventContext.action, out var listeners))
                        ExecuteActionListeners(eventContext, listeners.startedCallbacks);
                }

                startEvents.Clear(); //TODO  events that were consumed should be held onto in case a control wants to cancel a consumption and allow the past events to now be sent retroactively.
            }

            if (m_AccumulatedPerformedEvents.TryGetValue(inputActionsProcessor.user, out var performEvents))
            {
                foreach (var eventContext in performEvents)
                {
                    if (m_ActionListeners.TryGetValue(eventContext.action, out var listeners))
                        ExecuteActionListeners(eventContext, listeners.performedCallbacks);
                }

                performEvents.Clear();
            }

            if (m_AccumulatedCanceledEvents.TryGetValue(inputActionsProcessor.user, out var cancelEvents))
            {
                foreach (var eventContext in cancelEvents)
                {
                    if (m_ActionListeners.TryGetValue(eventContext.action, out var listeners))
                        ExecuteActionListeners(eventContext, listeners.cancelledCallbacks);
                }

                cancelEvents.Clear();
            }
        }

        void UnlockControls()
        {
            k_RemoveList.Clear();
            foreach (var lockedControl in m_LockedControls)
            {
                // Locked controls that have returned to default value are automatically unlocked
                if (lockedControl.Key.device.added)
                {
                    const float unlockActuationThreshold = 0.01f;
                    if (lockedControl.Key.IsActuated(unlockActuationThreshold))
                        continue;
                }

                // Remove separately, since we cannot remove while iterating
                k_RemoveList.Add(lockedControl.Key);
            }

            // Unlock controls and reenable any actions that were disabled by them.
            foreach (var inputControl in k_RemoveList)
            {
                m_LockedControls.Remove(inputControl);
                if (!m_ConsumedActions.TryGetValue(inputControl, out var consumedActionsList))
                    continue;

                foreach (var action in consumedActionsList)
                {
                    action.Enable();
                }

                m_ConsumedActions.Remove(inputControl);
            }
        }

        void DisableActionsForControl(InputControl control, InputActionsProcessor inputActionsProcessor)
        {
            foreach (var action in inputActionsProcessor.inputActions)
            {
                if (action == null)
                    continue;

                foreach (var otherControl in action.controls) //TODO Should use active control instead of all controls?
                {
                    if (!control.path.Contains(otherControl.path) && !otherControl.path.Contains(control.path))
                        continue;

                    // Disable will send a cancelled callback to started actions, but that will be consumed for our listeners. Manually invoke those callbacks now
                    action.Disable();
                    var startedOrPerformed = action.phase == InputActionPhase.Started || action.phase == InputActionPhase.Performed;
                    if (startedOrPerformed && m_ActionListeners.TryGetValue(action, out var listeners))
                    {
                        foreach (var listener in listeners.cancelledCallbacks)
                        {
                            try
                            {
                                listener.Invoke();
                            }
                            catch (Exception exception)
                            {
                                Debug.LogException(exception);
                            }
                        }
                    }

                    if (m_ConsumedActions.TryGetValue(control, out var list))
                    {
                        if (!list.Contains(action))
                            list.Add(action);
                    }
                    else
                    {
                        m_ConsumedActions.Add(control, new List<InputAction>() { action });
                    }
                }
            }
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            EditorApplication.delayCall += UpdateAllUsages;
        }
#endif

        void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (change == InputDeviceChange.Added)
            {
                UpdateUsages(device);
            }
        }

        void UpdateAllUsages()
        {
            var devices = InputSystem.devices;
            foreach (var device in devices)
            {
                UpdateUsages(device);
            }
        }

        void UpdateUsages(InputDevice device)
        {
            var usages = device.usages;
            if (usages.Contains(CommonUsages.LeftHand))
            {
                InputSystem.RemoveDeviceUsage(device, m_Handedness == XRControllerHandedness.LeftHanded ? k_SecondaryUsage : k_PrimaryUsage);
                InputSystem.AddDeviceUsage(device, m_Handedness == XRControllerHandedness.LeftHanded ? k_PrimaryUsage : k_SecondaryUsage);
            }
            else if (usages.Contains(CommonUsages.RightHand))
            {
                InputSystem.RemoveDeviceUsage(device, m_Handedness == XRControllerHandedness.RightHanded ? k_SecondaryUsage : k_PrimaryUsage);
                InputSystem.AddDeviceUsage(device, m_Handedness == XRControllerHandedness.RightHanded ? k_PrimaryUsage : k_SecondaryUsage);
            }
        }

        void IFunctionalityProvider.LoadProvider() { }

        void IFunctionalityProvider.UnloadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesInputActions>(obj);
            this.TryConnectSubscriber<IProvidesDeviceHandedness>(obj);
        }
    }
}
