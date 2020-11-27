using System;
using SharpFlux;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
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
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        FanOutWindow m_FanOutgWindow;
        ButtonControl m_ActiveButtonControl;
        DeviceCapability? m_DeviceCapability;
        ARMode? m_CachedARMode;
        NavigationMode? m_NavigationMode;
        DialogType? m_ActiveDialog;

        Dictionary<NavigationMode, string> m_SceneDictionary = new Dictionary<NavigationMode, string>();

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_FanOutgWindow = GetComponent<FanOutWindow>();

            m_NavigationButton.buttonClicked += OnNavigationButtonClicked;

            m_OrbitButton.buttonClicked += OnOrbitButtonClicked;
            m_ARButton.buttonClicked += OnARButtonClicked;
            m_VRButton.buttonClicked += OnVRButtonClicked;

            foreach (var info in UIStateManager.current.stateData.navigationState.navigationModeInfos)
            {
                m_SceneDictionary[info.navigationMode] = info.modeScenePath;
            }
        }

        [ContextMenu(nameof(OnVRButtonClicked))]
        void OnVRButtonClicked()
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            var currentNavigationMode = navigationState.navigationMode;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_SceneDictionary[currentNavigationMode]));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.VRSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetTheme, ThemeController.k_VROpaque));

            navigationState.EnableAllNavigation(true);
            navigationState.navigationMode = NavigationMode.VR;
            navigationState.showScaleReference = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_SceneDictionary[NavigationMode.VR]));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));

        }

        [ContextMenu(nameof(OnARButtonClicked))]
        void OnARButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, DialogType.ARCardSelection));
        }

        [ContextMenu(nameof(OnOrbitButtonClicked))]
        void OnOrbitButtonClicked()
        {
            var navigationState = UIStateManager.current.stateData.navigationState;
            var currentNavigationMode = navigationState.navigationMode;

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ShowModel, true));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.EnableAR, false));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_SceneDictionary[currentNavigationMode]));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.OrbitSidebar));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetTheme, ThemeController.k_Default));

            navigationState.EnableAllNavigation(true);
            navigationState.navigationMode = NavigationMode.Orbit;
            navigationState.showScaleReference = false;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationState, navigationState));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_SceneDictionary[NavigationMode.Orbit]));

            SettingsToolStateData settingsToolState = SettingsToolStateData.defaultData;
            settingsToolState.bimFilterEnabled = true;
            settingsToolState.sceneOptionEnabled = true;
            settingsToolState.sunStudyEnabled = true;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSettingsToolState, settingsToolState));
        }

        [ContextMenu(nameof(OnNavigationButtonClicked))]
        void OnNavigationButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            var dialogType = m_FanOutgWindow.open ? DialogType.None : DialogType.NavigationMode;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
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
                m_ARButton.button.interactable = false;
                m_VRButton.button.interactable = false;

                if (stateData.deviceCapability.HasFlag(DeviceCapability.ARCapability))
                {
                    m_ARButton.button.interactable = true;
                }

                if (stateData.deviceCapability.HasFlag(DeviceCapability.VRCapability))
                {
                    m_VRButton.button.interactable = true;
                }

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
            }

            if (buttonIcon != null)
            {
                m_NavigationButton.SetIcon(buttonIcon.sprite);
            }
        }
    }
}
