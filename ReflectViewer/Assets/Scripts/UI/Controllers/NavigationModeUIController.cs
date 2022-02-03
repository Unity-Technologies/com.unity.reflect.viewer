using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Utils;
using UnityEngine.UI;
using UnityEngine.XR.OpenXR;
using UnityEngine.Events;
using System.IO;
using System.Collections;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.XR.Management;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public class BadVRConfigurationEvent : UnityEvent<Action> { }

    /// <summary>
    /// Controller responsible for managing the selection of the type of orbit tool
    /// </summary>
    public class NavigationModeUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField, Tooltip("Button to toggle the dialog.")]
        ToolButton m_NavigationButton;
        [SerializeField]
        ToolButton m_FlyButton;
        [SerializeField]
        ToolButton m_ARButton;
        [SerializeField]
        ToolButton m_VRButton;
        [SerializeField]
        ToolButton m_WalkButton;
#pragma warning restore CS0649
        ButtonControl m_ActiveButtonControl;

        Dictionary<SetNavigationModeAction.NavigationMode, string> m_SceneDictionary = new Dictionary<SetNavigationModeAction.NavigationMode, string>();

        public BadVRConfigurationEvent badVRConfigurationEvent;
        IUISelector<SetVREnableAction.DeviceCapability> m_DeviceCapabilitySelector;
        IUISelector<IWalkInstructionUI> m_WalkInstructionSelector;
        IUISelector<SetNavigationModeAction.NavigationMode> m_NavigationModeSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveSubDialogSelector;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(m_DeviceCapabilitySelector = UISelectorFactory.createSelector<SetVREnableAction.DeviceCapability>(PipelineContext.current, nameof(IPipelineDataProvider.deviceCapability), OnDeviceCapabilityChanged));
            m_DisposeOnDestroy.Add(m_WalkInstructionSelector = UISelectorFactory.createSelector<IWalkInstructionUI>(WalkModeContext.current, nameof(IWalkModeDataProvider.instruction)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetForceNavigationModeAction.ForceNavigationModeTrigger>(ForceNavigationModeContext.current, nameof(IForceNavigationModeDataProvider.navigationMode), OnForceNavigationMode));
            m_DisposeOnDestroy.Add(m_ActiveSubDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog), OnActiveSubDialogChanged));
            m_DisposeOnDestroy.Add(m_NavigationModeSelector = UISelectorFactory.createSelector<SetNavigationModeAction.NavigationMode>(NavigationContext.current, nameof(INavigationDataProvider.navigationMode), OnNavigationModeChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(UIStateContext.current, nameof(IToolBarDataProvider.toolbarsEnabled), OnToolBarEnabledChanged));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<List<NavigationModeInfo>>(NavigationContext.current, nameof(INavigationModeInfosDataProvider<NavigationModeInfo>.navigationModeInfos),
                list =>
                {
                    if (list != null)
                    {
                        foreach (var info in list)
                        {
                            m_SceneDictionary[info.navigationMode] = info.modeScenePath;
                        }
                    }
                }));

            m_NavigationButton.buttonClicked += OnNavigationButtonClicked;

            m_FlyButton.selected = true;
            m_FlyButton.buttonClicked += OnFlyButtonClicked;
            m_ARButton.buttonClicked += OnARButtonClicked;
            m_VRButton.buttonClicked += OnVRButtonClicked;
            m_WalkButton.buttonClicked += OnWalkButtonClicked;
        }

        void OnToolBarEnabledChanged(bool newData)
        {
            m_NavigationButton.button.interactable = newData;
        }

        void OnNavigationModeChanged(SetNavigationModeAction.NavigationMode newData)
        {
            SetNavigationButtonIcon(newData);
            SetNavigationButtonSelected(newData);
        }

        void OnActiveSubDialogChanged(OpenDialogAction.DialogType newData)
        {
            if (newData == OpenDialogAction.DialogType.ARCardSelection)
            {
                m_NavigationButton.SetIcon(m_ARButton.buttonIcon.sprite);
                m_NavigationButton.selected = true;
            }
            else
            {
                if (m_NavigationButton.selected != (newData == OpenDialogAction.DialogType.NavigationMode))
                {
                    SetNavigationButtonIcon(m_NavigationModeSelector.GetValue());
                }

                m_NavigationButton.selected = newData == OpenDialogAction.DialogType.NavigationMode;
            }
        }

        void OnForceNavigationMode(SetForceNavigationModeAction.ForceNavigationModeTrigger data)
        {
            if (data.trigger)
            {
                IEnumerator WaitOneFrame()
                {
                    yield return null;
                    switch ((SetNavigationModeAction.NavigationMode)data.mode)
                    {
                        case SetNavigationModeAction.NavigationMode.Orbit:
                        case SetNavigationModeAction.NavigationMode.Fly:
                        {
                                OnFlyButtonClicked();
                                break;
                            }
                        case SetNavigationModeAction.NavigationMode.Walk:
                            {
                                OnWalkButtonClicked();
                                break;
                            }
                        case SetNavigationModeAction.NavigationMode.AR:
                            {
                                OnARButtonClicked();
                                break;
                            }
                        case SetNavigationModeAction.NavigationMode.VR:
                            {
                                OnVRButtonClicked();
                                break;
                            }
                    }
                }
                StartCoroutine(WaitOneFrame());
            }
        }

        void OnDeviceCapabilityChanged(SetVREnableAction.DeviceCapability newData)
        {
            CheckDeviceCapability(newData);
        }

        void OnWalkStateDataChanged(UIWalkStateData walkData)
        {
            m_WalkButton.selected = walkData.walkEnabled;
            m_WalkButton.button.interactable = !walkData.walkEnabled;
        }

        [ContextMenu(nameof(OnWalkButtonClicked))]
        void OnWalkButtonClicked()
        {
            if (m_NavigationModeSelector.GetValue() == SetNavigationModeAction.NavigationMode.AR)
            {
                OnFlyButtonClicked();
            }

            Dispatcher.Dispatch(CloseAllDialogsAction.From(null));
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.Walk));
            Dispatcher.Dispatch(SetWalkEnableAction.From(true));

            m_WalkInstructionSelector.GetValue().Next();
        }

        [ContextMenu(nameof(OnVRButtonClicked))]
        void OnVRButtonClicked()
        {
            StartCoroutine(DetectVRDevice());
        }

        IEnumerator DetectVRDevice()
        {
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            yield return XRGeneralSettings.Instance.Manager.InitializeLoader();

            if (XRGeneralSettings.Instance.Manager.activeLoader != null)
            {
                ActivateVRmode();
            }
            else
            {
                badVRConfigurationEvent?.Invoke(OnVRButtonClicked);
            }
        }

        void ActivateVRmode()
        {
            m_WalkButton.button.interactable = false;
            m_VRButton.button.interactable = false;
            var currentNavigationMode = m_NavigationModeSelector.GetValue();

            Dispatcher.Dispatch(SetAREnabledAction.From(false));

            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));

            Dispatcher.Dispatch(SetInstructionMode.From(false));

            if (currentNavigationMode != SetNavigationModeAction.NavigationMode.Walk)
                Dispatcher.Dispatch(UnloadSceneActions<Project>.From(m_SceneDictionary[currentNavigationMode]));

            Dispatcher.Dispatch(CloseAllDialogsAction.From(null));

            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.VRSidebar));

            Dispatcher.Dispatch(SetThemeAction.From(ThemeController.k_VROpaque));

            Dispatcher.Dispatch(SetWalkEnableAction.From(false));

            Dispatcher.Dispatch(SetVREnableAction.From(true));

            Dispatcher.Dispatch(EnableAllNavigationAction.From(true));
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.VR));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(false));

            Dispatcher.Dispatch(LoadSceneActions<Project>.From(m_SceneDictionary[SetNavigationModeAction.NavigationMode.VR]));

            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(true));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(true));

            Dispatcher.Dispatch(ToggleMeasureToolAction.From(ToggleMeasureToolAction.ToggleMeasureToolData.defaultData));
        }

        [ContextMenu(nameof(OnARButtonClicked))]
        void OnARButtonClicked()
        {
            Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));
            Dispatcher.Dispatch(OpenDialogAction.From(OpenDialogAction.DialogType.ARCardSelection));
            Dispatcher.Dispatch(SetVREnableAction.From(false));
        }

        [ContextMenu(nameof(OnFlyButtonClicked))]
        void OnFlyButtonClicked()
        {
            Dispatcher.Dispatch(CloseAllDialogsAction.From(null));

            var currentNavigationMode = m_NavigationModeSelector.GetValue();
            if (currentNavigationMode == SetNavigationModeAction.NavigationMode.Orbit ||
                currentNavigationMode == SetNavigationModeAction.NavigationMode.Fly)
            {
                return;
            }

            m_WalkButton.button.interactable = true;

            CheckDeviceCapability(m_DeviceCapabilitySelector.GetValue());

            Dispatcher.Dispatch(ShowModelAction.From(true));

            Dispatcher.Dispatch(SetAREnabledAction.From(false));

            Dispatcher.Dispatch(SetVREnableAction.From(false));

            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
            Dispatcher.Dispatch(SetInstructionMode.From(false));

            if (currentNavigationMode != SetNavigationModeAction.NavigationMode.Walk)
            {
                Dispatcher.Dispatch(UnloadSceneActions<Project>.From(m_SceneDictionary[currentNavigationMode]));
                Dispatcher.Dispatch(SetCameraViewTypeAction.From(SetCameraViewTypeAction.CameraViewType.Default));
            }

            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.OrbitSidebar));

            Dispatcher.Dispatch(SetThemeAction.From(ThemeController.k_Default));

            Dispatcher.Dispatch(SetWalkEnableAction.From(false));

            Dispatcher.Dispatch(EnableAllNavigationAction.From(true));
            Dispatcher.Dispatch(SetNavigationModeAction.From(SetNavigationModeAction.NavigationMode.Orbit));
            Dispatcher.Dispatch(SetShowScaleReferenceAction.From(false));

            Dispatcher.Dispatch(LoadSceneActions<Project>.From(m_SceneDictionary[SetNavigationModeAction.NavigationMode.Orbit]));

            Dispatcher.Dispatch(EnableBimFilterAction.From(true));
            Dispatcher.Dispatch(EnableSceneSettingsAction.From(true));
            Dispatcher.Dispatch(EnableSunStudyAction.From(true));
            Dispatcher.Dispatch(EnableMarkerSettingsAction.From(true));

            var arToolStateData = SetARToolStateAction.SetARToolStateData.defaultData;
            arToolStateData.selectionEnabled = true;
            arToolStateData.measureToolEnabled = true;
            Dispatcher.Dispatch(SetARToolStateAction.From(arToolStateData));
        }

        /// <summary>
        /// Switch application to Orbit Mode
        /// </summary>
        public void StartOrbitMode()
        {
            OnFlyButtonClicked();
        }

        [ContextMenu(nameof(OnNavigationButtonClicked))]
        void OnNavigationButtonClicked()
        {
            var dialogType = m_ActiveSubDialogSelector.GetValue() == OpenDialogAction.DialogType.NavigationMode ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.NavigationMode;
            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));
        }

        void CheckDeviceCapability(SetVREnableAction.DeviceCapability deviceCapability)
        {
            m_ARButton.gameObject.SetActive(false);
            m_ARButton.button.interactable = false;

            m_VRButton.gameObject.SetActive(false);
            m_VRButton.button.interactable = false;

            if (deviceCapability.HasFlag(SetVREnableAction.DeviceCapability.ARCapability))
            {
                m_ARButton.gameObject.SetActive(true);
                m_ARButton.button.interactable = true;
            }

            if (deviceCapability.HasFlag(SetVREnableAction.DeviceCapability.VRCapability))
            {
                m_VRButton.gameObject.SetActive(true);
                m_VRButton.button.interactable = true;
            }
        }

        void SetNavigationButtonIcon(SetNavigationModeAction.NavigationMode navigationMode)
        {
            Image buttonIcon = null;
            switch (navigationMode)
            {
                case SetNavigationModeAction.NavigationMode.Orbit:
                case SetNavigationModeAction.NavigationMode.Fly:
                    buttonIcon = m_FlyButton.buttonIcon;
                    break;
                case SetNavigationModeAction.NavigationMode.AR:
                    buttonIcon = m_ARButton.buttonIcon;
                    break;
                case SetNavigationModeAction.NavigationMode.VR:
                    buttonIcon = m_VRButton.buttonIcon;
                    break;
                case SetNavigationModeAction.NavigationMode.Walk:
                    buttonIcon = m_WalkButton.buttonIcon;
                    break;
            }

            if (buttonIcon != null)
            {
                m_NavigationButton.SetIcon(buttonIcon.sprite);
            }
        }

        void SetNavigationButtonSelected(SetNavigationModeAction.NavigationMode mode)
        {
            m_FlyButton.selected = false;
            m_WalkButton.selected = false;
            m_ARButton.selected = false;
            m_VRButton.selected = false;
            switch (mode)
            {
                case SetNavigationModeAction.NavigationMode.Fly:
                case SetNavigationModeAction.NavigationMode.Orbit:
                    m_FlyButton.selected = true;
                    break;
                case SetNavigationModeAction.NavigationMode.Walk:
                    m_WalkButton.selected = true;
                    break;
                case SetNavigationModeAction.NavigationMode.AR:
                    m_ARButton.selected = true;
                    break;
                case SetNavigationModeAction.NavigationMode.VR:
                    m_VRButton.selected = true;
                    break;
            }
        }
    }
}
