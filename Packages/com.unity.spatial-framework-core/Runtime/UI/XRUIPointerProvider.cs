using System;
using System.Collections.Generic;
using Unity.SpatialFramework.Input;
using Unity.XRTools.ModuleLoader;
using Unity.XRTools.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace Unity.SpatialFramework.Interaction
{
    [ProcessInput(-999)]
    [ScriptableSettingsPath("Assets/SpatialFramework/Settings")]
    class XRUIPointerProvider : ScriptableSettings<XRUIPointerProvider>, IModule, IProvidesXRPointers,
        IProvidesUIEvents, IUsesInputActions
    {
        class RaycastSource : IRaycastSource
        {
            internal GameObject previousHover;
            internal GameObject previousDrag;
            internal Func<IRaycastSource, bool> isValid;
            internal float maxDistance;
            internal InputActionAsset inputActions;
            internal InputDevice inputDevice;
            internal InputAction uiPressAction;
            public float distance { get; set; }
            public GameObject hoveredGameObject { get; set; }
            public GameObject dragGameObject { get; set; }
            public bool hasObject { get; set; }
            public bool blocked { get; set; }
            public Transform rayOrigin { get; set; }
        }

        const string k_ActionMapGuid = "d03c7cf65fc952444ab0ae6b6e912000";

#pragma warning disable 649
        [SerializeField]
        InputActionAsset m_UIActionMap;
#pragma warning restore 649

        // TODO uncomment this field and usages when API is available in XRI
        // UIInputModule m_UIInputModule;
        LayerMask m_UIRaycastMask;

        Dictionary<Transform, RaycastSource> m_RayOriginToSource = new Dictionary<Transform, RaycastSource>();
        Dictionary<int, RaycastSource> m_PointerIdToSource = new Dictionary<int, RaycastSource>();

        public event Action<GameObject, TrackedDeviceEventData> rayEntered;
        public event Action<GameObject, TrackedDeviceEventData> rayHovering;
        public event Action<GameObject, TrackedDeviceEventData> rayExited;
        public event Action<GameObject, TrackedDeviceEventData> dragStarted;
        public event Action<GameObject, TrackedDeviceEventData> dragEnded;

        public InputActionAsset inputActionsAsset { get => m_UIActionMap; }

        public IProvidesInputActions provider { get; set; }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (m_UIActionMap != null)
                return;

            var path = AssetDatabase.GUIDToAssetPath(k_ActionMapGuid);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Could not find default input action asset");
                return;
            }

            var asset = AssetDatabase.LoadAssetAtPath<InputActionAsset>(path);
            m_UIActionMap = asset;
        }
