using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class WalkModeInstruction : IWalkInstructionUI
    {
        const float k_WaitingDelay = 10.0f;

        enum WalkModeInstructionUI
        {
            Init = 0,
            Place,
            Rotate,
            Complete,
        };

        WalkModeSwitcher m_WalkModeSwitcher;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;

        // Hand held instruction
        const string k_InstructionFindAPlaneHandleText = "Tap and drag to initiate your starting position and rotation";
        const string k_InstructionConfirmPlacementHandheldText = "Drag your left finger to walk and drag your right finger to turn your head around";

        // Desktop instruction
        const string k_InstructionFindAPlaneDesktopText = "Click and drag to initiate your starting position and rotation.";
        const string k_InstructionConfirmPlacementDesktopText = "Use keyboard arrow keys to walk and mouse click and drag to turn your head..";

        static readonly Dictionary<DeviceType, string> k_PlatformDependentPlacementText = new Dictionary<DeviceType, string>()
        {
            { DeviceType.Desktop, k_InstructionConfirmPlacementDesktopText },
            { DeviceType.Handheld, k_InstructionConfirmPlacementHandheldText }
        };

        static readonly Dictionary<DeviceType, string> k_PlatformDependentInstructionFindAPlaneText = new Dictionary<DeviceType, string>()
        {
            { DeviceType.Desktop, k_InstructionFindAPlaneDesktopText },
            { DeviceType.Handheld, k_InstructionFindAPlaneHandleText }
        };

        WalkModeInstructionUI m_WalkModeInstructionUI;

        Dictionary<WalkModeInstructionUI, SetARInstructionUIAction.InstructionUIStep> m_States;

        public void Initialize(WalkModeSwitcher walkModeSwitcher)
        {
            m_WalkModeSwitcher = walkModeSwitcher;
            m_WalkModeInstructionUI = WalkModeInstructionUI.Init;

            m_States = new Dictionary<WalkModeInstructionUI, SetARInstructionUIAction.InstructionUIStep>
            {
                { WalkModeInstructionUI.Init, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Init, onNext = StartInstruction } },
                { WalkModeInstructionUI.Place, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Place, onNext = FindTeleportLocation, onBack = StartInstruction } },
                { WalkModeInstructionUI.Rotate, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Rotate, onNext = ChooseRotation, onBack = FindTeleportLocation } },
                { WalkModeInstructionUI.Complete, new SetARInstructionUIAction.InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Complete, onNext = OnComplete, onBack = ChooseRotation } },
            };

            m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode));
        }

        void ChooseRotation()
        {
            Next();
        }

        void FindTeleportLocation()
        {
            m_WalkModeSwitcher.Init();
            StartInstruction();
        }

        void OnComplete()
        {
            m_WalkModeSwitcher.OnWalkStart();
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.OrbitSidebar));

            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = k_PlatformDependentPlacementText[SystemInfo.deviceType], type = StatusMessageType.Instruction }));
        }

        void StartInstruction()
        {
            Dispatcher.Dispatch(SetInstructionMode.From(true));

            Dispatcher.Dispatch(SetStatusMessageWithType.From(
                new StatusMessageData() { text = k_PlatformDependentInstructionFindAPlaneText[SystemInfo.deviceType], type = StatusMessageType.Instruction }));

            Dispatcher.Dispatch(CloseAllDialogsAction.From(null));

            SetNavigationState(SetNavigationModeAction.NavigationMode.Walk);
        }

        public void Restart()
        {
            m_WalkModeInstructionUI = WalkModeInstructionUI.Init;
            m_States[m_WalkModeInstructionUI].onNext();
        }

        public void Cancel()
        {
            m_WalkModeInstructionUI = WalkModeInstructionUI.Init;
            m_WalkModeSwitcher.OnQuitWalkMode();
            Dispatcher.Dispatch(SetWalkEnableAction.From(false));
            Dispatcher.Dispatch(SetInstructionMode.From(false));
            if (m_NavigationModeSelector.GetValue() == SetNavigationModeAction.NavigationMode.Walk)
            {
                SetNavigationState(SetNavigationModeAction.NavigationMode.Orbit);
            }
        }

        public void Reset(Vector3 offset)
        {
            m_WalkModeSwitcher.ResetCamPos(offset);
        }

        public void Next()
        {
            if (m_WalkModeInstructionUI == WalkModeInstructionUI.Complete)
                return;

            var transition = m_States[++m_WalkModeInstructionUI].onNext;
            if (transition != null)
                transition();
        }

        public void Back()
        {
            var transition = m_States[--m_WalkModeInstructionUI].onBack;
            if (transition != null)
                transition();
        }

        void SetNavigationState(SetNavigationModeAction.NavigationMode mode)
        {
            Dispatcher.Dispatch(SetNavigationModeAction.From(mode));
        }

        public IEnumerator WaitCloseStatusDialog()
        {
            yield return new WaitForSeconds(k_WaitingDelay);
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Dispatcher.Dispatch(SetInstructionMode.From(false));
        }
    }
}
