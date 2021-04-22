using System.Collections;
using System.Collections.Generic;
using System.Xml;
using SharpFlux;
using SharpFlux.Dispatching;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public class WalkModeInstruction : IWalkInstructionUI
    {
        enum WalkModeInstructionUI
        {
            Init = 0,
            Place,
            Rotate,
            Complete,
        };

        WalkModeSwitcher m_WalkModeSwitcher;

        const string k_InstructionFindAPlaneHandleText = "Tap on a point in scene to set starting point.";
        const string k_InstructionFindAPlaneDesktopText = "Point and click in scene to set starting point.";
        const string k_InstructionConfirmPlacementDesktopText = "Walk around using arrow keys.\n " +
                                                                "Right click and drag to turn your head around.\n" +
                                                                " Press space bar to jump.";
        const string k_InstructionConfirmPlacementHandheldText = "Walk around using onscreen joystick.\n " +
                                                                "Tap and drag with your second hand to turn your head around.";

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

        Dictionary<WalkModeInstructionUI, InstructionUIStep> m_States;

        public void Initialize(WalkModeSwitcher walkModeSwitcher)
        {
            m_WalkModeSwitcher = walkModeSwitcher;
            m_WalkModeInstructionUI = WalkModeInstructionUI.Init;

            m_States = new Dictionary<WalkModeInstructionUI, InstructionUIStep>
            {
                { WalkModeInstructionUI.Init, new InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Init, onNext = StartInstruction } },
                { WalkModeInstructionUI.Place, new InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Place, onNext = FindTeleportLocation, onBack = StartInstruction } },
                { WalkModeInstructionUI.Rotate, new InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Rotate, onNext = ChooseRotation, onBack = FindTeleportLocation } },
                { WalkModeInstructionUI.Complete, new InstructionUIStep { stepIndex = (int)WalkModeInstructionUI.Complete, onNext = OnComplete, onBack = ChooseRotation } },
            };
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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.OrbitSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() {text = k_PlatformDependentPlacementText[SystemInfo.deviceType], type = StatusMessageType.Instruction}));
        }

        void StartInstruction()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, true));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusWithType,
                new StatusMessageData() {text = k_PlatformDependentInstructionFindAPlaneText[SystemInfo.deviceType], type = StatusMessageType.Instruction}));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            SetNavigationState(NavigationMode.Walk);
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
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableWalk, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));
            SetNavigationState(NavigationMode.Orbit);
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

        void SetNavigationState(NavigationMode mode)
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.navigationMode = mode;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));
        }

        public InstructionUIStep CurrentInstructionStep { get; }
    }
}