#endif

        public void LoadModule()
        {
            m_UIRaycastMask = LayerMask.GetMask("UI");
        }

        public void UnloadModule()
        {
            // Temporarily disabled until XRI input module allows this
            // if (m_UIInputModule != null)
            //     m_UIInputModule.customRaycast -= ProcessRaycastResults;
        }

        public void LoadProvider() { }

        public void UnloadProvider() { }

        public void ConnectSubscriber(object obj)
        {
            this.TryConnectSubscriber<IProvidesUIEvents>(obj);
            this.TryConnectSubscriber<IProvidesXRPointers>(obj);
        }

        void ProcessRaycastResults(PointerEventData eventData, List<RaycastResult> raycastResults)
        {
            if (m_PointerIdToSource.TryGetValue(eventData.pointerId, out var source))
            {
                RaycastResult firstResult = default;
                foreach (var raycast in raycastResults)
                {
                    if (raycast.gameObject == null || !raycast.isValid)
                        continue;

                    firstResult = raycast;
                    break;
                }

                source.hoveredGameObject = eventData.pointerEnter;
                source.dragGameObject = eventData.pointerDrag;
                source.hasObject = source.hoveredGameObject != null || source.dragGameObject != null;

                if (firstResult.isValid)
                {
                    source.distance = Vector3.Distance(source.rayOrigin.position, firstResult.worldPosition);

                    if (source.blocked // Check if source is blocked
                        || (source.uiPressAction != null && !source.uiPressAction.enabled) // Check if ui press action is disabled/consumed
                        || !source.isValid(source)) // Check if source is still invalid for some other reason
                    {
                        // If raycast source has been blocked or invalidated after seeing the result, clear the results so that the event system does not send events
                        raycastResults.Clear();
                        source.hasObject = false;
                    }
                }
                else
                {
                    source.distance = source.maxDistance;
                    source.hasObject = false;
                }

                if (eventData is TrackedDeviceEventData trackedDeviceEventData)
                    SendTrackedDeviceUIEvents(source, trackedDeviceEventData);
            }
        }

        public float GetHoverOverUIDistance(Transform rayOrigin)
        {
            if (m_RayOriginToSource.ContainsKey(rayOrigin))
                return m_RayOriginToSource[rayOrigin].distance;

            throw new ArgumentException("Cannot find distance to UI because the ray origin is not a register UI raycaster.");
        }

        public void SetUIBlockedForRayOrigin(Transform rayOrigin, bool blocked)
        {
            if (m_RayOriginToSource.TryGetValue(rayOrigin, out var source))
            {
                source.blocked = blocked;
            }
        }

        public bool IsHoveringOverUI(Transform rayOrigin)
        {
            if (m_RayOriginToSource.TryGetValue(rayOrigin, out var source))
                return source.hasObject;

            return false;
        }

        /// <inheritdoc />
        public GameObject CreateXRPointer(InputDevice device, Transform parent, Transform rayOrigin = null, Func<IRaycastSource, bool> validationCallback = null, GameObject controller = null)
        {
            GameObject xriController;
            if (controller == null)
            {
                xriController = new GameObject($"XR Pointer: {device.displayName} ");
                xriController.transform.SetParent(parent);
            }
            else
            {
                xriController = controller;
            }

            var actionBasedController = xriController.GetComponent<ActionBasedController>();
            if (actionBasedController == null)
                actionBasedController = xriController.AddComponent<ActionBasedController>();

            var rayInteractor = xriController.GetComponent<XRRayInteractor>();
            if (rayInteractor == null)
                rayInteractor = xriController.AddComponent<XRRayInteractor>();

            if (rayOrigin != null)
            {
                // Bug, an attach transform is automatically created so I need to destroy it to avoid unused gameobjects.
                var oldAttachTransform = rayInteractor.attachTransform;
                if (oldAttachTransform != null)
                    Destroy(oldAttachTransform.gameObject);

                SetupRayInteractorProperties(rayInteractor);

                rayInteractor.attachTransform = rayOrigin;
            }
            else
            {
                SetupRayInteractorProperties(rayInteractor);
                rayOrigin = rayInteractor.attachTransform;
            }


            var pointerId = -1;
            if (rayInteractor.TryGetUIModel(out var trackedDeviceModel))
                pointerId = trackedDeviceModel.pointerId;

            if (rayOrigin == null)
                rayOrigin = xriController.transform;

            var source = new RaycastSource { rayOrigin = rayOrigin, inputDevice = device, isValid = validationCallback, maxDistance = rayInteractor.maxRaycastDistance };
            m_RayOriginToSource.Add(source.rayOrigin, source);
            m_PointerIdToSource.Add(pointerId, source);

            var input = this.CreateActions(device);
            source.inputActions = input;
            AssignUIActionMapToController(actionBasedController, source);

            return xriController;
        }

        void SetupRayInteractorProperties(XRRayInteractor rayInteractor)
        {
            rayInteractor.raycastMask = m_UIRaycastMask;
            rayInteractor.enableUIInteraction = true;
            rayInteractor.raycastTriggerInteraction = QueryTriggerInteraction.Collide;
            rayInteractor.allowAnchorControl = false;
        }

        void IUsesInputActions.OnActionsCreated(InputActionAsset input){}

        static void AssignUIActionMapToController(ActionBasedController actionBasedController, RaycastSource source)
        {
            var input = source.inputActions;
            actionBasedController.positionAction = new InputActionProperty(input.FindAction("Position"));
            actionBasedController.rotationAction = new InputActionProperty(input.FindAction("Rotation"));

            source.uiPressAction = input.FindAction("Click");
            var click = new InputActionProperty(source.uiPressAction);
            actionBasedController.uiPressAction = click;

            var select = new InputActionProperty(input.FindAction("Select"));
            actionBasedController.selectAction = select;
        }

        // TOMB: This should be handled by the InteractionProxy, or maybe the DeviceInputModule (ToolModule also has this information)
        // This needs revisiting when IProxy becomes a core part of Spatial.
        public InputDevice GetDeviceForRayOrigin(Transform rayOrigin)
        {
            if (m_RayOriginToSource.TryGetValue(rayOrigin, out var source))
                return source.inputDevice;

            return null;
        }

        void SendTrackedDeviceUIEvents(RaycastSource source, TrackedDeviceEventData trackedDeviceEventData)
        {
            if (!source.hasObject)
            {
                if (source.previousHover != null)
                {
                    rayExited?.Invoke(source.previousHover, trackedDeviceEventData);
                    source.previousHover = null;
                }

                return;
            }

            var pointerEnter = source.hoveredGameObject;
            if (pointerEnter != source.previousHover)
            {
                rayExited?.Invoke(source.previousHover, trackedDeviceEventData);
                source.previousHover = pointerEnter;
                if (pointerEnter != null)
                {
                    rayEntered?.Invoke(pointerEnter, trackedDeviceEventData);
                }
            }

            if (pointerEnter != null)
            {
                rayHovering?.Invoke(pointerEnter, trackedDeviceEventData);
            }

            var pointerDrag = source.dragGameObject;

            if (pointerDrag != source.previousDrag)
            {
                if (source.previousDrag != null)
                    dragEnded?.Invoke(source.previousDrag, trackedDeviceEventData);

                if (pointerDrag != null)
                    dragStarted?.Invoke(pointerDrag, trackedDeviceEventData);

                source.previousDrag = pointerDrag;
            }
        }

        public void ProcessInput(InputActionAsset input)
        {
            // Temporarily disabled until XRI input module allows this
            // if (m_UIInputModule == null && EventSystem.current != null && EventSystem.current.currentInputModule != null && EventSystem.current.currentInputModule is UIInputModule uiInputModule)
            // {
            //     m_UIInputModule = uiInputModule;
            //     m_UIInputModule.customRaycast += ProcessRaycastResults;
            // }

            foreach (var source in m_PointerIdToSource)
            {
                if (source.Value.hasObject && source.Value.uiPressAction.activeControl != null)
                    this.ConsumeControl(source.Value.uiPressAction.activeControl);
            }
        }
    }
}
