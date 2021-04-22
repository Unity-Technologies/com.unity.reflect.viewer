using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using Unity.MARS.Data;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Viewer;

namespace Unity.Reflect.Viewer.UI
{

    [CreateAssetMenu(fileName = "WallBasedARInstruction", menuName = "Reflect/WallBasedARInstruction", order = 52)]
    public class WallBasedARInstructionUI : ScriptableObject, IInstructionUI, IUIButtonValidator
    {
        enum WallBasedInstructionUI
        {
            Init = 0,
            AlignModelView,
            FindModelFloor,
            FindFirstWall,
            FindSecondWall,
            ConfirmAnchorPoint,
            FindTheFloor,
            FindFirstARWall,
            FindSecondARWall,
            ConfirmARAnchorPoint,
            // TODO: Bring step back once we have adjustments
            //ConfirmPlacement,
            OnBoardingComplete,
        };

        public ARMode arMode => ARMode.WallBased;

#pragma warning disable CS0649
        public ExposedReference<Raycaster> RaycasterRef;
        [SerializeField]
        public Material m_PlaneSelectionMaterial;
        [SerializeField]
        public Material m_FloorSelectionMaterial;
#pragma warning restore CS0649

        Raycaster m_Raycaster;
        ARModeUIController m_ARModeUIController;

        WallBasedInstructionUI m_WallBasedInstructionUI;

        const string m_InstructionAlignModelView = "1/9: Focus on the area with the corner that will be matched to the real world and press OK to get started";
        const string m_InstructionFindModelFloor = "2/9: Select the floor that will form the corner of the walls and press OK";
        const string m_InstructionFindFirstWall = "3/9: Select wall A that will form a corner and press OK";
        const string m_InstructionFindSecondWall = "4/9: Select wall B that will form a corner with wall A and press OK";
        const string m_InstructionConfirmAnchorPoint = "5/9: Corner point generated, press OK to confirm";
        const string m_InstructionFindTheFloor = "6/9: Pan your device aiming at the ground to find a horizontal surface and then press OK";
        const string m_InstructionFindFirstARWall = "7/9: Select the first plane which will match Wall A and press OK";
        const string m_InstructionFindSecondARWall = "8/9: Select the second plane which will match Wall B and press OK";
        const string m_InstructionConfirmARAnchorPoint = "9/9: Corner established, press OK to place and match the model";
        const string m_InstructionConfirmPlacementText = "Adjust your model as desired and press OK.";

        Dictionary<WallBasedInstructionUI, InstructionUIStep> m_States;

        SpatialOrientedPlaneSelector m_PlaneSelector;
        public InstructionUIStep CurrentInstructionStep => m_States[m_WallBasedInstructionUI];

