using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class SceneOptionsUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        Button m_DialogButton;
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
        Image m_DialogButtonImage;
        SceneOptionData m_CurrentsSceneOptionData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClicked);

            m_TextureToggle.onValueChanged.AddListener(OnTextureToggleChanged);
            m_LightDataToggle.onValueChanged.AddListener(OnLightDataToggleChanged);
            m_SkyboxDropdown.onValueChanged.AddListener(OnSkyboxChanged);
            m_SimulationToggle.onValueChanged.AddListener(OnSimulationToggleChanged);
            m_WeatherDropdown.onValueChanged.AddListener(OnWeatherChanged);

            m_TemperatureControl.onFloatValueChanged.AddListener(OnTemperatureControlChanged);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.SceneOptions;

            if (m_CurrentsSceneOptionData == data.sceneOptionData)
                return;

            if (m_CurrentsSceneOptionData.enableTexture != data.sceneOptionData.enableTexture)
                m_TextureToggle.on = data.sceneOptionData.enableTexture;

            if (m_CurrentsSceneOptionData.enableLightData != data.sceneOptionData.enableLightData)
                m_LightDataToggle.on = data.sceneOptionData.enableLightData;

            if (m_CurrentsSceneOptionData.skyboxData != data.sceneOptionData.skyboxData)
            {
                if (data.sceneOptionData.skyboxData.skyboxType == SkyboxType.Light)
                    m_SkyboxDropdown.SetValueWithoutNotify(0);
                else if (data.sceneOptionData.skyboxData.skyboxType == SkyboxType.Dark)
                    m_SkyboxDropdown.SetValueWithoutNotify(1);
                else
                    m_SkyboxDropdown.SetValueWithoutNotify(2);
            }


            if (m_CurrentsSceneOptionData.enableClimateSimulation != data.sceneOptionData.enableClimateSimulation)
                m_SimulationToggle.on = data.sceneOptionData.enableClimateSimulation;

            if (m_CurrentsSceneOptionData.weatherType != data.sceneOptionData.weatherType)
            {
                if (data.sceneOptionData.weatherType == WeatherType.HeavyRain)
                    m_WeatherDropdown.SetValueWithoutNotify(0);
                else if (data.sceneOptionData.weatherType == WeatherType.Sunny)
                    m_WeatherDropdown.SetValueWithoutNotify(1);
            }

            m_TemperatureControl.SetValue(data.sceneOptionData.temperature);

            m_CurrentsSceneOptionData = data.sceneOptionData;
        }

        void OnTextureToggleChanged(bool on)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            data.enableTexture = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetViewOption, data));
        }

        void OnLightDataToggleChanged(bool on)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            data.enableLightData = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetViewOption, data));
        }

        void OnSkyboxChanged(int index)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            if (index == 0)
                data.skyboxData.skyboxType = SkyboxType.Light;
            else if (index == 1)
                data.skyboxData.skyboxType = SkyboxType.Dark;
            else
            {
                data.skyboxData.skyboxType = SkyboxType.Custom;
                data.skyboxData.customColor = Color.green;
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSkybox, data));
        }

        void OnSimulationToggleChanged(bool on)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            data.enableClimateSimulation = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetClimateOption, data));
        }


        void OnWeatherChanged(int index)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            if (index == 0)
                data.weatherType = WeatherType.HeavyRain;
            else if (index == 1)
                data.weatherType = WeatherType.Sunny;

            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetClimateOption, data));
        }

        void OnTemperatureControlChanged(float value)
        {
            var data = UIStateManager.current.stateData.sceneOptionData;
            data.temperature = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetClimateOption, data));
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.SceneOptions;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
        }
    }
}
