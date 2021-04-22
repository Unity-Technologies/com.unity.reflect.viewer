using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;

namespace Unity.Reflect.Viewer.UI
{

    [CreateAssetMenu(fileName = "ViewBasedARInstruction", menuName = "Reflect/ViewBasedARInstruction", order = 52)]
    public class ViewBasedARInstructionUI : ScriptableObject, IInstructionUI, IUIButtonValidator
    {
        enum ViewBasedInstructionUI
        {
            Init = 0,
            AlignModelView,
            FindTheFloor,
            ConfirmPlacement,
            OnBoardingComplete,
        };

        public ARMode arMode => ARMode.ViewBased;

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

        Dictionary<ViewBasedInstructionUI, InstructionUIStep> m_States;
        public InstructionUIStep CurrentInstructionStep => m_States[m_ViewBasedInstructionUI];

        public void Initialize(ARModeUIController resolver)
        {
            m_ARModeUIController = resolver;
            m_Raycaster = RaycasterRef.Resolve(resolver);

            m_States = new Dictionary<ViewBasedInstructionUI, InstructionUIStep>
            {
                { ViewBasedInstructionUI.Init, new InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.Init, onNext = StartInstruction} },
                { ViewBasedInstructionUI.AlignModelView, new InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.AlignModelView, onNext = AlignModelViewNext, onBack = AlignModelViewBack} },
                { ViewBasedInstructionUI.FindTheFloor, new InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.FindTheFloor, onNext = FindTheFloorNext, onBack = FindTheFloorBack} },
                { ViewBasedInstructionUI.ConfirmPlacement, new InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.ConfirmPlacement, onNext = ConfirmPlacementNext, onBack = ConfirmPlacementBack } },
                { ViewBasedInstructionUI.OnBoardingComplete, new InstructionUIStep { stepIndex = (int) ViewBasedInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext } },
            };
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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, false));
        }

        public void Next()
        {
            if(!CurrentInstructionStep.CheckValidations())
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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Init));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, this));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, true));

            m_Raycaster.Reset();

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOne));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, MeasureToolStateData.defaultData));

            Next();
        }

        void AlignModelViewNext()
        {
            AlignModelView();
        }

        void AlignModelViewBack()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));

            AlignModelView();
        }

        void AlignModelView()
        {
            m_Raycaster.Reset();

            if(UIStateManager.current.m_PlacementRules != null)
                UIStateManager.current.m_PlacementRules.SetActive(false);
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionAlignModelView, type = StatusMessageType.Instruction}));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARModelAlignSidebar));

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = true;
            toolState.okEnabled = true;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(true);
            navigationState.showScaleReference = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void FindTheFloorNext()
        {
            m_Raycaster.SetViewBaseARMode(Camera.main.transform);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.FloorPlacementRule));

            FindTheFloor();
        }


        void FindTheFloorBack()
        {
            FindTheFloor();
        }

        void FindTheFloor()
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(false);
            navigationState.showScaleReference = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionFindAPlaneText, type = StatusMessageType.Instruction}));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));

            UIStateManager.current.m_PlacementRules.SetActive(true);
            m_Raycaster.ActiveScanning = true;
            m_Raycaster.SetViewBasedPlaceMode(false);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void ConfirmPlacementNext()
        {
            m_Raycaster.PlaceObject();

            ConfirmPlacement();
        }

        void ConfirmPlacementBack()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
            ConfirmPlacement();
        }

        void ConfirmPlacement()
        {
            UIStateManager.current.m_PlacementRules.SetActive(false);

            m_Raycaster.ActiveScanning = false;
            m_Raycaster.SetViewBasedPlaceMode(true);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionAimToPlaceText, type = StatusMessageType.Instruction}));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }


        void OnBoardingCompleteNext()
        {
            m_Raycaster.SetViewBasedPlaceMode(false);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.previousStepEnabled = true;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Completed));

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
