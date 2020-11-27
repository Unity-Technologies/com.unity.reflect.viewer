using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using UnityEngine;

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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, false));
        }

        public void Next()
        {
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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Init));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, this));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From( ActionTypes.EnablePlacement, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel,
                StatusMessageLevel.Instruction));

            m_Raycaster.Reset();
            m_Raycaster.SetObjectToPlace(UIStateManager.current.m_BoundingBoxRootNode.gameObject);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.FloorPlacementRule));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOne));
            Next();
        }

        void AlignModelViewNext()
        {
            AlignModelView();
        }

        void AlignModelViewBack()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));

            AlignModelView();
        }

        void AlignModelView()
        {
            m_Raycaster.Reset();
            UIStateManager.current.m_RootNode.transform.localPosition = Vector3.zero;
            UIStateManager.current.m_RootNode.transform.localRotation = Quaternion.identity;

            UIStateManager.current.m_PlacementRules.SetActive(false);
            m_Raycaster.ActiveScanning = false;
            m_Raycaster.DisableCursor();

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=m_InstructionAlignModelView, level=StatusMessageLevel.Instruction }));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARModelAlignSidebar));

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.navigationEnabled = true;
            toolState.okEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(true);
            navigationState.showScaleReference = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void FindTheFloorNext()
        {
            m_Raycaster.SetViewBaseARMode(Camera.main.transform);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, true));

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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=m_InstructionFindAPlaneText, level=StatusMessageLevel.Instruction }));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));

            UIStateManager.current.m_PlacementRules.SetActive(true);
            m_Raycaster.ActiveScanning = true;
            m_Raycaster.SetViewBasedPlaceMode(false);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = this;
            toolState.previousStepEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void ConfirmPlacementNext()
        {
            m_Raycaster.SetObjectToPlace(UIStateManager.current.m_BoundingBoxRootNode.gameObject);
            m_Raycaster.PlaceObject();

            ConfirmPlacement();
        }

        void ConfirmPlacementBack()
        {
            m_Raycaster.SwapModelToBox(UIStateManager.current.m_RootNode, UIStateManager.current.m_BoundingBoxRootNode);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.None));
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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=m_InstructionAimToPlaceText, level=StatusMessageLevel.Instruction }));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }


        void OnBoardingCompleteNext()
        {
            m_Raycaster.SetViewBasedPlaceMode(false);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.previousStepEnabled = true;
            toolState.selectionEnabled = true;
            toolState.scaleEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            m_Raycaster.SwapModel(UIStateManager.current.m_BoundingBoxRootNode, UIStateManager.current.m_RootNode);

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
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
