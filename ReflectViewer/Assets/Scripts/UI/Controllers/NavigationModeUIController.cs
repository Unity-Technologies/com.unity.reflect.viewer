using System;
using SharpFlux;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;
using UnityEngine.Reflect.Utils;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.Events;
using System.IO;
using System.Collections;

namespace Unity.Reflect.Viewer.UI
{

    [Serializable]
    public class BadVRConfigurationEvent : UnityEvent<bool, Action> { }

    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    [RequireComponent(typeof(FanOutWindow))]
    public class NavigationModeUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        GameObject m_FanOutControl;
        [SerializeField, Tooltip("Button to toggle the dialog.")]
        ToolButton m_NavigationButton;
        [SerializeField]
        ToolButton m_OrbitButton;
        [SerializeField]
        ToolButton m_ARButton;
        [SerializeField]
        ToolButton m_VRButton;
        [SerializeField]
        ToolButton m_WalkButton;

        [SerializeField]
        Transform m_RadialLayoutParent;
        [SerializeField]
        Transform m_LinearLayoutParent;
#pragma warning restore CS0649
        bool m_ToolbarsEnabled;
        FanOutWindow m_FanOutgWindow;
        ButtonControl m_ActiveButtonControl;
        DeviceCapability? m_DeviceCapability;
        ARMode? m_CachedARMode;
        NavigationMode? m_NavigationMode;
        DialogType? m_ActiveDialog;

        Dictionary<NavigationMode, string> m_SceneDictionary = new Dictionary<NavigationMode, string>();

        public BadVRConfigurationEvent badVRConfigurationEvent;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            UIStateManager.walkStateChanged += OnWalkStateDataChanged;

            m_FanOutgWindow = GetComponent<FanOutWindow>();

            m_NavigationButton.buttonClicked += OnNavigationButtonClicked;

            m_OrbitButton.buttonClicked += OnOrbitButtonClicked;
            m_ARButton.buttonClicked += OnARButtonClicked;
            m_VRButton.buttonClicked += OnVRButtonClicked;
            m_WalkButton.buttonClicked += OnWalkButtonClicked;

            foreach (var info in UIStateManager.current.stateData.navigationState.navigationModeInfos)
            {
                m_SceneDictionary[info.navigationMode] = info.modeScenePath;
            }

