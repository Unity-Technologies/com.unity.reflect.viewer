using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{

    [CreateAssetMenu(fileName = "ViewBasedARInstruction", menuName = "Reflect/ViewBasedARInstruction", order = 52)]
    public class ViewBasedARInstructionUI : ScriptableObject, IARInstructionUI, SetARToolStateAction.IUIButtonValidator
    {
        enum ViewBasedInstructionUI
        {
            Init = 0,
            AlignModelView,
            FindTheFloor,
            ConfirmPlacement,
            OnBoardingComplete,
        };

        public void Reset() { }

        public SetARModeAction.ARMode arMode => SetARModeAction.ARMode.ViewBased;

#pragma warning disable CS0649
        public ExposedReference<Canvas> InstructionUICanvasRef;
        public ExposedReference<Raycaster> RaycasterRef;
#pragma warning restore CS0649

        Raycaster m_Raycaster;
        ARModeUIController m_ARModeUIController;

        const string m_InstructionAlignModelView = "Position camera to the desired view that will be reflected in AR and press OK to confirm";
        const string m_InstructionFindAPlaneText = "Pan your device to find a horizontal surface to place your model, press OK to confirm your position";
        const string m_InstructionAimToPlaceText = "Aim at desired spot for your model and press OK to place the model";

        ViewBasedInstructionUI m_ViewBasedInstructionUI;

        Dictionary<ViewBasedInstructionUI, SetARInstructionUIAction.InstructionUIStep> m_States;
        IUISelector<GameObject> m_PlacementRuleGameObjectGetter;
        IUISelector<GameObject> m_FirstSelectedPlaneGetter;
        IUISelector<GameObject> m_SecondSelectedPlaneGetter;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoGetter;

        public SetARInstructionUIAction.InstructionUIStep CurrentInstructionStep => m_States[m_ViewBasedInstructionUI];

        public void Initialize( IARModeUIController resolver)
        {
            m_ARModeUIController = (ARModeUIController)resolver;
            m_Raycaster = RaycasterRef.Resolve(m_ARModeUIController);

            DisposeSelectors();
            m_FirstSelectedPlaneGetter = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.firstSelectedPlane));
            m_SecondSelectedPlaneGetter = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.secondSelectedPlane));
            m_PlacementRuleGameObjectGetter = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.placementRulesGameObject));
            m_ObjectSelectionInfoGetter = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo));

            m_States = new Dictionary<ViewBasedInstructionUI, SetARInstructionUIAction.InstructionUIStep>
            {
                { ViewBasedInstructionUI.Init, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.Init, onNext = StartInstruction} },
                { ViewBasedInstructionUI.AlignModelView, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.AlignModelView, onNext = AlignModelViewNext, onBack = AlignModelViewBack} },
                { ViewBasedInstructionUI.FindTheFloor, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.FindTheFloor, onNext = FindTheFloorNext, onBack = FindTheFloorBack} },
                { ViewBasedInstructionUI.ConfirmPlacement, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.ConfirmPlacement, onNext = ConfirmPlacementNext, onBack = ConfirmPlacementBack } },
                { ViewBasedInstructionUI.OnBoardingComplete, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext } },
            };
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
        }

        void DisposeSelectors()
        {
            m_FirstSelectedPlaneGetter?.Dispose();
            m_SecondSelectedPlaneGetter?.Dispose();
            m_PlacementRuleGameObjectGetter?.Dispose();
            m_ObjectSelectionInfoGetter?.Dispose();
        }

        public void Restart()
        {
            m_ViewBasedInstructionUI = ViewBasedInstructionUI.Init;
            m_ARModeUIController.StartCoroutine(ResetInstructionUI());
        }

        public void Cancel()
        {
            m_ARModeUIController.StartCoroutine(AcknowledgeCancel());
        }

        IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0);
            Dispatcher.Dispatch(CancelAction.From(false));
        }

        public void Next()
        {
            if (!CurrentInstructionStep.CheckValidations(m_FirstSelectedPlaneGetter.GetValue(), m_SecondSelectedPlaneGetter.GetValue(), ((ObjectSelectionInfo)m_ObjectSelectionInfoGetter.GetValue()).CurrentSelectedObject()))
                return;

            var transition = m_States[++m_ViewBasedInstructionUI].onNext;
            if (transition != null)
                transition();
        }

        public void Back()
        {
            var transition = m_States[--m_ViewBasedInstructionUI].onBack;
            if (transition != null)
                transition();
        }

        void StartInstruction()
        {
            Dispatcher.Dispatch(ShowModelAction.From(true));
            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Init));
            Dispatcher.Dispatch(SetARInstructionUIAction.From(new { currentARInstructionUI = this }));
            Dispatcher.Dispatch(SetAREnabledAction.From(false));
            Dispatcher.Dispatch(SetInstructionMode.From(true));
            m_Raycaster.Reset();
            Dispatcher.Dispatch(SetModelScaleAction.From(SetModelScaleAction.ArchitectureScale.OneToOne));
            Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));


            Next();
        }

        void AlignModelViewNext()
        {
            AlignModelView();
        }

        void AlignModelViewBack()
        {
            Dispatcher.Dispatch(SetAREnabledAction.From(false));

            AlignModelView();
        }

        void AlignModelView()
        {
            m_Raycaster.Reset();

            if(m_PlacementRuleGameObjectGetter.GetValue() != null)
                m_PlacementRuleGameObjectGetter.GetValue().SetActive(false);
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text=m_InstructionAlignModelView, type = StatusMessageType.Instruction}));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARModelAlignSidebar));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = false;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = true;
            toolState.cancelEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));

            toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.navigationEnabled = true;
            toolState.okEnabled = true;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(EnableAllNavigationAction.From(true));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(false));
            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
        }

        void FindTheFloorNext()
        {
            m_Raycaster.SetViewBaseARMode(Camera.main.transform);
            Dispatcher.Dispatch(SetAREnabledAction.From(true));
            m_ARModeUIController.ActivePlacementRules(SetModelFloorAction.PlacementRule.FloorPlacementRule);
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.FloorPlacementRule));

            FindTheFloor();
        }

        void FindTheFloorBack()
        {
            FindTheFloor();
        }

        void FindTheFloor()
        {
            Dispatcher.Dispatch(EnableAllNavigationAction.From(false));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(true));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text=m_InstructionFindAPlaneText, type = StatusMessageType.Instruction}));
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(false));
            m_PlacementRuleGameObjectGetter.GetValue().SetActive(true);
            m_Raycaster.ActiveScanning = true;
            m_Raycaster.SetViewBasedPlaceMode(false);
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = false;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
        }

        void ConfirmPlacementNext()
        {
            m_Raycaster.PlaceObject();

            ConfirmPlacement();
        }

        void ConfirmPlacementBack()
        {
            Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.None));
            ConfirmPlacement();
        }

        void ConfirmPlacement()
        {
            m_PlacementRuleGameObjectGetter.GetValue().SetActive(false);

            m_Raycaster.ActiveScanning = false;
            m_Raycaster.SetViewBasedPlaceMode(true);

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From( true));
            Dispatcher.Dispatch(ShowModelAction.From(false));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text=m_InstructionAimToPlaceText, type = StatusMessageType.Instruction}));
            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
        }


        void OnBoardingCompleteNext()
        {
            m_Raycaster.SetViewBasedPlaceMode(false);

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.previousStepEnabled = true;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetInstructionMode.From(false));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Dispatcher.Dispatch(ShowModelAction.From( true));
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(false));
            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Completed));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARSidebar));
            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(true));
        }

        IEnumerator ResetInstructionUI()
        {
            yield return new WaitForSeconds(0);
            m_ViewBasedInstructionUI = ViewBasedInstructionUI.Init;
            m_States[m_ViewBasedInstructionUI].onNext();
        }

        public bool ButtonValidate()
        {
            switch (m_ViewBasedInstructionUI)
            {
                case ViewBasedInstructionUI.FindTheFloor:
                {
                    return m_Raycaster.ValidTarget;
                }
                default:
                {
                    return false;
                }
            }
        }
    }
}
