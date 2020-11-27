using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using UnityEngine;

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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.Cancel, false));
        }

        public void Next()
        {
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
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUIState, InstructionUIState.Init));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetInstructionUI, this));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, true));
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.EnableAllNavigation(false);
            navigationState.showScaleReference = true;

            m_Raycaster.Reset();
            m_Raycaster.SetObjectToPlace(UIStateManager.current.m_BoundingBoxRootNode.gameObject);
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetPlacementRules, PlacementRule.TableTopPlacementRule));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            // default scale 1:100
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetModelScale, ArchitectureScale.OneToOneHundred));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel,
                StatusMessageLevel.Instruction));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));


            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
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

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From( ActionTypes.EnablePlacement, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=m_InstructionFindAPlaneText, level=StatusMessageLevel.Instruction }));

            m_Raycaster.ActiveScanning = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, false));
            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = this;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

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

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowBoundingBoxModel, true));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, false));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From( ActionTypes.EnablePlacement, true));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARInstructionSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithLevel,
                new StatusMessageData() { text=m_InstructionConfirmPlacementText, level=StatusMessageLevel.Instruction }));

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okButtonValidator = null;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = false;
            settingsToolState.sceneOptionEnabled = false;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        void OnBoardingCompleteNext()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));

            m_Raycaster.SwapModel(UIStateManager.current.m_BoundingBoxRootNode, UIStateManager.current.m_RootNode);

            ARToolStateData toolState = ARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.selectionEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetARToolState, toolState));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
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
