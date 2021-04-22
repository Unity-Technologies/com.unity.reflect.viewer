using System.Linq;
using SharpFlux;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class DebugOptionsUIController : MonoBehaviour
    {
        const string AUTO_QUALITY_FORMAT = "{0} (auto)";

#pragma warning disable 649
        [SerializeField]
        SlideToggle m_GesturesTrackingToggle;
        [SerializeField]
        SlideToggle m_ARAxisTrackingToggle;
        [SerializeField]
        TextMeshProUGUI m_QualitySettingValue;
        [SerializeField]
        TMP_Dropdown m_QualityDropdown;
        [SerializeField]
        SlideToggle m_DebugBoundingBoxMaterialToggle;
        [SerializeField]
        SlideToggle m_CullingToggle;
        [SerializeField]
        SlideToggle m_StaticBatchingToggle;
        [SerializeField]
        MinMaxPropertyControl m_AnglePrioritySlider;
        [SerializeField]
        MinMaxPropertyControl m_DistancePrioritySlider;
        [SerializeField]
        MinMaxPropertyControl m_SizePrioritySlider;
#pragma warning restore 649

        DialogWindow m_DialogWindow;
        DebugOptionsData? m_CurrentsDebugOptionsData;
        string[] m_QualitySettingsNames;

        void Awake()
        {
            // get existing quality levels instead of hard-coding them
            m_QualitySettingsNames = QualitySettings.names;
            m_QualityDropdown.AddOptions(m_QualitySettingsNames.ToList());
            OnQualityChanged(UIStateManager.current.applicationStateData);
            UIStateManager.debugStateChanged += OnDebugStateChanged;
            UIStateManager.applicationStateChanged += OnQualityChanged;
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void OnQualityChanged(ApplicationStateData obj)
        {
            m_QualitySettingValue.text = obj.qualityStateData.isAutomatic ?
                string.Format(AUTO_QUALITY_FORMAT, m_QualitySettingsNames[obj.qualityStateData.qualityLevel]) :
                m_QualitySettingsNames[obj.qualityStateData.qualityLevel];
        }

        void OnQualityDropdownChanged(int value)
        {
            var data = UIStateManager.current.applicationStateData.qualityStateData;
            if (value == 0)
            {
                // keep the current quality setting when swapping to automatic
                data.isAutomatic = true;
            }
            else
            {
                data.isAutomatic = false;
                // decrement value because Automatic option is at index 0
                data.qualityLevel = value - 1;
            }
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetQuality, data));
        }

        void Start()
        {
            m_GesturesTrackingToggle.onValueChanged.AddListener(OnGesturesTrackingToggleChanged);
            m_ARAxisTrackingToggle.onValueChanged.AddListener(OnARAxisTrackingToggleChanged);

            m_DebugBoundingBoxMaterialToggle.onValueChanged.AddListener(OnDebugBoundingBoxMaterialToggleChanged);
            m_CullingToggle.onValueChanged.AddListener(OnCullingToggleChanged);
            m_StaticBatchingToggle.onValueChanged.AddListener(OnStaticBatchingToggleChanged);

            m_AnglePrioritySlider.onFloatValueChanged.AddListener(SetAnglePriority);
            m_DistancePrioritySlider.onFloatValueChanged.AddListener(SetDistancePriority);
            m_SizePrioritySlider.onFloatValueChanged.AddListener(SetSizePriority);

            m_QualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);
        }

        void OnDebugStateChanged(UIDebugStateData data)
        {
            if (m_CurrentsDebugOptionsData == data.debugOptionsData)
                return;

            m_GesturesTrackingToggle.on = data.debugOptionsData.gesturesTrackingEnabled;
            m_ARAxisTrackingToggle.on = data.debugOptionsData.ARAxisTrackingEnabled;

            m_DebugBoundingBoxMaterialToggle.on = data.debugOptionsData.useDebugBoundingBoxMaterials;
            m_CullingToggle.on = data.debugOptionsData.useCulling;
            m_StaticBatchingToggle.on = data.debugOptionsData.useStaticBatching;

            m_AnglePrioritySlider.SetValue(data.debugOptionsData.spatialPriorityWeights.x);
            m_DistancePrioritySlider.SetValue(data.debugOptionsData.spatialPriorityWeights.y);
            m_SizePrioritySlider.SetValue(data.debugOptionsData.spatialPriorityWeights.z);

            m_CurrentsDebugOptionsData = data.debugOptionsData;
        }

        void OnGesturesTrackingToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.gesturesTrackingEnabled = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
        }

        void OnARAxisTrackingToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.ARAxisTrackingEnabled = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
        }

        void SetAnglePriority(float value)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.spatialPriorityWeights.x = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSpatialPriorityWeights, data.spatialPriorityWeights));
        }

        void SetDistancePriority(float value)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.spatialPriorityWeights.y = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSpatialPriorityWeights, data.spatialPriorityWeights));
        }

        void SetSizePriority(float value)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.spatialPriorityWeights.z = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSpatialPriorityWeights, data.spatialPriorityWeights));
        }

        void OnDebugBoundingBoxMaterialToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.useDebugBoundingBoxMaterials = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugBoundingBoxMaterials, data.useDebugBoundingBoxMaterials));
        }

        void OnCullingToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.useCulling = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetCulling, data.useCulling));
        }

        void OnStaticBatchingToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.useStaticBatching = on;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStaticBatching, data.useStaticBatching));
        }
    }
}
