using System;
using System.Collections.Generic;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class SceneOptionsUIController : MonoBehaviour
    {
#pragma warning disable 649

        [SerializeField]
        ToolButton m_DialogButton;
        [SerializeField]
        SlideToggle m_TextureToggle;
        [SerializeField]
        SlideToggle m_LightDataToggle;
        [SerializeField]
        TMP_Dropdown m_SkyboxDropdown;
        [SerializeField]
        SlideToggle m_SimulationToggle;
        [SerializeField]
        TMP_Dropdown m_WeatherDropdown;
        [SerializeField]
        SlideToggle m_StatsInfoToggle;
        [SerializeField]
        SlideToggle m_FilterHLODsToggle;
        [SerializeField]
        SlideToggle m_DebugOptionToggle;
        [SerializeField]
        GameObject m_DebugOptionMenu;

        [SerializeField]
        GameObject m_OrbitTypeMenu;
        [SerializeField]
        TMP_Dropdown m_OrbitDropdown;

        [SerializeField]
        MinMaxPropertyControl m_TemperatureControl;
        [SerializeField]
        TextMeshProUGUI m_TemperatureText;
        [SerializeField]
        TextMeshProUGUI m_TimeOfDayText;
        [SerializeField]
        TextMeshProUGUI m_TimeOfYearText;
        [SerializeField]
        TextMeshProUGUI m_UtcOffsetText;
        [SerializeField]
        TextMeshProUGUI m_LatitudeText;
        [SerializeField]
        TextMeshProUGUI m_LongitudeText;
        [SerializeField]
        TextMeshProUGUI m_NorthAngleText;
#pragma warning restore 649

        DialogWindow m_DialogWindow;
        bool m_ButtonVisibility;


        IUISelector<SkyboxData> m_SkyboxDatSelector;
        IUISelector<WeatherType> m_WeatherTypeSelector;
        IUISelector<bool> m_VREnableSelector;
        IUISelector<SetOrbitTypeAction.OrbitType> m_TouchOrbitTypeSelector;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        IUISelector<SetInfoTypeAction.InfoType> m_ToolStateSelector;

        List<IDisposable> m_Disposables = new List<IDisposable>();

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();
            m_Disposables.Add(UISelectorFactory.createSelector<float>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.temperature), newData => { m_TemperatureControl.SetValue(newData); }));
            m_Disposables.Add(m_SkyboxDatSelector = UISelectorFactory.createSelector<SkyboxData>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.skyboxData), OnSkyboxDataChanged));
            m_Disposables.Add(m_WeatherTypeSelector = UISelectorFactory.createSelector<WeatherType>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.weatherType), OnWeatherTypeChanged));
            m_Disposables.Add(m_VREnableSelector = UISelectorFactory.createSelector<bool>(VRContext.current, nameof(VRStateData.VREnable)));
            m_Disposables.Add(UISelectorFactory.createSelector<IButtonVisibility>(AppBarContext.current, nameof(IAppBarDataProvider.buttonVisibility), OnButtonVisibilityChanged));
            m_Disposables.Add(UISelectorFactory.createSelector<IButtonInteractable>(AppBarContext.current, nameof(IAppBarDataProvider.buttonInteractable), OnButtonInteractableChanged));

            m_Disposables.Add(m_TouchOrbitTypeSelector = UISelectorFactory.createSelector<SetOrbitTypeAction.OrbitType>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.touchOrbitType), OnTouchOrbitTypeChanged));

            m_ButtonVisibility = true;
            m_Disposables.Add(m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeSubDialog), OnActiveSubDialogChanged));
            m_Disposables.Add(m_ToolStateSelector = UISelectorFactory.createSelector<SetInfoTypeAction.InfoType>(ToolStateContext.current, nameof(IToolStateDataProvider.infoType), OnToolStateChange));
        }

        void OnToolStateChange(SetInfoTypeAction.InfoType newData)
        {
            m_DebugOptionMenu.SetActive(newData == SetInfoTypeAction.InfoType.Debug);
        }

        void Start()
        {
            m_DialogButton.buttonClicked += OnDialogButtonClicked;
            m_DialogButton.buttonLongPressed += OnDialogButtonLongPressed;

            m_TextureToggle.onValueChanged.AddListener(OnTextureToggleChanged);
            m_LightDataToggle.onValueChanged.AddListener(OnLightDataToggleChanged);
            m_SkyboxDropdown.onValueChanged.AddListener(OnSkyboxChanged);
            m_SimulationToggle.onValueChanged.AddListener(OnSimulationToggleChanged);
            m_WeatherDropdown.onValueChanged.AddListener(OnWeatherChanged);
            m_StatsInfoToggle.onValueChanged.AddListener(OnStatsInfoToggleChanged);
            m_FilterHLODsToggle.onValueChanged.AddListener(OnFilterHLODsToggleChanged);
            m_DebugOptionToggle.onValueChanged.AddListener(OnDebugOptionToggleChanged);
            m_TemperatureControl.onFloatValueChanged.AddListener(OnTemperatureControlChanged);
            m_OrbitDropdown.onValueChanged.AddListener(OnOrbitDropdownChanged);

            m_Disposables.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableLightData), newData => { m_LightDataToggle.on = newData; }));
            m_Disposables.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableTexture), newData => { m_TextureToggle.on = newData; }));
            m_Disposables.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableStatsInfo), newData => { m_StatsInfoToggle.on = newData; }));
            m_Disposables.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.filterHlods), newData => { m_FilterHLODsToggle.on = newData; }));
            m_Disposables.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableClimateSimulation), newData => { m_SimulationToggle.on = newData; }));
        }

        void OnDestroy()
        {
            m_Disposables.ForEach(x => x.Dispose());
        }

        void OnOrbitDropdownChanged(int value)
        {
            if (value == 0)
            {
                // Pivot
                Dispatcher.Dispatch(SetSceneOptionAction.From(new { touchOrbitType = SetOrbitTypeAction.OrbitType.WorldOrbit }));
            }
            else
            {
                // Orbit
                Dispatcher.Dispatch(SetSceneOptionAction.From(new { touchOrbitType = SetOrbitTypeAction.OrbitType.OrbitAtPoint }));
            }
        }

        void OnSkyboxDataChanged(SkyboxData newData)
        {
            if (newData.skyboxType == SkyboxType.Light)
                m_SkyboxDropdown.SetValueWithoutNotify(0);
            else if (newData.skyboxType == SkyboxType.Dark)
                m_SkyboxDropdown.SetValueWithoutNotify(1);
            else
                m_SkyboxDropdown.SetValueWithoutNotify(2);
        }

        void OnWeatherTypeChanged(WeatherType newData)
        {
            if (newData == WeatherType.HeavyRain)
                m_WeatherDropdown.SetValueWithoutNotify(0);
            else if (newData == WeatherType.Sunny)
                m_WeatherDropdown.SetValueWithoutNotify(1);
        }

        void OnActiveSubDialogChanged(OpenDialogAction.DialogType data)
        {
            if (m_ToolStateSelector != null)
            {
                m_DialogButton.transform.parent.gameObject.SetActive(m_ButtonVisibility && data != OpenDialogAction.DialogType.LandingScreen);

                m_DialogButton.selected = data == OpenDialogAction.DialogType.SceneOptions;
                m_DebugOptionMenu?.SetActive(m_ToolStateSelector.GetValue() == SetInfoTypeAction.InfoType.Debug);
            }
        }

        void OnTextureToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetEnableTextureAction.From(on));
        }

        void OnLightDataToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { enableLightData = on }));
        }

        void OnSkyboxChanged(int index)
        {
            var skybox = m_SkyboxDatSelector.GetValue();
            if (index == 0)
                skybox.skyboxType = SkyboxType.Light;
            else if (index == 1)
                skybox.skyboxType = SkyboxType.Dark;
            else
            {
                skybox.skyboxType = SkyboxType.Custom;
                skybox.customColor = Color.green;
            }

            Dispatcher.Dispatch(SetSceneOptionAction.From(new { skyboxData = skybox }));
        }

        void OnSimulationToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { enableClimateSimulation = on }));
        }

        void OnWeatherChanged(int index)
        {
            var weather = m_WeatherTypeSelector.GetValue();
            if (index == 0)
                weather = WeatherType.HeavyRain;
            else if (index == 1)
                weather = WeatherType.Sunny;

            Dispatcher.Dispatch(SetSceneOptionAction.From(new { weatherType = weather }));
        }

        void OnTemperatureControlChanged(float value)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { temperature = value }));
        }

        void OnStatsInfoToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { enableStatsInfo = on }));
        }

        void OnFilterHLODsToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { filterHlods = on }));
        }

        void OnDebugOptionToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetSceneOptionAction.From(new { enableDebugOption = on }));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.SceneOptions;
            Dispatcher.Dispatch(OpenSubDialogAction.From(dialogType));

            if (dialogType == OpenDialogAction.DialogType.SceneOptions)
            {
                SetDialogPosition();
            }
        }

        void OnDialogButtonLongPressed()
        {
            Dispatcher.Dispatch(SetInfoTypeAction.From(SetInfoTypeAction.InfoType.Debug));

            if (!m_DialogWindow.open)
            {
                Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.SceneOptions));
                SetDialogPosition();
            }
        }

        void SetDialogPosition()
        {
            if (!m_VREnableSelector.GetValue())
            {
                var popupTransform = transform;
                Vector3 popupPosition = popupTransform.position;
                popupPosition.x = m_DialogButton.transform.position.x;
                popupTransform.position = popupPosition;
            }
        }

        void OnButtonVisibilityChanged(IButtonVisibility data)
        {
            if (data?.type == (int)ButtonType.Settings)
            {
                m_ButtonVisibility = data.visible;
                m_DialogButton.transform.parent.gameObject.SetActive(m_ButtonVisibility && m_ActiveDialogSelector.GetValue() != OpenDialogAction.DialogType.LandingScreen);
            }
        }

        void OnButtonInteractableChanged(IButtonInteractable data)
        {
            if (data?.type == (int)ButtonType.Settings)
            {
                m_DialogButton.button.interactable = data.interactable;
            }
        }


        void OnTouchOrbitTypeChanged(SetOrbitTypeAction.OrbitType orbitType)
        {
            switch (orbitType)
            {
                case SetOrbitTypeAction.OrbitType.None:
                    break;
                case SetOrbitTypeAction.OrbitType.WorldOrbit:
                    m_OrbitDropdown.SetValueWithoutNotify(0);
                    break;
                case SetOrbitTypeAction.OrbitType.OrbitAtSelection:
                    break;
                case SetOrbitTypeAction.OrbitType.OrbitAtPoint:
                    m_OrbitDropdown.SetValueWithoutNotify(1);
                    break;
            }
        }
    }
}