            InitFanMode();
        }

        void OnWalkStateDataChanged(UIWalkStateData walkData)
        {
            m_WalkButton.selected = walkData.walkEnabled;
            m_WalkButton.button.interactable = !walkData.walkEnabled;
        }

        [ContextMenu(nameof(OnWalkButtonClicked))]
        void OnWalkButtonClicked()
        {
            if (UIStateManager.current.stateData.navigationState.navigationMode == NavigationMode.AR)
            {
                OnOrbitButtonClicked();
            }

            var navigationState = UIStateManager.current.stateData.navigationState;
            navigationState.navigationMode = NavigationMode.Walk;
            m_ARButton.button.interactable = false;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableWalk, true));

            UIStateManager.current.walkStateData.instruction.Next();
        }

        [ContextMenu(nameof(OnVRButtonClicked))]
        void OnVRButtonClicked()
        {

#if !UNITY_EDITOR
            DetectOpenXRSetup(out bool APIDetectedRuntime, out bool VRDeviceDisconnected);

            if (!APIDetectedRuntime)
            {
                badVRConfigurationEvent?.Invoke(VRDeviceDisconnected, StartDelayActivateVRmode);
            }
            else
            {
                ActivateVRmode();
            }
#else
            // No possible detection from OpenXRRuntime Class in the Editor
            // User will configure its OpenXR setup in the Player Settings
            ActivateVRmode();
#endif

        }

        void StartDelayActivateVRmode()
        {
            StartCoroutine(DelayActivateVRmode());
        }

        IEnumerator DelayActivateVRmode()
        {
            yield return new WaitForSeconds(1);
            ActivateVRmode();
        }

        void ActivateVRmode() {
            m_WalkButton.button.interactable = false;
            m_VRButton.button.interactable = false;
            var navigationState = UIStateManager.current.stateData.navigationState;
            var currentNavigationMode = navigationState.navigationMode;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));

            if (currentNavigationMode != NavigationMode.Walk)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_SceneDictionary[currentNavigationMode]));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.VRSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetTheme, ThemeController.k_VROpaque));

            if(UIStateManager.current.walkStateData.walkEnabled)
                UIStateManager.current.walkStateData.instruction.Cancel();

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableVR, true));

            navigationState.EnableAllNavigation(true);
            navigationState.navigationMode = NavigationMode.VR;
            navigationState.showScaleReference = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_SceneDictionary[NavigationMode.VR]));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetMeasureToolOptions,  MeasureToolStateData.defaultData));
        }

        [ContextMenu(nameof(OnARButtonClicked))]
        void OnARButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.ARCardSelection));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableVR, false));
        }

        [ContextMenu(nameof(OnOrbitButtonClicked))]
        void OnOrbitButtonClicked()
        {
            m_WalkButton.button.interactable = true;

            var navigationState = UIStateManager.current.stateData.navigationState;
            var currentNavigationMode = navigationState.navigationMode;
            CheckDeviceCapability(UIStateManager.current.stateData.deviceCapability);

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableVR, false));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusInstructionMode, false));

            if (currentNavigationMode != NavigationMode.Walk)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_SceneDictionary[currentNavigationMode]));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ResetHomeView, null));
            }

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.OrbitSidebar));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetTheme, ThemeController.k_Default));

            if(UIStateManager.current.walkStateData.walkEnabled)
                UIStateManager.current.walkStateData.instruction.Cancel();

            navigationState.EnableAllNavigation(true);
            navigationState.navigationMode = NavigationMode.Orbit;
            navigationState.showScaleReference = false;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_SceneDictionary[NavigationMode.Orbit]));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = true;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        [ContextMenu(nameof(OnNavigationButtonClicked))]
        void OnNavigationButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            var dialogType = m_FanOutgWindow.open ? DialogType.None : DialogType.NavigationMode;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            if (m_ActiveDialog != stateData.activeDialog)
            {
                if (stateData.activeDialog == DialogType.ARCardSelection)
                {
                    m_NavigationButton.SetIcon(m_ARButton.buttonIcon.sprite);
                    m_NavigationButton.selected = true;
                }
                else
                {
                    if (m_NavigationButton.selected != (stateData.activeDialog == DialogType.NavigationMode))
                    {
                        SetNavigationButtonIcon(stateData.navigationState.navigationMode);
                    }

                    m_NavigationButton.selected = stateData.activeDialog == DialogType.NavigationMode;
                }

                m_ActiveDialog = stateData.activeDialog;
            }

            if (m_DeviceCapability != stateData.deviceCapability)
            {
               CheckDeviceCapability(stateData.deviceCapability);
               m_DeviceCapability = stateData.deviceCapability;
            }

            if (m_ToolbarsEnabled != stateData.toolbarsEnabled)
            {
                m_NavigationButton.button.interactable = stateData.toolbarsEnabled;
                m_ToolbarsEnabled = stateData.toolbarsEnabled;
            }

            if (m_NavigationMode != stateData.navigationState.navigationMode)
            {
                SetNavigationButtonIcon(stateData.navigationState.navigationMode);
                m_NavigationMode = stateData.navigationState.navigationMode;
            }
        }

        void CheckDeviceCapability(DeviceCapability deviceCapability)
        {
            m_ARButton.button.interactable = false;
            m_VRButton.button.interactable = false;

            if (deviceCapability.HasFlag(DeviceCapability.ARCapability))
            {
                m_ARButton.button.interactable = true;
            }

            if (deviceCapability.HasFlag(DeviceCapability.VRCapability))
            {
                m_VRButton.button.interactable = true;
            }
        }

        void SetNavigationButtonIcon(NavigationMode navigationMode)
        {
            Image buttonIcon = null;
            switch (navigationMode)
            {
                case NavigationMode.Orbit:
                    buttonIcon = m_OrbitButton.buttonIcon;
                    break;
                case NavigationMode.AR:
                    buttonIcon = m_ARButton.buttonIcon;
                    break;
                case NavigationMode.VR:
                    buttonIcon = m_VRButton.buttonIcon;
                    break;
                case NavigationMode.Walk:
                    buttonIcon = m_WalkButton.buttonIcon;
                    break;
            }

            if (buttonIcon != null)
            {
                m_NavigationButton.SetIcon(buttonIcon.sprite);
            }
        }

        void DetectOpenXRSetup(out bool APIDetectedRuntime, out bool VRDeviceDisconnected)
        {
            // 0.0.0 is the default value when no device is connected, or when no Krhonos registry ActiveRuntime key is provided
            APIDetectedRuntime = !OpenXRRuntime.version.Equals("0.0.0");
            VRDeviceDisconnected = false;

            var enabledExtensions = string.Join(",", OpenXRRuntime.GetEnabledExtensions());
            Debug.Log($"OPENXR API detected Capabilities: {OpenXRRuntime.name}, {OpenXRRuntime.apiVersion}, {OpenXRRuntime.pluginVersion}, {OpenXRRuntime.version}, {enabledExtensions}");

#if UNITY_STANDALONE_WIN
            // Lets see if we can differantiate a disconnected device from a missing OpenXR setup
            if (!APIDetectedRuntime)
            {
                // Best effort detection as accessing LocalMachine registry key hive can be denied for security
                try
                {
                    using (var reflectKey = Microsoft.Win32.Registry.LocalMachine.OpenSubKey("SOFTWARE\\Khronos\\OpenXR\\1"))
                    {
                        var detectedActiveRuntimeJson = reflectKey.GetValue("ActiveRuntime");
                        if (detectedActiveRuntimeJson != null && File.Exists(detectedActiveRuntimeJson.ToString()))
                        {
                            VRDeviceDisconnected = true;
                        }
                    }
                }
                catch (Exception) { }
            }
#endif
        }

        void InitFanMode()
        {
            var screenDpi = UIUtils.GetScreenDpi();
            var deviceType = UIUtils.GetDeviceType(Screen.width, Screen.height, screenDpi);
            bool linearFanMode = deviceType == DisplayType.Phone;

            var layoutParent = linearFanMode ? m_LinearLayoutParent : m_RadialLayoutParent;
            m_ARButton.transform.parent.SetParent(layoutParent);
            m_ARButton.transform.parent.localScale = Vector3.one;
            m_VRButton.transform.parent.SetParent(layoutParent);
            m_VRButton.transform.parent.localScale = Vector3.one;
            m_OrbitButton.transform.parent.SetParent(layoutParent);
            m_OrbitButton.transform.parent.localScale = Vector3.one;
            m_WalkButton.transform.parent.SetParent(layoutParent);
            m_WalkButton.transform.parent.localScale = Vector3.one;
        }
    }
}
