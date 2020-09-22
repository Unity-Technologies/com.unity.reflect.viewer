using SharpFlux;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
        [SerializeField, Tooltip("List of NavigationModeInfo.")]
        List<NavigationModeInfo> m_NavigationModeToolbar = new List<NavigationModeInfo>();
#pragma warning restore CS0649

        bool m_ToolbarsEnabled;
        FanOutWindow m_FanOutgWindow;
        ButtonControl m_ActiveButtonControl;
        NavigationMode? m_CachedNavigationMode;
        DeviceCapability? m_DeviceCapability;
        Dictionary<ToolbarType, string> m_SceneDictionary = new Dictionary<ToolbarType, string>();
        string m_CachedScenePath;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_FanOutgWindow = GetComponent<FanOutWindow>();

            m_NavigationButton.buttonClicked += OnNavigationButtonClicked;

            m_OrbitButton.buttonClicked += OnOrbitButtonClicked;
            m_ARButton.buttonClicked += OnARButtonClicked;
            m_VRButton.buttonClicked += OnVRButtonClicked;

            foreach (var info in m_NavigationModeToolbar)
            {
                m_SceneDictionary[info.modeToolbar] = info.modeScenePath;
            }
        }

        [ContextMenu("OnVRButtonClicked")]
        private void OnVRButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_CachedScenePath));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationMode, NavigationMode.VR));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.VRSidebar));

            m_CachedScenePath = m_SceneDictionary[ToolbarType.VRSidebar];
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_CachedScenePath));
        }

        [ContextMenu("OnARButtonClicked")]
        private void OnARButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_CachedScenePath));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationMode, NavigationMode.AR));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.ARSidebar));

            m_CachedScenePath = m_SceneDictionary[ToolbarType.ARSidebar];
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_CachedScenePath));
        }

        [ContextMenu("OnOrbitButtonClicked")]
        private void OnOrbitButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusLevel, StatusMessageLevel.Info));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.UnloadScene, m_CachedScenePath));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetNavigationMode, NavigationMode.Orbit));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));

            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.OrbitSidebar));

            m_CachedScenePath = m_SceneDictionary[ToolbarType.OrbitSidebar];
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.LoadScene, m_CachedScenePath));
        }

        [ContextMenu("OnNavigationButtonClicked")]
        private void OnNavigationButtonClicked()
        {
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            var dialogType = m_FanOutgWindow.open ? DialogType.None : DialogType.NavigationMode;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }

        void OnStateDataChanged(UIStateData stateData)
        {
            m_NavigationButton.selected = stateData.activeDialog == DialogType.NavigationMode;

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

            if (m_CachedNavigationMode != stateData.navigationState.navigationMode)
            {
                switch (stateData.navigationState.navigationMode)
                {
                    case NavigationMode.AR:
                        m_NavigationButton.SetIcon(m_ARButton.buttonIcon.sprite);
                        break;

                    case NavigationMode.VR:
                        m_NavigationButton.SetIcon(m_VRButton.buttonIcon.sprite);
                        break;

                    default:
                        m_NavigationButton.SetIcon(m_OrbitButton.buttonIcon.sprite);
                        break;
                }
                m_CachedNavigationMode = stateData.navigationState.navigationMode;
            }
        }
    }
}