        public void Initialize(ARModeUIController resolver)
        {
            m_ARModeUIController = resolver;
            m_Raycaster = RaycasterRef.Resolve(resolver);

            m_States = new Dictionary<WallBasedInstructionUI,InstructionUIStep>
            {
                { WallBasedInstructionUI.Init, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.Init, onNext = StartInstruction } },
                { WallBasedInstructionUI.AlignModelView, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.AlignModelView, onNext = AlignModelView, onBack = AlignModelViewBack } },
                { WallBasedInstructionUI.FindModelFloor, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.FindModelFloor, onNext = FindModelFloor, onBack = FindModelFloorBack } },
                { WallBasedInstructionUI.FindFirstWall, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.FindFirstWall, onNext = FindFirstWallNext, onBack = FindFirstWallBack,
                validations = new IPlacementValidation[] { new WallSizeValidation(1.0f) } } },
                { WallBasedInstructionUI.FindSecondWall, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.FindSecondWall, onNext = FindSecondWallNext, onBack = FindSecondWallBack,
                validations = new IPlacementValidation[] { new WallSizeValidation(1.0f), new ParallelWallValidation(0.3f) } } },
                { WallBasedInstructionUI.ConfirmAnchorPoint, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.ConfirmAnchorPoint, onNext = ConfirmAnchorPointNext, onBack = ConfirmAnchorPointBack } },
                { WallBasedInstructionUI.FindTheFloor, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.FindTheFloor, onNext = FindTheARFloorNext, onBack = FindTheARFloorBack } },
                { WallBasedInstructionUI.FindFirstARWall, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.FindFirstARWall, onNext = FindFirstARWallNext, onBack = FindFirstARWallBack } },
                { WallBasedInstructionUI.FindSecondARWall, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.FindSecondARWall, onNext = FindSecondARWallNext, onBack= FindSecondARWallBack } },
                { WallBasedInstructionUI.ConfirmARAnchorPoint, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.ConfirmARAnchorPoint, onNext = ConfirmARAnchorPointNext, onBack = ConfirmARAnchorPointBack } },
                // TODO: Bring step back once we have adjustments enabled
                //{ WallBasedInstructionUI.ConfirmPlacement, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.ConfirmPlacement, onNext = ConfirmPlacementNext, onBack = ConfirmPlacementBack } },
                { WallBasedInstructionUI.OnBoardingComplete, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext } },
            };
            m_PlaneSelector = new SpatialOrientedPlaneSelector();
        }
        public void Restart()
        {
            m_WallBasedInstructionUI = WallBasedInstructionUI.Init;
            m_ARModeUIController.StartCoroutine(ResetInstructionUI());
        }

        public void Cancel()
        {
            /* TODO: Bring cancel back once we have adjustments
            switch (m_WallBasedInstructionUI)
            {
                case WallBasedInstructionUI.ConfirmPlacement:
                {
                    m_Raycaster.ResetTransformation();
                    break;
                }
            }
            */
            m_ARModeUIController.StartCoroutine(AcknowledgeCancel());
        }

        private IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, false));
        }

        public void Next()
        {
            if(!CurrentInstructionStep.CheckValidations())
                return;

            var transition = m_States[++m_WallBasedInstructionUI].onNext;
            if (transition != null)
                transition();
        }

        public void Back()
        {
            var transition = m_States[--m_WallBasedInstructionUI].onBack;
            if (transition != null)
                transition();
        }

        void StartInstruction()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Init));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, this));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARModelAlignSidebar));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOne));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, MeasureToolStateData.defaultData));

            Next();
        }

        void AlignModelView()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            ToolState toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.OrbitTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            ARToolStateData arToolStateData = ARToolStateData.defaultData;
            arToolStateData.navigationEnabled = true;
            arToolStateData.okEnabled = true;
            arToolStateData.okButtonValidator = this;
            arToolStateData.scaleEnabled = false;
            arToolStateData.rotateEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, arToolStateData));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionAlignModelView, type = StatusMessageType.Instruction}));

            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(true);
            navigationState.showScaleReference = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
        }

        void AlignModelViewBack()
        {
            AlignModelView();
        }

        void FindModelFloor()
        {
            m_PlaneSelector.Orientation = MarsPlaneAlignment.HorizontalUp;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, new ObjectSelectionInfo()));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, m_PlaneSelector));
            ARToolStateData arToolState = ARToolStateData.defaultData;
            arToolState.navigationEnabled = false;
            arToolState.selectionEnabled = true;
            arToolState.previousStepEnabled = true;
            arToolState.okButtonValidator = this;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, arToolState));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SelectTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindModelFloor, type = StatusMessageType.Instruction}));
        }

        void FindModelFloorBack()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            DestroyImmediate(placementStateData.modelFloor);
            FindModelFloor();
        }

        void FindFirstWallNext()
        {
            // first record the previously selected plane as Mode floor
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            var info = UIStateManager.current.projectStateData.objectSelectionInfo;
            placementStateData.modelFloor = info.CurrentSelectedObject().ClonePlaneSurface("modelFloor", m_FloorSelectionMaterial);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            FindFirstWall();
        }
        void FindFirstWall()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, new ObjectSelectionInfo()));
            m_PlaneSelector.Orientation = MarsPlaneAlignment.Vertical;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetObjectPicker, m_PlaneSelector));
            ARToolStateData arToolState = ARToolStateData.defaultData;
            arToolState.navigationEnabled = false;
            arToolState.selectionEnabled = true;
            arToolState.previousStepEnabled = true;
            arToolState.wallIndicatorsEnabled = true;
            arToolState.okButtonValidator = this;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, arToolState));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SelectTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindFirstWall, type = StatusMessageType.Instruction}));
        }

        void FindFirstWallBack()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.previousStepEnabled = true;
            toolState.okButtonValidator = this;
            toolState.wallIndicatorsEnabled = true;
            toolState.anchorPointsEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            DestroyImmediate(placementStateData.firstSelectedPlane);
            FindFirstWall();
        }

        void FindSecondWallNext()
        {
            // first record the previously selected plane as Wall A
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            var info = UIStateManager.current.projectStateData.objectSelectionInfo;
            placementStateData.firstSelectedPlane = info.CurrentSelectedObject().ClonePlaneSurface("firstWall", m_PlaneSelectionMaterial);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            FindSecondWall();
        }

        void FindSecondWall()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, new ObjectSelectionInfo()));
            ARToolStateData arToolState = ARToolStateData.defaultData;
            arToolState.navigationEnabled = false;
            arToolState.selectionEnabled = true;
            arToolState.previousStepEnabled = true;
            arToolState.okButtonValidator = this;
            arToolState.wallIndicatorsEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, arToolState));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SelectTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindSecondWall, type = StatusMessageType.Instruction}));
        }

        void FindSecondWallBack()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            placementStateData.modelPlacementLocation = Vector3.zero;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.previousStepEnabled = true;
            toolState.okButtonValidator = this;
            toolState.wallIndicatorsEnabled = true;
            toolState.anchorPointsEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            DestroyImmediate(placementStateData.secondSelectedPlane);
            FindSecondWall();
        }

        void ConfirmAnchorPointNext()
        {
            // first record the previously selected plane as Wall B
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            var info = UIStateManager.current.projectStateData.objectSelectionInfo;
            placementStateData.secondSelectedPlane = info.CurrentSelectedObject().ClonePlaneSurface("secondWall", m_PlaneSelectionMaterial);
            // calculate anchor point position
            var floorContext = placementStateData.modelFloor.GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            var plane1Context = placementStateData.firstSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            var plane2Context = placementStateData.secondSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            PlanePlaneIntersection(out var unusedPoint, out var lineVec, plane1Context.SelectedPlane.normal, plane1Context.HitPoint,
                plane2Context.SelectedPlane.normal, plane2Context.HitPoint);
            // calculate intersect with bottom floor for now
            LinePlaneIntersection(out var anchorPoint, unusedPoint, lineVec, floorContext.SelectedPlane.normal, floorContext.HitPoint);
            placementStateData.modelPlacementLocation = anchorPoint;
            // calculate beam height
            placementStateData.beamHeight = Math.Max(placementStateData.firstSelectedPlane.GetComponent<Renderer>().bounds.size.y,
                placementStateData.secondSelectedPlane.GetComponent<Renderer>().bounds.size.y);
            // update placement
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            ConfirmAnchorPoint();
        }

        void ConfirmAnchorPoint()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SelectObjects, new ObjectSelectionInfo()));
            var arToolStateData = ARToolStateData.defaultData;
            arToolStateData.navigationEnabled = false;
            arToolStateData.previousStepEnabled = true;
            arToolStateData.okButtonValidator = this;
            arToolStateData.wallIndicatorsEnabled = true;
            arToolStateData.anchorPointsEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, arToolStateData));

            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.OrbitTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionConfirmAnchorPoint, type = StatusMessageType.Instruction}));
        }

        void ConfirmAnchorPointBack()
        {
            ARToolStateData arToolStateData = ARToolStateData.defaultData;
            arToolStateData.navigationEnabled = true;
            arToolStateData.previousStepEnabled = true;
            arToolStateData.okButtonValidator = this;
            arToolStateData.wallIndicatorsEnabled = true;
            arToolStateData.anchorPointsEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, arToolStateData));
            ToolState toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.OrbitTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));

            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = false;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.None));
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(true);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar,
                ToolbarType.ARModelAlignSidebar));

            ConfirmAnchorPoint();

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void FindTheARFloorNext()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = true;
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(false);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            FindTheARFloor();
        }

        void FindTheARFloor()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.FloorPlacementRule));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar,
                ToolbarType.ARInstructionSidebar));

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindTheFloor, type = StatusMessageType.Instruction}));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void FindTheARFloorBack()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            DestroyImmediate(placementStateData.arFloor);
            FindTheARFloor();
        }

        void FindFirstARWallNext()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            var meshObject = m_Raycaster.LastTarget.gameObject.CloneMeshObject("arFloor");
            placementStateData.arFloor = meshObject;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            FindFirstARWall();
        }

        void FindFirstARWall()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = true;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.WallPlacementRule));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindFirstARWall, type = StatusMessageType.Instruction}));
        }

        void FindFirstARWallBack()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            DestroyImmediate(placementStateData.firstARSelectedPlane);
            FindFirstARWall();
        }

        void FindSecondARWallNext()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            var meshObject = m_Raycaster.LastTarget.gameObject.CloneMeshObject("arFirstWall", m_PlaneSelectionMaterial);
            placementStateData.firstARSelectedPlane = meshObject;
            placementStateData.arPlacementAlignment = m_Raycaster.LastTarget.gameObject.transform.parent.forward;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            FindSecondARWall();
        }

        void FindSecondARWall()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.WallPlacementRule));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindSecondARWall, type = StatusMessageType.Instruction}));
        }

        void FindSecondARWallBack()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            DestroyImmediate(placementStateData.secondARSelectedPlane);
            FindSecondARWall();
        }

        void ConfirmARAnchorPointNext()
        {
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            var meshObject = m_Raycaster.LastTarget.gameObject.CloneMeshObject("arSecondWall", m_PlaneSelectionMaterial);
            placementStateData.secondARSelectedPlane = meshObject;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            ConfirmARAnchorPoint();
        }

        void ConfirmARAnchorPoint()
        {
            UIStateManager.current.m_PlacementRules.SetActive(false);
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            // calculate AR anchor point
            ARPlacementStateData placementStateData = UIStateManager.current.arStateData.placementStateData;
            PlanePlaneIntersection(out var unusedPoint, out var lineVec, placementStateData.firstARSelectedPlane.transform.up,
                placementStateData.firstARSelectedPlane.transform.position,
                placementStateData.secondARSelectedPlane.transform.up, placementStateData.secondARSelectedPlane.transform.position);
            // calculate intersect with bottom floor for now
            LinePlaneIntersection(out var anchorPoint, unusedPoint, lineVec, placementStateData.arFloor.transform.up, placementStateData.arFloor.transform.position);
            placementStateData.arPlacementLocation = anchorPoint;
            // calculate beam height
            placementStateData.beamHeight = Math.Max(placementStateData.firstARSelectedPlane.GetComponent<Renderer>().bounds.size.y,
                placementStateData.secondARSelectedPlane.GetComponent<Renderer>().bounds.size.y);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementState, placementStateData));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = true;
            toolState.arAnchorPointsEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionConfirmARAnchorPoint, type = StatusMessageType.Instruction}));
        }

        void ConfirmARAnchorPointBack()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));
            m_Raycaster.RestoreModel(UIStateManager.current.m_BoundingBoxRootNode, UIStateManager.current.m_RootNode);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));
            ConfirmARAnchorPoint();
        }

        void ConfirmPlacementNext()
        {
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = false;
            toolState.arAnchorPointsEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));
            ConfirmPlacement();
        }

        void ConfirmPlacement()
        {
            var modelPlaneContext = UIStateManager.current.arStateData.placementStateData.firstSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            var arPlaneNormal = UIStateManager.current.arStateData.placementStateData.firstARSelectedPlane.transform.up;

            m_Raycaster.AlignModelWithAnchor(UIStateManager.current.m_BoundingBoxRootNode, modelPlaneContext.SelectedPlane.normal,
                arPlaneNormal, UIStateManager.current.arStateData.placementStateData.modelPlacementLocation,
                UIStateManager.current.arStateData.placementStateData.arPlacementLocation);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionConfirmPlacementText, type = StatusMessageType.Instruction}));
        }

        void ConfirmPlacementBack()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
            ConfirmPlacement();
        }

        void OnBoardingCompleteNext()
        {
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.selectionEnabled = true;
            toolState.navigationEnabled = false;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = false;
            toolState.arAnchorPointsEnabled = false;
            toolState.measureToolEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            var modelPlaneContext = UIStateManager.current.arStateData.placementStateData.firstSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            m_Raycaster.AlignModelWithAnchor(UIStateManager.current.m_RootNode, modelPlaneContext.SelectedPlane.normal,
                UIStateManager.current.arStateData.placementStateData.arPlacementAlignment, UIStateManager.current.arStateData.placementStateData.modelPlacementLocation,
                UIStateManager.current.arStateData.placementStateData.arPlacementLocation);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));
            // only clear the messages if not debugging ARAxis
            if (!UIStateManager.current.debugStateData.debugOptionsData.ARAxisTrackingEnabled)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        IEnumerator ResetInstructionUI()
        {
            yield return new WaitForSeconds(0);
            m_WallBasedInstructionUI = WallBasedInstructionUI.Init;
            m_States[m_WallBasedInstructionUI].onNext();
        }

        public bool ButtonValidate()
        {
            switch (m_WallBasedInstructionUI)
            {
                case WallBasedInstructionUI.AlignModelView:
                {
                    return true;
                }

                case WallBasedInstructionUI.FindModelFloor:
                {
                    if (UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject() != null)
                    {
                        return true;
                    }

                    return false;
                }

                case WallBasedInstructionUI.FindFirstWall:
                {
                    if (UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject() != null)
                    {
                        return true;
                    }

                    return false;
                }

                case WallBasedInstructionUI.FindSecondWall:
                {
                    if (UIStateManager.current.projectStateData.objectSelectionInfo.CurrentSelectedObject() != null)
                    {
                        return true;
                    }

                    return false;
                }

                case WallBasedInstructionUI.ConfirmAnchorPoint:
                {
                    if (UIStateManager.current.arStateData.placementStateData.modelPlacementLocation != Vector3.zero)
                    {
                        return true;
                    }

                    return false;
                }

                case WallBasedInstructionUI.FindTheFloor:
                {
                    return m_Raycaster.ValidTarget;
                }

                case WallBasedInstructionUI.FindFirstARWall:
                {
                    return m_Raycaster.ValidTarget;
                }

                case WallBasedInstructionUI.FindSecondARWall:
                {
                    return m_Raycaster.ValidTarget;
                }

                default:
                {
                    return false;
                }
            }
        }

        // create a vector of direction "vector" with length "size"
        Vector3 SetVectorLength(Vector3 vector, float size)
        {
            // normalize the vector
            Vector3 vectorNormalized = Vector3.Normalize(vector);

            // scale the vector
            return vectorNormalized *= size;
        }

        // Find the line of intersection between two planes.	The planes are defined by a normal and a point on that plane.
	    // The outputs are a point on the line and a vector which indicates it's direction. If the planes are not parallel,
	    // the function outputs true, otherwise false.
        bool PlanePlaneIntersection(out Vector3 linePoint, out Vector3 lineVec, Vector3 plane1Normal, Vector3 plane1Position, Vector3 plane2Normal, Vector3 plane2Position)
        {
            linePoint = Vector3.zero;
		    lineVec = Vector3.zero;

		    // We can get the direction of the line of intersection of the two planes by calculating the
		    // cross product of the normals of the two planes. Note that this is just a direction and the line
		    // is not fixed in space yet. We need a point for that to go with the line vector.
		    lineVec = Vector3.Cross(plane1Normal, plane2Normal);

		    // Next is to calculate a point on the line to fix it's position in space. This is done by finding a vector from
		    // the plane2 location, moving parallel to it's plane, and intersecting plane1. To prevent rounding
		    // errors, this vector also has to be perpendicular to lineDirection. To get this vector, calculate
		    // the cross product of the normal of plane2 and the lineDirection.
		    var ldir = Vector3.Cross(plane2Normal, lineVec);

		    var denominator = Vector3.Dot(plane1Normal, ldir);

		    // Prevent divide by zero and rounding errors by requiring about 5 degrees angle between the planes.
		    if (Mathf.Abs(denominator) > 0.006f)
            {

			    var plane1ToPlane2 = plane1Position - plane2Position;
			    var t = Vector3.Dot(plane1Normal, plane1ToPlane2) / denominator;
			    linePoint = plane2Position + t * ldir;

			    return true;
		    }
            else
            {
                // output not valid
			    return false;
		    }
	    }

	    // Get the intersection between a line and a plane.
	    // If the line and plane are not parallel, the function outputs true, otherwise false.
	    bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint){

		    float length;
		    float dotNumerator;
		    float dotDenominator;
		    Vector3 vector;
		    intersection = Vector3.zero;

		    // calculate the distance between the linePoint and the line-plane intersection point
		    dotNumerator = Vector3.Dot((planePoint - linePoint), planeNormal);
		    dotDenominator = Vector3.Dot(lineVec, planeNormal);

		    // line and plane are not parallel
		    if (dotDenominator != 0.0f)
            {
			    length =  dotNumerator / dotDenominator;

			    //create a vector from the linePoint to the intersection point
			    vector = SetVectorLength(lineVec, length);

			    //get the coordinates of the line-plane intersection point
			    intersection = linePoint + vector;

			    return true;
		    }
            else
            {
                // output not valid
			    return false;
		    }
	    }
    }
}
