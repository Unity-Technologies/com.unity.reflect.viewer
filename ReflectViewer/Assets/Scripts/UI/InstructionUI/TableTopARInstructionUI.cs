using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;

namespace Unity.Reflect.Viewer.UI
{

    [CreateAssetMenu(fileName = "TableTopARInstruction", menuName = "Reflect/TableTopARInstruction", order = 51)]
    public class TableTopARInstructionUI : ScriptableObject, IInstructionUI, IUIButtonValidator
    {
        enum TableTopInstructionUI
        {
            Init = 0,
            FindTheFloor,
            ConfirmPlacement,
            OnBoardingComplete,
        };

        public ARMode arMode => ARMode.TableTop;

#pragma warning disable CS0649
        public ExposedReference<Canvas> InstructionUICanvasRef;
        public ExposedReference<Raycaster> RaycasterRef;
#pragma warning restore CS0649

        Raycaster m_Raycaster;
        ARModeUIController m_ARModeUIController;

        TableTopInstructionUI m_TableTopInstructionUI;

        const string m_InstructionFindAPlaneText = "Pan your device to find a horizontal surface and then tap or press OK to place the model";
        const string m_InstructionConfirmPlacementText = "Adjust your model as desired and press OK";

        Dictionary<TableTopInstructionUI, InstructionUIStep> m_States;
        public InstructionUIStep CurrentInstructionStep => m_States[m_TableTopInstructionUI];
        public void Initialize(ARModeUIController resolver)
        {
            m_ARModeUIController = resolver;
            m_Raycaster = RaycasterRef.Resolve(resolver);

            m_States = new Dictionary<TableTopInstructionUI, InstructionUIStep>
            {
                { TableTopInstructionUI.Init, new InstructionUIStep { stepIndex = (int) TableTopInstructionUI.Init, onNext = StartInstruction} },
                { TableTopInstructionUI.FindTheFloor, new InstructionUIStep { stepIndex = (int) TableTopInstructionUI.FindTheFloor, onNext = FindTheFloorNext, onBack = FindTheFloorBack} },
                { TableTopInstructionUI.ConfirmPlacement, new InstructionUIStep { stepIndex = (int) TableTopInstructionUI.ConfirmPlacement, onNext = ConfirmPlacementNext, onBack = ConfirmPlacementBack } },
                { TableTopInstructionUI.OnBoardingComplete, new InstructionUIStep { stepIndex = (int) TableTopInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext } },
            };
        }


        public void Restart()
        {
            m_TableTopInstructionUI = TableTopInstructionUI.Init;
            m_ARModeUIController.StartCoroutine(ResetInstructionUI());
        }

        public void Cancel()
        {
            switch (m_TableTopInstructionUI)
            {
                case TableTopInstructionUI.ConfirmPlacement:
                {
                    m_Raycaster.ResetTransformation();
                    break;
                }
            }
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

            var transition = m_States[++m_TableTopInstructionUI].onNext;
            if (transition != null)
                transition();
        }

        public void Back()
        {
            var transition = m_States[--m_TableTopInstructionUI].onBack;
            if (transition != null)
                transition();
        }

        IEnumerator ResetInstructionUI()
        {
            yield return new WaitForSeconds(0);
            m_TableTopInstructionUI = TableTopInstructionUI.Init;
            m_States[m_TableTopInstructionUI].onNext();
        }

        void StartInstruction()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Init));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, this));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, true));
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(false);
            navigationState.showScaleReference = true;

            m_Raycaster.Reset();
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.TableTopPlacementRule));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            // default scale 1:100
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOneHundred));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));


            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions, MeasureToolStateData.defaultData));

            Next();
        }

        void FindTheFloorNext()
        {
            FindTheFloor();
        }

        void FindTheFloorBack()
        {
            FindTheFloor();
        }

        void FindTheFloor()
        {
            UIStateManager.current.m_PlacementRules.SetActive(true);


            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() {text = m_InstructionFindAPlaneText, type = StatusMessageType.Instruction}));

            m_Raycaster.ActiveScanning = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = this;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

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

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() { text=m_InstructionConfirmPlacementText, type = StatusMessageType.Instruction}));

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = null;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.rotateEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void OnBoardingCompleteNext()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Completed));

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.rotateEnabled = false;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        public bool ButtonValidate()
        {
            switch (m_TableTopInstructionUI)
            {
                case TableTopInstructionUI.FindTheFloor:
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
