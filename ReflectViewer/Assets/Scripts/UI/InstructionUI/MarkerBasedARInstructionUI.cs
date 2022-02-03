using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Markers.DI;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [CreateAssetMenu(fileName = "MarkerBasedARInstruction", menuName = "Reflect/MarkerBasedARInstruction", order = 53)]
    public class MarkerBasedARInstructionUI : ScriptableObject, IARInstructionUI, SetARToolStateAction.IUIButtonValidator
    {
        enum MarkerBasedInstructionUIState
        {
            Init = 0,
            Selection,
            Anchoring,

            //Alignment,
            OnBoardingComplete
        }

        public SetARModeAction.ARMode arMode => SetARModeAction.ARMode.MarkerBased;
        public SetARInstructionUIAction.InstructionUIStep CurrentInstructionStep => m_States[m_CurrentState];

        ARModeUIController m_ARModeUIController;
        Dictionary<MarkerBasedInstructionUIState, SetARInstructionUIAction.InstructionUIStep> m_States;
        MarkerBasedInstructionUIState m_CurrentState = MarkerBasedInstructionUIState.Init;

        IMarkerController m_MarkerController;
        ARSession m_ARSession;
        ARSessionOrigin m_ARSessionOrigin;

        public ExposedReference<MarkerControllerResolver> m_MarkerControllerResolverRef;
        IUISelector<GameObject> m_PlacementRuleGameObjectSelector;
        IUISelector<ARPlacementStateData> m_PlacementStateDataSelector;
        IUISelector<SetProgressStateAction.ProgressState> m_ProgressStateActionSelector;
        bool m_ScanningBarcode = false;

        const string k_Selection = "Focus view on the QR Code.";
        const string k_Alignment = "Adjust model positon, or re-align with marker.";

        public void Next()
        {
            var transition = m_States[++m_CurrentState].onNext;
            if (transition != null)
                transition();
        }

        public void Back()
        {
            var transition = m_States[--m_CurrentState].onBack;
            if (transition != null)
                transition();
        }

        public void Cancel()
        {
            if (m_MarkerController != null)
            {
                m_MarkerController?.CancelBarcode();
                if (m_MarkerController.ImageTracker != null)
                {
                    m_MarkerController.ImageTracker.OnTrackedFound -= HandleTrackableFound;
                    m_MarkerController.ImageTracker.OnTrackedPositionUpdate -= HandleTrackableUpdate;
                    m_MarkerController?.ImageTracker?.Stop();
                }
            }

            PauseAR(false);
            Dispatcher.Dispatch(SetAREnabledAction.From(false));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Dispatcher.Dispatch(ShowModelAction.From(true));
            m_CurrentState = MarkerBasedInstructionUIState.Init;
            m_ARModeUIController.StartCoroutine(AcknowledgeCancel());
            // Kick back to Orbit mode
            var data = new SetForceNavigationModeAction.ForceNavigationModeTrigger((int)SetNavigationModeAction.NavigationMode.Orbit);
            Dispatcher.Dispatch(SetForceNavigationModeAction.From(data));
        }

        IEnumerator AcknowledgeCancel()
        {
            yield return new WaitForSeconds(0.1f);
            Dispatcher.Dispatch(CancelAction.From(false));
        }

        public void Initialize(IARModeUIController resolver)
        {
            DisposeSelectors();
            m_ARModeUIController = (ARModeUIController)resolver;
            m_MarkerController = m_MarkerControllerResolverRef.Resolve(m_ARModeUIController).MarkerController;
            m_PlacementStateDataSelector = UISelectorFactory.createSelector<ARPlacementStateData>(ARContext.current, "placementStateData");
            m_PlacementRuleGameObjectSelector = UISelectorFactory.createSelector<GameObject>(ARPlacementContext.current, "placementRulesGameObject");
            m_ProgressStateActionSelector = UISelectorFactory.createSelector<SetProgressStateAction.ProgressState>(ProgressContext.current, nameof(IProgressDataProvider.progressState));

            m_States = new Dictionary<MarkerBasedInstructionUIState,SetARInstructionUIAction.InstructionUIStep>
            {
                {
                    MarkerBasedInstructionUIState.Init,
                    new SetARInstructionUIAction.InstructionUIStep()
                    {
                        stepIndex = (int)MarkerBasedInstructionUIState.Init,
                        onNext = StartInstruction
                    } },
                {
                    MarkerBasedInstructionUIState.Selection,
                    new SetARInstructionUIAction.InstructionUIStep()
                    {
                        stepIndex = (int)MarkerBasedInstructionUIState.Selection,
                        onNext = SelectionNext,
                        onBack = SelectionBack
                    }
                },
                {
                    MarkerBasedInstructionUIState.Anchoring,
                    new SetARInstructionUIAction.InstructionUIStep()
                    {
                        stepIndex = (int)MarkerBasedInstructionUIState.Anchoring,
                        onNext = AnchoringNext,
                        onBack = AnchoringBack
                    }
                },
                //{ MarkerBasedInstructionUIState.Alignment, new SetARInstructionUIAction.InstructionUIStep(){stepIndex = (int)MarkerBasedInstructionUIState.Alignment, onNext = AlignmentNext, onBack = AlignmentBack}},
                {
                    MarkerBasedInstructionUIState.OnBoardingComplete,
                    new SetARInstructionUIAction.InstructionUIStep()
                    {
                        stepIndex = (int)MarkerBasedInstructionUIState.OnBoardingComplete,
                        onNext = OnBoardingComplete
                    }
                }
            };
        }

        void DisposeSelectors()
        {
            m_PlacementStateDataSelector?.Dispose();
            m_PlacementRuleGameObjectSelector?.Dispose();
            m_ProgressStateActionSelector?.Dispose();
        }

        // On ARModeChanged
        public void Restart()
        {
            m_ARModeUIController.StartCoroutine(ResetInstructionUI());
        }

        IEnumerator ResetInstructionUI()
        {
            yield return new WaitForSeconds(0.1f);
            if (m_MarkerController != null)
            {
                m_MarkerController.OnMarkerUpdated -= HandleMarkerFound;
                m_MarkerController.OnBarcodeScanExit -= HandleBarcodeReaderClosed;
                m_MarkerController?.CancelBarcode();
                m_MarkerController?.ImageTracker?.Stop();
                m_ScanningBarcode = false;
            }
            m_CurrentState = MarkerBasedInstructionUIState.Init;
            m_States[m_CurrentState].onNext();
        }

        // On DisableARMode
        public void Reset()
        {
            Restart();
        }

        public bool ButtonValidate()
        {
            return true;
        }

        // Initializing state
        void StartInstruction()
        {
            // Set initial state, hide model so it doesn't occlude any markers in the user's view.
            Dispatcher.Dispatch(ShowModelAction.From(false));
            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Init));
  			Dispatcher.Dispatch(SetAREnabledAction.From(false));

            Dispatcher.Dispatch(SetARInstructionUIAction.From(new { currentARInstructionUI = this }));
            Dispatcher.Dispatch(SetInstructionMode.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Next();
        }


        void SelectionNext()
        {
            PauseAR(true);
            m_ARModeUIController.StartCoroutine(SelectionSoon());
        }

        void SelectionBack()
        {
            PauseAR(true);
            if (m_MarkerController != null)
            {
                m_MarkerController?.ImageTracker?.Stop();
                // Clear any queued marker when going back to the selection process.
                m_MarkerController.QueuedMarkerId = null;
            }
            m_ARModeUIController.StartCoroutine(SelectionSoon());
        }

        void PauseAR(bool pause)
        {
            if (!m_ARSession)
                m_ARSession = FindObjectOfType<ARSession>(true);
            if (!m_ARSessionOrigin)
                m_ARSessionOrigin = FindObjectOfType<ARSessionOrigin>(true);
            if (m_ARSession)
                m_ARSession.gameObject.SetActive(!pause);
            if (m_ARSessionOrigin)
                m_ARSessionOrigin.gameObject.SetActive(!pause);
        }

        IEnumerator SelectionSoon()
        {
            yield return new WaitForSeconds(1f);
            SelectionView();
        }

        void SelectionView()
        {
            // If there is a queued marker, then select it. otherwise open the barcode scanner.
            if (m_MarkerController.QueuedMarkerId != null)
            {
                var queuedMarker = m_MarkerController.MarkerStorage.Get(m_MarkerController.QueuedMarkerId);
                if (queuedMarker.HasValue)
                {
                    m_MarkerController.ActiveMarker = queuedMarker.Value;
                    HandleMarkerFound(queuedMarker.Value);
                    return;
                }
            }
            if (!m_ScanningBarcode)
            {
                m_MarkerController.OnMarkerUpdated += HandleMarkerFound;
                m_MarkerController.OnBarcodeScanExit += HandleBarcodeReaderClosed;
                m_ScanningBarcode = true;
                // Open QR Reader
                m_MarkerController.ScanBarcode();
            }
        }

        void HandleMarkerFound(IMarker foundMarker)
        {
            m_MarkerController.OnMarkerUpdated -= HandleMarkerFound;
            m_MarkerController.OnBarcodeScanExit -= HandleBarcodeReaderClosed;
            m_ScanningBarcode = false;
            m_MarkerController.CancelBarcode();
            if (m_CurrentState != MarkerBasedInstructionUIState.Selection)
                return;

            Next();
        }

        void HandleBarcodeReaderClosed()
        {
            if (m_MarkerController == null)
                return;
            m_MarkerController.OnMarkerUpdated -= HandleMarkerFound;
            m_MarkerController.OnBarcodeScanExit -= HandleBarcodeReaderClosed;
            m_ScanningBarcode = false;

            Cancel();
        }

        bool SelectionValidate()
        {
             if (m_MarkerController.ActiveMarker != null)
                 return true;
             return false;
        }

        void AnchoringNext()
        {
            m_MarkerController.OnMarkerUpdated -= HandleMarkerFound;
            m_MarkerController.OnBarcodeScanExit -= HandleBarcodeReaderClosed;

            PauseAR(false);
            Dispatcher.Dispatch(SetAREnabledAction.From(true));
            m_ARModeUIController.StartCoroutine(AnchoringNextSoon());
        }

        IEnumerator AnchoringNextSoon()
        {
            yield return new WaitForSeconds(1f);
            AnchoringView();
        }

        void AnchoringBack()
        {
            AnchoringView();
        }

        void AnchoringView()
        {
            var instruction = $"Locate anchor for {m_MarkerController.ActiveMarker.Name}, then press Ok.";
            Dispatcher.Dispatch(SetStatusMessageWithType.From(new StatusMessageData { text=instruction, type = StatusMessageType.Instruction}));

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okButtonValidator = null;
            toolState.okEnabled = false;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARInstructionSidebar));

            Dispatcher.Dispatch(ShowModelAction.From(false));

            Dispatcher.Dispatch(EnableBimFilterAction.From(false));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(false));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(false));

            Dispatcher.Dispatch(SetAREnabledAction.From(true));
            Dispatcher.Dispatch(SetARPlacementRuleAction.From(SetModelFloorAction.PlacementRule.MarkerPlacementRule));

            // Run image tracking, watch for a target
            // Show accept button when a target is available.

            m_MarkerController.ImageTracker.OnTrackedFound += HandleTrackableFound;
            m_MarkerController.ImageTracker.OnTrackedPositionUpdate += HandleTrackableUpdate;

            m_PlacementRuleGameObjectSelector.GetValue().SetActive(false);

            m_MarkerController.ImageTracker.Run();
        }

        void AlignmentNext()
        {
            // Show model
            AlignmentView();
        }

        void AlignmentBack()
        {
            AlignmentView();
        }

        void AlignmentView()
        {
            // Stop image tracker
            m_MarkerController.ImageTracker.Stop();

            // Show model
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(true));
            Dispatcher.Dispatch(ShowModelAction.From(false));
            Dispatcher.Dispatch(SetModelScaleAction.From(SetModelScaleAction.ArchitectureScale.OneToOne));
            // Show tool for making minor adjustments
            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okButtonValidator = this;

            // Show complete button
            toolState.okEnabled = true;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            toolState.previousStepEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));

            // Show button for Re-anchoring to updated image tracker position.
            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = k_Alignment, type = StatusMessageType.Instruction }));

            m_ARModeUIController.StartCoroutine(VisualizeWhenLoaded());
        }

        IEnumerator VisualizeWhenLoaded()
        {
            yield return new WaitUntil(() => m_ProgressStateActionSelector.GetValue() == SetProgressStateAction.ProgressState.NoPendingRequest);
            m_MarkerController.Visualize(m_MarkerController.ActiveMarker);
        }

        void HandleTrackableFound(Pose pose, string trackableId)
        {
            if (m_MarkerController.MarkerStorage.Markers == null || m_MarkerController.MarkerStorage.Markers.Count == 0)
            {
                Debug.LogError("No Markers");
                return;
            }

            if (m_MarkerController.ActiveMarker == null)
            {
                Debug.LogError("No Marker available!");
                return;
            }

            m_MarkerController.CurrentMarkerPose = pose;

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okButtonValidator = this;
            toolState.okEnabled = true;
            toolState.scaleEnabled = false;
            toolState.rotateEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));
        }

        void HandleTrackableUpdate(Pose pose, string trackableId)
        {
            m_MarkerController.CurrentMarkerPose = pose;
        }

        // Finalization State
        void OnBoardingComplete()
        {
            m_MarkerController.ImageTracker.OnTrackedFound -= HandleTrackableFound;
            m_MarkerController.ImageTracker.OnTrackedPositionUpdate -= HandleTrackableUpdate;
            // Stop image tracker
            m_MarkerController.ImageTracker.Stop();
            Dispatcher.Dispatch(SetInstructionMode.From(false));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.ARSidebar));

            Dispatcher.Dispatch(ShowModelAction.From(true));
            Dispatcher.Dispatch(ShowBoundingBoxModelAction.From(false));
            Dispatcher.Dispatch(SetInstructionUIStateAction.From(SetInstructionUIStateAction.InstructionUIState.Completed));

            var toolState = SetARToolStateAction.SetARToolStateData.defaultData;
            toolState.okEnabled = true;
            toolState.okButtonValidator = null;
            toolState.previousStepEnabled = true;
            toolState.cancelEnabled = true;
            toolState.scaleEnabled = true;
            toolState.rotateEnabled = false;
            toolState.selectionEnabled = true;
            toolState.measureToolEnabled = true;
            toolState.navigationEnabled = false;
            toolState.arWallIndicatorsEnabled = false;
            toolState.arAnchorPointsEnabled = false;
            Dispatcher.Dispatch(SetARToolStateAction.From(toolState));

            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(false));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(true));

            // Finally set the model's transform to the marker
            m_ARModeUIController.StartCoroutine(VisualizeWhenLoaded());
        }
    }
}
