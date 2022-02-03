using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux.Dispatching;
using TMPro;
using Unity.Reflect.Actors;
using Unity.Reflect.Collections;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

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
        MinMaxPropertyControl m_AnglePrioritySlider;
        [SerializeField]
        MinMaxPropertyControl m_DistancePrioritySlider;
        [SerializeField]
        MinMaxPropertyControl m_SizePrioritySlider;
        [SerializeField]
        SlideToggle m_UseSpatialManifestToggle;
        [SerializeField]
        SlideToggle m_UseHlodsToggle;
        [SerializeField]
        TMP_Dropdown m_HlodDelayModeDropdown;
        [SerializeField]
        TMP_Dropdown m_HlodPrioritizerDropdown;
        [SerializeField]
        MinMaxPropertyControl m_TargetFpsSlider;
        [SerializeField]
        SlideToggle m_ShowActorDebugToggle;

        [SerializeField]
        GameObject m_InfoButtonGameObject;
#pragma warning restore 649

        DialogWindow m_DialogWindow;
        string[] m_QualitySettingsNames;

        IUISelector<Vector3> m_SpatialPriorityWeightsSelector;
        IUISelector<IQualitySettingsDataProvider> m_QualitySettingsSelector;

        List<IDisposable> m_DisposableSelectors = new List<IDisposable>();

        void Awake()
        {
            // get existing quality levels instead of hard-coding them
            m_QualitySettingsNames = QualitySettings.names;
            m_QualityDropdown.AddOptions(m_QualitySettingsNames.ToList());

            m_DisposableSelectors.Add(m_QualitySettingsSelector = UISelectorFactory.createSelector<IQualitySettingsDataProvider>(ApplicationSettingsContext.current,nameof(IApplicationSettingsDataProvider<QualityState>.qualityStateData),
                (qualitySettings) =>
                {
                    m_QualitySettingValue.text = qualitySettings.isAutomatic ?
                    string.Format(AUTO_QUALITY_FORMAT, m_QualitySettingsNames[qualitySettings.qualityLevel]) :
                    m_QualitySettingsNames[qualitySettings.qualityLevel];
                }));

            m_DialogWindow = GetComponent<DialogWindow>();

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableDebugOption), OnEnableDebugOptionChanged));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.gesturesTrackingEnabled), (enable) =>
            {
                m_GesturesTrackingToggle.on = enable;
                m_InfoButtonGameObject.SetActive(enable);
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.ARAxisTrackingEnabled), (enable) =>
            {
                m_ARAxisTrackingToggle.on = enable;
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useDebugBoundingBoxMaterials), (enable) =>
            {
                m_DebugBoundingBoxMaterialToggle.on = enable;
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useCulling), (enable) =>
            {
                m_CullingToggle.on = enable;
            }));

            m_DisposableSelectors.Add(m_SpatialPriorityWeightsSelector = UISelectorFactory.createSelector<Vector3>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.spatialPriorityWeights), (weights) =>
            {
                m_AnglePrioritySlider.SetValue(weights.x);
                m_DistancePrioritySlider.SetValue(weights.y);
                m_SizePrioritySlider.SetValue(weights.z);
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useSpatialManifest), (enable) =>
            {
                m_UseSpatialManifestToggle.on = enable;
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.useHlods), (enable) =>
            {
                m_UseHlodsToggle.on = enable;
            }));

            var hlodModes = Enum.GetNames(typeof(HlodMode)).ToList();
            m_HlodDelayModeDropdown.AddOptions(hlodModes);
            m_DisposableSelectors.Add(UISelectorFactory.createSelector<int>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.hlodDelayMode), (mode) =>
            {
                m_HlodDelayModeDropdown.SetValueWithoutNotify(mode);
            }));

            var prioritizers = Enum.GetNames(typeof(SyncTreeActor.Prioritizer)).ToList();
            m_HlodPrioritizerDropdown.AddOptions(prioritizers);
            m_DisposableSelectors.Add(UISelectorFactory.createSelector<int>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.hlodPrioritizer), (mode) =>
            {
                m_HlodPrioritizerDropdown.SetValueWithoutNotify(mode);
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<int>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.targetFps), (count) =>
            {
                m_TargetFpsSlider.SetValue(count);
            }));

            m_DisposableSelectors.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.showActorDebug), (enable) =>
            {
                m_ShowActorDebugToggle.on = enable;
            }));
        }

        void OnDestroy()
        {
            foreach(var disposable in m_DisposableSelectors)
            {
                disposable.Dispose();
            }
            m_DisposableSelectors.Clear();
        }

        void OnQualityDropdownChanged(int value)
        {
            var qualityStateData = m_QualitySettingsSelector.GetValue();
            SetQualitySettingsAction.SetQualitySettingsData settingsData = new SetQualitySettingsAction.SetQualitySettingsData();
            settingsData.fpsThresholdQualityDecrease = qualityStateData.fpsThresholdQualityDecrease;
            settingsData.fpsThresholdQualityIncrease = qualityStateData.fpsThresholdQualityIncrease;
            settingsData.lastQualityChangeTimestamp = qualityStateData.lastQualityChangeTimestamp;

            if (value == 0)
            {
                // keep the current quality setting when swapping to automatic
                settingsData.isAutomatic = true;
                settingsData.qualityLevel = qualityStateData.qualityLevel;
            }
            else
            {
                settingsData.isAutomatic = false;
                // decrement value because Automatic option is at index 0
                settingsData.qualityLevel = value - 1;
            }

            Dispatcher.Dispatch(SetQualitySettingsAction.From(settingsData));
        }

        void Start()
        {
            m_GesturesTrackingToggle.onValueChanged.AddListener(OnGesturesTrackingToggleChanged);
            m_ARAxisTrackingToggle.onValueChanged.AddListener(OnARAxisTrackingToggleChanged);

            m_DebugBoundingBoxMaterialToggle.onValueChanged.AddListener(OnDebugBoundingBoxMaterialToggleChanged);
            m_CullingToggle.onValueChanged.AddListener(OnCullingToggleChanged);

            m_AnglePrioritySlider.onFloatValueChanged.AddListener(SetAnglePriority);
            m_DistancePrioritySlider.onFloatValueChanged.AddListener(SetDistancePriority);
            m_SizePrioritySlider.onFloatValueChanged.AddListener(SetSizePriority);

            m_QualityDropdown.onValueChanged.AddListener(OnQualityDropdownChanged);

            m_UseSpatialManifestToggle.onValueChanged.AddListener(OnSpatialManifestToggleChanged);
            m_UseHlodsToggle.onValueChanged.AddListener(OnHlodsToggleChanged);
            m_HlodDelayModeDropdown.onValueChanged.AddListener(SetHlodDelayMode);
            m_HlodPrioritizerDropdown.onValueChanged.AddListener(SetHlodPrioritizer);
            m_TargetFpsSlider.onIntValueChanged.AddListener(SetTargetFps);
            m_ShowActorDebugToggle.onValueChanged.AddListener(OnShowActorDebugToggleChanged);
        }

        void OnGesturesTrackingToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {gesturesTrackingEnabled = on}));
        }

        void OnARAxisTrackingToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {ARAxisTrackingEnabled = on}));
        }

        void SetAnglePriority(float value)
        {
            var weights = m_SpatialPriorityWeightsSelector.GetValue();
            weights.x = value;
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {spatialPriorityWeights = weights}));
        }

        void SetDistancePriority(float value)
        {
            var weights = m_SpatialPriorityWeightsSelector.GetValue();
            weights.y = value;
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {spatialPriorityWeights = weights}));
        }

        void SetSizePriority(float value)
        {
            var weights = m_SpatialPriorityWeightsSelector.GetValue();
            weights.z = value;
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {spatialPriorityWeights = weights}));
        }

        void OnDebugBoundingBoxMaterialToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {useDebugBoundingBoxMaterials = on}));
        }

        void OnCullingToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {useCulling = on}));
        }

        void OnSpatialManifestToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {useSpatialManifest = on}));
        }

        void OnHlodsToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {useHlods = on}));
        }

        void SetHlodDelayMode(int value)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {hlodDelayMode = value}));
        }

        void SetHlodPrioritizer(int value)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {hlodPrioritizer = value}));
        }

        void SetTargetFps(int value)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {targetFps = value}));
        }

        void OnShowActorDebugToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new {showActorDebug = on}));
        }

        void OnEnableDebugOptionChanged(bool on)
        {
            if (on)
                m_DialogWindow.Open();
            else
                m_DialogWindow.Close();
        }
    }
}
