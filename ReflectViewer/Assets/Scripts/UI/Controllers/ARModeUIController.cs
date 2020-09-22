using System;
using System.Collections;
using SharpFlux;
#if UNITY_EDITOR
using UnityEditor.MARS.Simulation;
#endif
using Unity.MARS;
using Unity.MARS.Data;
using Unity.MARS.Providers;
using Unity.MARS.Query;
using Unity.XRTools.ModuleLoader;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace Unity.Reflect.Viewer.UI
{
    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    public class ARModeUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        InputActionAsset m_InputActionAsset;
        [SerializeField, Tooltip("Rate at which to rotate object with a drag.")]
        float m_RotationRateDegreesDrag = 100.0f;

        public Canvas m_InstructionUICanvas;
        public Raycaster m_Raycaster;
#pragma warning restore CS0649

        private const string instructionFindAPlaneText = "Pan your device to find a horizontal surface...";
        private const string instructionConfirmPlacementText = "Adjust your model as desired and press OK.";

        bool m_ToolbarsEnabled;
        NavigationMode? m_CachedNavigationMode;
        InstructionUI? m_CachedInstructionUI;
        bool? m_CachedOperationCancelled;

        MARSCamera m_MARSCamera;
        UniversalAdditionalCameraData m_CameraData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.arStateChanged += OnARStateDataChanged;

            m_MARSCamera = Camera.main.GetComponent<MARSCamera>();
            m_CameraData = Camera.main.GetComponent<UniversalAdditionalCameraData>();

            m_InputActionAsset["Placement Rotate Action"].performed += OnPlacementRotateAction;
        }

        private void OnPlacementRotateAction(InputAction.CallbackContext context)
        {
            if (m_CachedInstructionUI != InstructionUI.ConfirmPlacement)
                return;

            if (context.control.IsPressed())
            {
                var delta = context.ReadValue<Vector2>();

                var forward = m_MARSCamera.transform.forward;
                var worldToVerticalOrientedDevice = Quaternion.Inverse(Quaternion.LookRotation(forward, Vector3.up));
                var deviceToWorld = m_MARSCamera.transform.rotation;
                var rotatedDelta = worldToVerticalOrientedDevice * deviceToWorld * delta;

                var rotationAmount = -1.0f * (rotatedDelta.x / Screen.dpi) * m_RotationRateDegreesDrag;

                UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelRotation, new Vector3(0.0f, rotationAmount, 0.0f)));
            }
        }

        private void Start()
        {
            // since this starts on async scene load, needs to be up to date
            OnStateDataChanged(UIStateManager.current.stateData);
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_ToolbarsEnabled != stateData.toolbarsEnabled)
            {
                m_ToolbarsEnabled = stateData.toolbarsEnabled;
            }

            if (m_CachedNavigationMode != stateData.navigationState.navigationMode && m_ToolbarsEnabled)
            {
                m_CachedNavigationMode = stateData.navigationState.navigationMode;
                if (m_CachedNavigationMode == NavigationMode.AR)
                {
                    StartCoroutine(ResetInstructionUI());
                }
                else
                {
                    var m_actionMap = m_InputActionAsset.FindActionMap("AR", true);
                    m_actionMap.Disable();

                    // disable SimulationView
                    EnableSimulationViewInEditor(false);

                    // enable RootNode
                    m_MARSCamera.enabled = false;
                    // return to default Renderer
                    m_CameraData.SetRenderer((int)UniversalRenderer.DefaultForwardRenderer);


                    UIStateManager.current.m_RootNode.SetActive(true);

                    if (UIStateManager.current.SessionReady())
                    {
                        // Pause MARSSession
                        UIStateManager.current.StopDetectingPlanes();
                        UIStateManager.current.StopDetectingPoints();
                        UIStateManager.current.PauseSession();
                    }
                }
            }

            if (m_CachedOperationCancelled != stateData.operationCancelled && m_ToolbarsEnabled)
            {
                m_CachedOperationCancelled = stateData.operationCancelled;
                if (m_CachedOperationCancelled == true)
                {
                    switch (m_CachedInstructionUI)
                    {
                        case InstructionUI.ConfirmPlacement:
                        case InstructionUI.AimToPlaceBoundingBox:
                        {
                            m_Raycaster.ResetTransformation();
                            break;
                        }
                    }

                    StartCoroutine(AcknowledgeCancel());
                }
            }
        }

        private IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, false));
        }

        private IEnumerator ResetInstructionUI()
        {
            yield return new WaitForSeconds(0);
            m_CachedInstructionUI = null;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, InstructionUI.Init));
        }

        private void EnableSimulationViewInEditor(bool enable)
        {
#if UNITY_EDITOR
            SimulationSettings.instance.ShowSimulatedEnvironment = enable;
            SimulationSettings.instance.ShowSimulatedData = enable;
#endif
        }

        void OnARStateDataChanged(UIARStateData arStateData)
        {
            if (m_CachedInstructionUI != arStateData.instructionUI && m_CachedNavigationMode == NavigationMode.AR)
            {
                m_CachedInstructionUI = arStateData.instructionUI;
                switch (m_CachedInstructionUI)
                {
                    case InstructionUI.Init:
                    {
                        var m_actionMap = m_InputActionAsset.FindActionMap("AR", true);
                        m_actionMap.Enable();

                        // enable SimulationView
                        EnableSimulationViewInEditor(true);

                        // disable RootNode
                        UIStateManager.current.m_RootNode.SetActive(false);
                        UIStateManager.current.m_BoundingBoxRootNode.SetActive(false);
                        // Set AR Renderer
                        m_CameraData.SetRenderer((int)UniversalRenderer.ARRenderer);
                        m_MARSCamera.enabled = true;

                        // un-Pause MARSSession
                        UIStateManager.current.StartDetectingPlanes();
                        UIStateManager.current.StartDetectingPoints();
                        UIStateManager.current.ResumeSession();

                        // move next InstructionUI
                        StartCoroutine(MoveNext());
                        break;
                    }

                    case InstructionUI.CrossPlatformFindAPlane:
                    {
                        StartCoroutine(InstructionFindAPlaneCoroutine());
                        break;
                    }

                    case InstructionUI.AimToPlaceBoundingBox:
                    {
                        StartCoroutine(InstructionAimToPlaceBoundingBox());
                        break;
                    }
                    case InstructionUI.ConfirmPlacement:
                    {
                        StartCoroutine(InstructionConfirmPlacement());
                        break;
                    }
                    case InstructionUI.OnBoardingComplete:
                    {
                        StartCoroutine(InstructionOnBoardingComplete());
                        break;
                    }
                }
            }
        }

        IEnumerator MoveNext()
        {
            var next = UIStateManager.current.arStateData.instructionUI + 1;

            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, next));
        }

        IEnumerator InstructionAimToPlaceBoundingBox()
        {
            UIStateManager.current.m_PlacementRules.SetActive(true);

            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));

            m_InstructionUICanvas.enabled = false;
        }

        IEnumerator InstructionConfirmPlacement()
        {
            UIStateManager.current.m_PlacementRules.SetActive(false);

            m_Raycaster.SetObjectToPlace(UIStateManager.current.m_BoundingBoxRootNode.gameObject);
            m_Raycaster.PlaceObject();

            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=instructionConfirmPlacementText, level=StatusMessageLevel.Instruction }));
        }

        IEnumerator InstructionFindAPlaneCoroutine()
        {
            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, null));

            // default scale 1:100
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOneHundred));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar,
                ToolbarType.ARInstructionSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel,
                StatusMessageLevel.Instruction));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=instructionFindAPlaneText, level=StatusMessageLevel.Instruction }));

            m_Raycaster.Reset();
            m_InstructionUICanvas.enabled = true;
            m_Raycaster.SetObjectToPlace(UIStateManager.current.m_BoundingBoxRootNode.gameObject);
        }

        private void OnDisable()
        {
            StopAllCoroutines();

            if (m_MARSCamera != null)
                m_MARSCamera.enabled = false;

            DestroyImmediate(UIStateManager.current.m_PlacementRules);
            UIStateManager.current.m_PlacementRules = null;
            UIStateManager.current.ResetSession();
            UIStateManager.current.PauseSession();

            m_CachedNavigationMode = null;
            m_CachedInstructionUI = null;
            m_CachedOperationCancelled = null;

            // remove input system hooks
            m_InputActionAsset["Placement Rotate Action"].performed -= OnPlacementRotateAction;
            UIStateManager.stateChanged -= OnStateDataChanged;
            UIStateManager.arStateChanged -= OnARStateDataChanged;
        }

        IEnumerator InstructionOnBoardingComplete()
        {
            yield return new WaitForSeconds(0);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            m_Raycaster.SwapModel(UIStateManager.current.m_BoundingBoxRootNode, UIStateManager.current.m_RootNode);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));
        }
    }
}
