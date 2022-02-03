using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.MARS.Data;
using Unity.Reflect.Viewer.Core.Actions;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [CreateAssetMenu(fileName = "WallBasedARInstruction", menuName = "Reflect/WallBasedARInstruction", order = 52)]
    public class WallBasedARInstructionUI : ScriptableObject, IARInstructionUI, SetARToolStateAction.IUIButtonValidator
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

        public SetARModeAction.ARMode arMode => SetARModeAction.ARMode.WallBased;

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

        Dictionary<WallBasedInstructionUI, SetARInstructionUIAction.InstructionUIStep> m_States;

        SpatialOrientedPlaneSelector m_PlaneSelector;
        IUISelector<GameObject> m_FirstSelectedPlaneSelector;
        IUISelector<GameObject> m_SecondSelectedPlaneSelector;
        IUISelector<Transform> m_RootSelector;
        IUISelector<GameObject> m_PlacementRuleGameObjectSelector;
        IUISelector<bool> m_ARAxisTrackingSelector;
        IUISelector<Transform> m_BoundinBoxRootSelector;
        IUISelector<GameObject> m_ModelFloorSelector;
        IUISelector<GameObject> m_ARFloorSelector;
        IUISelector<GameObject> m_FirstARSelectedPlaneSelector;
        IUISelector<GameObject> m_SecondARSelectedPlaneSelector;
        IUISelector<Vector3> m_ARPlacementLocationSelector;
        IUISelector<Vector3> m_ModelPlacementLocationSelector;
        IUISelector<Vector3> m_ARPlacementAlignmentSelector;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoSelector;

        public SetARInstructionUIAction.InstructionUIStep CurrentInstructionStep => m_States[m_WallBasedInstructionUI];

        public void Initialize(IARModeUIController resolver)
        {
            DisposeResources();
            m_ARModeUIController = (ARModeUIController)resolver;
            m_Raycaster = RaycasterRef.Resolve(m_ARModeUIController);
            m_FirstSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.firstSelectedPlane));
            m_SecondSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.secondSelectedPlane));
            m_BoundinBoxRootSelector = UISelectorFactory.createSelector<Transform>(ARPlacementContext.current, nameof(IARPlacementDataProvider.boundingBoxRootNode));
            m_ModelFloorSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.modelFloor));
            m_ARFloorSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.arFloor));
            m_FirstARSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.firstARSelectedPlane));
            m_SecondARSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.secondARSelectedPlane));
            m_ModelPlacementLocationSelector = UISelectorFactory.createSelector<Vector3>(ARPlacementContext.current, nameof(IARPlacementDataProvider.modelPlacementLocation));
            m_ARPlacementLocationSelector = UISelectorFactory.createSelector<Vector3>(ARPlacementContext.current, nameof(IARPlacementDataProvider.arPlacementLocation));
            m_ARPlacementAlignmentSelector = UISelectorFactory.createSelector<Vector3>(ARPlacementContext.current, nameof(IARPlacementDataProvider.arPlacementAlignment));
            m_RootSelector = UISelectorFactory.createSelector<Transform>(PipelineContext.current, nameof(IPipelineDataProvider.rootNode));
            m_PlacementRuleGameObjectSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current,nameof(IARPlacementDataProvider.placementRulesGameObject));
            m_ObjectSelectionInfoSelector = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo));

            m_States = new Dictionary<WallBasedInstructionUI, SetARInstructionUIAction.InstructionUIStep>
            {
                { WallBasedInstructionUI.Init, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.Init, onNext = StartInstruction } },
                { WallBasedInstructionUI.AlignModelView, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.AlignModelView, onNext = AlignModelView, onBack = AlignModelViewBack } },
                { WallBasedInstructionUI.FindModelFloor, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.FindModelFloor, onNext = FindModelFloor, onBack = FindModelFloorBack } },
                {
                    WallBasedInstructionUI.FindFirstWall, new SetARInstructionUIAction.InstructionUIStep
                    {
                        stepIndex = (int)WallBasedInstructionUI.FindFirstWall, onNext = FindFirstWallNext, onBack = FindFirstWallBack,
                        validations = new IPlacementValidation[] { new WallSizeValidation(1.0f) }
                    }
                },
                {
                    WallBasedInstructionUI.FindSecondWall, new SetARInstructionUIAction.InstructionUIStep
                    {
                        stepIndex = (int)WallBasedInstructionUI.FindSecondWall, onNext = FindSecondWallNext, onBack = FindSecondWallBack,
                        validations = new IPlacementValidation[] { new WallSizeValidation(1.0f), new ParallelWallValidation(0.3f) }
                    }
                },
                { WallBasedInstructionUI.ConfirmAnchorPoint, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.ConfirmAnchorPoint, onNext = ConfirmAnchorPointNext, onBack = ConfirmAnchorPointBack } },
                { WallBasedInstructionUI.FindTheFloor, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.FindTheFloor, onNext = FindTheARFloorNext, onBack = FindTheARFloorBack } },
                { WallBasedInstructionUI.FindFirstARWall, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.FindFirstARWall, onNext = FindFirstARWallNext, onBack = FindFirstARWallBack } },
                { WallBasedInstructionUI.FindSecondARWall, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.FindSecondARWall, onNext = FindSecondARWallNext, onBack = FindSecondARWallBack } },
                { WallBasedInstructionUI.ConfirmARAnchorPoint, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.ConfirmARAnchorPoint, onNext = ConfirmARAnchorPointNext, onBack = ConfirmARAnchorPointBack } },

                // TODO: Bring step back once we have adjustments enabled
                //{ WallBasedInstructionUI.ConfirmPlacement, new InstructionUIStep{ stepIndex = (int)WallBasedInstructionUI.ConfirmPlacement, onNext = ConfirmPlacementNext, onBack = ConfirmPlacementBack } },
                { WallBasedInstructionUI.OnBoardingComplete, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WallBasedInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext } },
            };
            m_PlaneSelector = new SpatialOrientedPlaneSelector();
            m_ARAxisTrackingSelector = UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.ARAxisTrackingEnabled));
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
        }

        void DisposeResources()
        {
            m_FirstSelectedPlaneSelector?.Dispose();
            m_SecondSelectedPlaneSelector?.Dispose();
            m_BoundinBoxRootSelector?.Dispose();
            m_ModelFloorSelector?.Dispose();
            m_ARFloorSelector?.Dispose();
            m_FirstARSelectedPlaneSelector?.Dispose();
            m_SecondARSelectedPlaneSelector?.Dispose();
            m_ModelPlacementLocationSelector?.Dispose();
            m_ARPlacementLocationSelector?.Dispose();
            m_ARPlacementAlignmentSelector?.Dispose();
            m_RootSelector?.Dispose();
            m_PlacementRuleGameObjectSelector?.Dispose();
            m_ObjectSelectionInfoSelector?.Dispose();
            m_ARAxisTrackingSelector?.Dispose();
            m_PlaneSelector?.Dispose();
        }

        public void Reset()
        {
            m_Raycaster.RestoreModel(m_BoundinBoxRootSelector.GetValue(), m_RootSelector.GetValue());
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

        IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0);
            Dispatcher.Dispatch(CancelAction.From(false));
        }

        public void Next()
        {
            if (!CurrentInstructionStep.CheckValidations(m_FirstSelectedPlaneSelector.GetValue(), m_SecondSelectedPlaneSelector.GetValue(), ((ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue()).CurrentSelectedObject()))
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
            Dispatcher.Dispatch(ShowModelAction.From(true));
            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Init));
            Dispatcher.Dispatch(SetARInstructionUIAction.From(new { currentARInstructionUI = this }));

            Dispatcher.Dispatch(SetAREnabledAction.From(false));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARModelAlignSidebar));
            Dispatcher.Dispatch(SetInstructionMode.From(true));
            Dispatcher.Dispatch(SetModelScaleAction.From(SetModelScaleAction.ArchitectureScale.OneToOne));

            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
            Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = true;
            toolState.scaleEnabled = true;
            toolState.cancelEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Next();
        }

        void AlignModelView()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.OrbitTool));
            var arToolStateData = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolStateData.navigationEnabled = true;
            arToolStateData.okEnabled = true;
            arToolStateData.okButtonValidator = this;
            arToolStateData.scaleEnabled = false;
            arToolStateData.rotateEnabled = false;
            arToolStateData.selectionEnabled = false;
            arToolStateData.measureToolEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolStateData));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionAlignModelView, type = StatusMessageType.Instruction }));

            Dispatcher.Dispatch(EnableAllNavigationAction.From(true));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(false));
        }

        void AlignModelViewBack()
        {
            AlignModelView();
        }

        void FindModelFloor()
        {
            m_PlaneSelector.Orientation = MarsPlaneAlignment.HorizontalUp;
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(m_PlaneSelector));
            var arToolState = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolState.navigationEnabled = false;
            arToolState.selectionEnabled = true;
            arToolState.previousStepEnabled = true;
            arToolState.okButtonValidator = this;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolState));
            Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.SelectTool));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionFindModelFloor, type = StatusMessageType.Instruction }));
        }

        void FindModelFloorBack()
        {
            DestroyImmediate(m_ModelFloorSelector.GetValue());
            FindModelFloor();
        }

        void FindFirstWallNext()
        {
            // first record the previously selected plane as Mode floor
            var info = (ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue();
            Dispatcher.Dispatch(SetModelFloorAction.From(info.CurrentSelectedObject().ClonePlaneSurface("modelFloor", m_FloorSelectionMaterial)));
            FindFirstWall();
        }

        void FindFirstWall()
        {
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
            m_PlaneSelector.Orientation = MarsPlaneAlignment.Vertical;
            Dispatcher.Dispatch(SetSpatialSelectorAction.From(m_PlaneSelector));
            var arToolState = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolState.navigationEnabled = false;
            arToolState.selectionEnabled = true;
            arToolState.previousStepEnabled = true;
            arToolState.wallIndicatorsEnabled = true;
            arToolState.okButtonValidator = this;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolState));
            Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.SelectTool));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionFindFirstWall, type = StatusMessageType.Instruction }));
        }

        void FindFirstWallBack()
        {
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.previousStepEnabled = true;
            toolState.okButtonValidator = this;
            toolState.wallIndicatorsEnabled = true;
            toolState.anchorPointsEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            DestroyImmediate(m_FirstSelectedPlaneSelector.GetValue());
            FindFirstWall();
        }

        void FindSecondWallNext()
        {
            // first record the previously selected plane as Wall A
            var info = (ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue();
            Dispatcher.Dispatch(SetFirstPlaneAction.From(info.CurrentSelectedObject().ClonePlaneSurface("firstWall", m_PlaneSelectionMaterial)));
            FindSecondWall();
        }

        void FindSecondWall()
        {
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
            var arToolState = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolState.navigationEnabled = false;
            arToolState.selectionEnabled = true;
            arToolState.previousStepEnabled = true;
            arToolState.okButtonValidator = this;
            arToolState.wallIndicatorsEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolState));
            Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.SelectTool));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionFindSecondWall, type = StatusMessageType.Instruction }));
        }

        void FindSecondWallBack()
        {
            Dispatcher.Dispatch(SetModelPlacementLocationAction.From(Vector3.zero));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.previousStepEnabled = true;
            toolState.okButtonValidator = this;
            toolState.wallIndicatorsEnabled = true;
            toolState.anchorPointsEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            DestroyImmediate(m_SecondSelectedPlaneSelector.GetValue());
            FindSecondWall();
        }

        void ConfirmAnchorPointNext()
        {
            SetAnchorPointAction.SetAnchorPointData data;

            // first record the previously selected plane as Wall B
            var info = (ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue();
            data.secondSelectedPlane = info.CurrentSelectedObject().ClonePlaneSurface("secondWall", m_PlaneSelectionMaterial);

            // calculate anchor point position
            var floorContext = m_ModelFloorSelector.GetValue().GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            var plane1Context = m_FirstSelectedPlaneSelector.GetValue().GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            var plane2Context = data.secondSelectedPlane.GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            PlanePlaneIntersection(out var unusedPoint, out var lineVec, plane1Context.SelectedPlane.normal, plane1Context.HitPoint,
                plane2Context.SelectedPlane.normal, plane2Context.HitPoint);

            // calculate intersect with bottom floor for now
            LinePlaneIntersection(out var anchorPoint, unusedPoint, lineVec, floorContext.SelectedPlane.normal, floorContext.HitPoint);
            data.modelPlacementLocation = anchorPoint;

            // calculate beam height
            data.beamHeight = Math.Max(m_FirstSelectedPlaneSelector.GetValue().GetComponent<Renderer>().bounds.size.y, data.secondSelectedPlane.GetComponent<Renderer>().bounds.size.y);

            // update placement
            Dispatcher.Dispatch(SetAnchorPointAction.From(data));
            ConfirmAnchorPoint();
        }

        void ConfirmAnchorPoint()
        {
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
            var arToolStateData = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolStateData.navigationEnabled = false;
            arToolStateData.previousStepEnabled = true;
            arToolStateData.okButtonValidator = this;
            arToolStateData.wallIndicatorsEnabled = true;
            arToolStateData.anchorPointsEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolStateData));
            Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.None));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionConfirmAnchorPoint, type = StatusMessageType.Instruction }));
        }

        void ConfirmAnchorPointBack()
        {
            var arToolStateData = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolStateData.navigationEnabled = true;
            arToolStateData.previousStepEnabled = true;
            arToolStateData.okButtonValidator = this;
            arToolStateData.wallIndicatorsEnabled = true;
            arToolStateData.anchorPointsEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolStateData));
            Dispatcher.Dispatch(SetActiveToolAction.From(SetActiveToolAction.ToolType.None));

            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = false;

            Dispatcher.Dispatch(SetAREnabledAction.From(false));
            m_ARModeUIController.ActivePlacementRules(SetModelFloorAction.PlacementRule.None);
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.None));
            Dispatcher.Dispatch(EnableAllNavigationAction.From(true));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARModelAlignSidebar));

            ConfirmAnchorPoint();

            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
        }

        void FindTheARFloorNext()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = true;
            Dispatcher.Dispatch(EnableAllNavigationAction.From(false));
            Dispatcher.Dispatch(SetAREnabledAction.From(true));
            Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            FindTheARFloor();
        }

        void FindTheARFloor()
        {
            m_ARModeUIController.ActivePlacementRules(SetModelFloorAction.PlacementRule.FloorPlacementRule);
            Dispatcher.Dispatch(ShowModelAction.From(false));
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.FloorPlacementRule));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = false;
            toolState.navigationEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionFindTheFloor, type = StatusMessageType.Instruction }));

            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
        }

        void FindTheARFloorBack()
        {
            DestroyImmediate(m_ARFloorSelector.GetValue());
            FindTheARFloor();
        }

        void FindFirstARWallNext()
        {
            var meshObject = m_Raycaster.LastTarget.gameObject.CloneMeshObject("arFloor");
            Dispatcher.Dispatch(SetARFloorAction.From(meshObject));
            FindFirstARWall();
        }

        void FindFirstARWall()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = true;

            m_ARModeUIController.ActivePlacementRules(SetModelFloorAction.PlacementRule.WallPlacementRule);
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.WallPlacementRule));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionFindFirstARWall, type = StatusMessageType.Instruction }));
        }

        void FindFirstARWallBack()
        {
            DestroyImmediate(m_FirstARSelectedPlaneSelector.GetValue());
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.arWallIndicatorsEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            FindFirstARWall();
        }

        void FindSecondARWallNext()
        {
            var meshObject = m_Raycaster.LastTarget.gameObject.CloneMeshObject("arFirstWall", m_PlaneSelectionMaterial);
            SetARFirstWallAlignmentAction.SetARFirstWallData data;
            data.firstARSelectedPlane = meshObject;
            data.arPlacementAlignment = m_Raycaster.LastTarget.gameObject.transform.parent.forward;
            Dispatcher.Dispatch(SetARFirstWallAlignmentAction.From(data));
            FindSecondARWall();
        }

        void FindSecondARWall()
        {
            m_Raycaster.Reset();
            m_Raycaster.ActiveScanning = true;
            m_ARModeUIController.ActivePlacementRules(SetModelFloorAction.PlacementRule.WallPlacementRule);
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.WallPlacementRule));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionFindSecondARWall, type = StatusMessageType.Instruction }));
        }

        void FindSecondARWallBack()
        {
            DestroyImmediate(m_SecondARSelectedPlaneSelector.GetValue());
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.arWallIndicatorsEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            FindSecondARWall();
        }

        void ConfirmARAnchorPointNext()
        {
            var meshObject = m_Raycaster.LastTarget.gameObject.CloneMeshObject("arSecondWall", m_PlaneSelectionMaterial);
            Dispatcher.Dispatch(SetARSecondPlaneAction.From(meshObject));
            ConfirmARAnchorPoint();
        }

        void ConfirmARAnchorPoint()
        {
            m_PlacementRuleGameObjectSelector.GetValue().SetActive(false);
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            var firstARSelectedPlane = m_FirstARSelectedPlaneSelector.GetValue();
            var secondARSelectedPlane = m_SecondARSelectedPlaneSelector.GetValue();
            var ARFloor = m_ARFloorSelector.GetValue();

            // calculate AR anchor point
            SetARAnchorPointAction.SetARAnchorPointData data;
            PlanePlaneIntersection(out var unusedPoint, out var lineVec, firstARSelectedPlane.transform.up, firstARSelectedPlane.transform.position,
                secondARSelectedPlane.transform.up, secondARSelectedPlane.transform.position);

            // calculate intersect with bottom floor for now
            LinePlaneIntersection(out data.arPlacementLocation, unusedPoint, lineVec, ARFloor.transform.up, ARFloor.transform.position);

            // calculate beam height
            data.beamHeight = Math.Max(firstARSelectedPlane.GetComponent<Renderer>().bounds.size.y, secondARSelectedPlane.GetComponent<Renderer>().bounds.size.y);
            Dispatcher.Dispatch(SetARAnchorPointAction.From(data));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = true;
            toolState.arAnchorPointsEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionConfirmARAnchorPoint, type = StatusMessageType.Instruction }));
        }

        void ConfirmARAnchorPointBack()
        {
            Dispatcher.Dispatch(SetInstructionMode.From(true));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));
            m_Raycaster.RestoreModel(m_BoundinBoxRootSelector.GetValue(), m_RootSelector.GetValue());
            Dispatcher.Dispatch(ShowModelAction.From(false));
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(false));
            ConfirmARAnchorPoint();
        }

        void ConfirmPlacementNext()
        {
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = false;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = false;
            toolState.arAnchorPointsEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            ConfirmPlacement();
        }

        void ConfirmPlacement()
        {
            var modelPlaneContext = m_FirstSelectedPlaneSelector.GetValue().GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            var arPlaneNormal = m_FirstARSelectedPlaneSelector.GetValue().transform.up;

            m_Raycaster.AlignModelWithAnchor(m_BoundinBoxRootSelector.GetValue(), modelPlaneContext.SelectedPlane.normal,
                arPlaneNormal, m_ModelPlacementLocationSelector.GetValue(),
                m_ARPlacementLocationSelector.GetValue());

            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(true));

            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = m_InstructionConfirmPlacementText, type = StatusMessageType.Instruction }));
        }

        void ConfirmPlacementBack()
        {
            Dispatcher.Dispatch(SetInstructionMode.From(true));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));

            Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
            ConfirmPlacement();
        }

        void OnBoardingCompleteNext()
        {
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.selectionEnabled = true;
            toolState.navigationEnabled = false;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.arWallIndicatorsEnabled = false;
            toolState.arAnchorPointsEnabled = false;
            toolState.measureToolEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));

            var modelPlaneContext = m_FirstSelectedPlaneSelector.GetValue().GetComponent<PlaneSelectionContext>().SelectionContextList[0];
            m_Raycaster.AlignModelWithAnchor(m_RootSelector.GetValue(), modelPlaneContext.SelectedPlane.normal,
                m_ARPlacementAlignmentSelector.GetValue(), m_ModelPlacementLocationSelector.GetValue(),
                m_ARPlacementLocationSelector.GetValue());

            Dispatcher.Dispatch(ShowModelAction.From(true));

            // only clear the messages if not debugging ARAxis
            if (!m_ARAxisTrackingSelector.GetValue())
            {
                Dispatcher.Dispatch(SetInstructionMode.From(false));
                Dispatcher.Dispatch(ClearStatusAction.From(true));
                Dispatcher.Dispatch(ClearStatusAction.From(false));
            }

            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARSidebar));

            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(true));
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
                case WallBasedInstructionUI.FindFirstWall:
                case WallBasedInstructionUI.FindSecondWall:
                {
                    if (((ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue()).CurrentSelectedObject() != null)
                    {
                        return true;
                    }

                    return false;
                }
                case WallBasedInstructionUI.ConfirmAnchorPoint:
                {
                    if (m_ModelPlacementLocationSelector.GetValue() != Vector3.zero)
                    {
                        return true;
                    }

                    return false;
                }
                case WallBasedInstructionUI.FindTheFloor:
                case WallBasedInstructionUI.FindFirstARWall:
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
        bool LinePlaneIntersection(out Vector3 intersection, Vector3 linePoint, Vector3 lineVec, Vector3 planeNormal, Vector3 planePoint)
        {
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
                length = dotNumerator / dotDenominator;

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
