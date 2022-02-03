using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [CreateAssetMenu(fileName = "TableTopARInstruction", menuName = "Reflect/TableTopARInstruction", order = 51)]
    public class TableTopARInstructionUI : ScriptableObject, IARInstructionUI, SetARToolStateAction.IUIButtonValidator
    {
        enum TableTopInstructionUI
        {
            Init = 0,
            FindTheFloor,
            ConfirmPlacement,
            OnBoardingComplete,
        };

        public void Reset()
        {
        }

        public SetARModeAction.ARMode arMode => SetARModeAction.ARMode.TableTop;

#pragma warning disable CS0649
        public ExposedReference<Canvas> InstructionUICanvasRef;
        public ExposedReference<Raycaster> RaycasterRef;
#pragma warning restore CS0649

        Raycaster m_Raycaster;
        ARModeUIController m_ARModeUIController;

        TableTopInstructionUI m_TableTopInstructionUI;

        const string m_InstructionFindAPlaneText =
            "Pan your device to find a horizontal surface and then tap or press OK to place the model";

        const string m_InstructionConfirmPlacementText = "Adjust your model as desired and press OK";

        Dictionary<TableTopInstructionUI, SetARInstructionUIAction.InstructionUIStep> m_States;
        IUISelector<GameObject> m_PlacementRuleGameObjectSelector;
        IUISelector<GameObject> m_FirstSelectedPlaneSelector;
        IUISelector<GameObject> m_SecondSelectedPlaneSelector;
        IUISelector<SelectObjectAction.IObjectSelectionInfo> m_ObjectSelectionInfoSelector;

        public SetARInstructionUIAction.InstructionUIStep CurrentInstructionStep => m_States[m_TableTopInstructionUI];

        public void Initialize(IARModeUIController resolver)
        {
            m_ARModeUIController = (ARModeUIController)resolver;
            m_Raycaster = RaycasterRef.Resolve(m_ARModeUIController);
            DisposeSelectors();
            m_PlacementRuleGameObjectSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.placementRulesGameObject));
            m_FirstSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.firstSelectedPlane));
            m_SecondSelectedPlaneSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, nameof(IARPlacementDataProvider.secondSelectedPlane));
            m_ObjectSelectionInfoSelector = UISelectorFactory.createSelector<SelectObjectAction.IObjectSelectionInfo>(ProjectContext.current, nameof(IObjectSelectorDataProvider.objectSelectionInfo));

            m_States = new Dictionary<TableTopInstructionUI, SetARInstructionUIAction.InstructionUIStep>
            {
                {
                    TableTopInstructionUI.Init,
                    new SetARInstructionUIAction.InstructionUIStep {stepIndex = (int) TableTopInstructionUI.Init, onNext = StartInstruction}
                },
                {
                    TableTopInstructionUI.FindTheFloor,
                    new SetARInstructionUIAction.InstructionUIStep
                    {
                        stepIndex = (int) TableTopInstructionUI.FindTheFloor, onNext = FindTheFloorNext,
                        onBack = FindTheFloorBack
                    }
                },
                {
                    TableTopInstructionUI.ConfirmPlacement,
                    new SetARInstructionUIAction.InstructionUIStep
                    {
                        stepIndex = (int) TableTopInstructionUI.ConfirmPlacement, onNext = ConfirmPlacementNext,
                        onBack = ConfirmPlacementBack
                    }
                },
                {
                    TableTopInstructionUI.OnBoardingComplete,
                    new SetARInstructionUIAction.InstructionUIStep
                        {stepIndex = (int) TableTopInstructionUI.OnBoardingComplete, onNext = OnBoardingCompleteNext}
                },
            };
            Dispatcher.Dispatch(SelectObjectAction.From(new ObjectSelectionInfo()));
        }

        void DisposeSelectors()
        {
            m_PlacementRuleGameObjectSelector?.Dispose();
            m_FirstSelectedPlaneSelector?.Dispose();
            m_SecondSelectedPlaneSelector?.Dispose();
            m_ObjectSelectionInfoSelector?.Dispose();
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

        IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0);
            Dispatcher.Dispatch(CancelAction.From(false));
        }

        public void Next()
        {
            if (!CurrentInstructionStep.CheckValidations(m_FirstSelectedPlaneSelector.GetValue(), m_SecondSelectedPlaneSelector.GetValue(), ((ObjectSelectionInfo)m_ObjectSelectionInfoSelector.GetValue()).CurrentSelectedObject()))
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
            Dispatcher.Dispatch(ShowModelAction.From( false));
            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Init));
            Dispatcher.Dispatch(SetARInstructionUIAction.From(new { currentARInstructionUI = this }));
            Dispatcher.Dispatch(SetAREnabledAction.From(true));

            m_Raycaster.Reset();
            m_ARModeUIController.ActivePlacementRules(SetModelFloorAction.PlacementRule.TableTopPlacementRule);
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.TableTopPlacementRule));

            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));

            // default scale 1:100
            Dispatcher.Dispatch(SetModelScaleAction.From(SetModelScaleAction.ArchitectureScale.OneToOneHundred));

            Dispatcher.Dispatch(EnableAllNavigationAction.From(false));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(true));
            Dispatcher.Dispatch(SetInstructionMode.From(true));

            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
            Dispatcher.Dispatch(
                ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = true;
            toolState.scaleEnabled = true;
            toolState.cancelEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));

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
            m_PlacementRuleGameObjectSelector.GetValue().SetActive(true);
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() {text = m_InstructionFindAPlaneText, type = StatusMessageType.Instruction}));
            m_Raycaster.ActiveScanning = true;
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From( false));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = false;
            toolState.okButtonValidator = this;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            toolState.selectionEnabled = false;
            toolState.measureToolEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
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
            if (m_PlacementRuleGameObjectSelector.GetValue() != null)
                m_PlacementRuleGameObjectSelector.GetValue().SetActive(false);
            m_Raycaster.ActiveScanning = false;

            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(true));
            Dispatcher.Dispatch(ShowModelAction.From(false));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData()
                    {text = m_InstructionConfirmPlacementText, type = StatusMessageType.Instruction}));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.rotateEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));
        }

        void OnBoardingCompleteNext()
        {
            Dispatcher.Dispatch(SetInstructionMode.From( false));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARSidebar));
            Dispatcher.Dispatch(ShowModelAction.From( true));
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(false));
            Dispatcher.Dispatch(
                SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Completed));
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.rotateEnabled = false;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(true));
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
