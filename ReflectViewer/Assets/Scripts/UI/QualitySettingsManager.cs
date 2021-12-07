using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{
    public class QualitySettingsManager: MonoBehaviour
    {
        public const float qualityChangeWaitInterval = 5.0f;
        [SerializeField]
        FrameCalculator m_FrameCalculator;

        int m_MaxQualityLevel;

        IQualitySettingsDataProvider m_QualitySettingsDataProvider;
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        public void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(ApplicationSettingsContext.current, nameof(IApplicationSettingsDataProvider<QualityState>.qualityStateData) + "." + nameof(IQualitySettingsDataProvider.qualityLevel),
             (qualityLevel) =>
             {
                 QualitySettings.SetQualityLevel(qualityLevel);
             }));

            var qualitySettingsSelector = UISelectorFactory.createSelector<IQualitySettingsDataProvider>(ApplicationSettingsContext.current, nameof(IApplicationSettingsDataProvider<QualityState>.qualityStateData),
                provider =>
                {
                    m_QualitySettingsDataProvider = provider;
                } );
            m_DisposeOnDestroy.Add(qualitySettingsSelector);
            m_QualitySettingsDataProvider = qualitySettingsSelector.GetValue();
        }

        public void Start()
        {
            m_MaxQualityLevel = QualitySettings.names.Length - 1;

            if (m_FrameCalculator == null)
                m_FrameCalculator = FindObjectOfType<FrameCalculator>();

            m_FrameCalculator.fpsChanged += OnFpsChanged;
        }

        void OnFpsChanged(float fps)
        {
            if (m_QualitySettingsDataProvider.isAutomatic && Time.unscaledTime > m_QualitySettingsDataProvider.lastQualityChangeTimestamp + qualityChangeWaitInterval)
            {
                if (fps < m_QualitySettingsDataProvider.fpsThresholdQualityDecrease)
                    ChangeQuality(-1);
                else if (fps > m_QualitySettingsDataProvider.fpsThresholdQualityIncrease)
                    ChangeQuality(+1);

                m_QualitySettingsDataProvider.lastQualityChangeTimestamp = Time.unscaledTime;
            }
        }

        void ChangeQuality(int modifier)
        {
            var newQuality = Mathf.Clamp(m_QualitySettingsDataProvider.qualityLevel + modifier, 0, m_MaxQualityLevel);

            if (newQuality == m_QualitySettingsDataProvider.qualityLevel)
                return;

            SetQualitySettingsAction.SetQualitySettingsData settingsData = new SetQualitySettingsAction.SetQualitySettingsData();
            settingsData.isAutomatic = m_QualitySettingsDataProvider.isAutomatic;
            settingsData.fpsThresholdQualityDecrease = m_QualitySettingsDataProvider.fpsThresholdQualityDecrease;
            settingsData.fpsThresholdQualityIncrease = m_QualitySettingsDataProvider.fpsThresholdQualityIncrease;
            settingsData.lastQualityChangeTimestamp = Time.unscaledTime;
            settingsData.qualityLevel = newQuality;

            Dispatcher.Dispatch(SetQualitySettingsAction.From(settingsData));
        }
    }

}
