using SharpFlux;
using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer
{
    public class QualitySettingsManager : MonoBehaviour
    {
        public const float qualityChangeWaitInterval = 5.0f;
        [SerializeField]
        FrameCalculator m_FrameCalculator;

        QualityState m_CachedQualityStateData;
        int m_MaxQualityLevel;

        public void Start()
        {
            m_CachedQualityStateData = UIStateManager.current.applicationStateData.qualityStateData;
            m_MaxQualityLevel = QualitySettings.names.Length - 1;

            if (m_FrameCalculator == null)
                m_FrameCalculator = FindObjectOfType<FrameCalculator>();

            m_FrameCalculator.fpsChanged += OnFpsChanged;

            UIStateManager.applicationStateChanged += OnApplicationStateChanged;
        }

        void OnApplicationStateChanged(ApplicationStateData data)
        {
            if (data.qualityStateData == m_CachedQualityStateData)
                return;

            if (m_CachedQualityStateData.qualityLevel != data.qualityStateData.qualityLevel)
                QualitySettings.SetQualityLevel(data.qualityStateData.qualityLevel);

            m_CachedQualityStateData = data.qualityStateData;
        }

        void OnFpsChanged(float fps)
        {
            if (m_CachedQualityStateData.isAutomatic && Time.unscaledTime > m_CachedQualityStateData.lastQualityChangeTimestamp + qualityChangeWaitInterval)
            {
                if (fps < m_CachedQualityStateData.fpsThresholdQualityDecrease)
                    ChangeQuality(-1);
                else if (fps > m_CachedQualityStateData.fpsThresholdQualityIncrease)
                    ChangeQuality(+1);

                m_CachedQualityStateData.lastQualityChangeTimestamp = Time.unscaledTime;
            }
        }

        void ChangeQuality(int modifier)
        {
            var qualityStateData = m_CachedQualityStateData;
            var newQuality = Mathf.Clamp(qualityStateData.qualityLevel + modifier, 0, m_MaxQualityLevel);

            if (newQuality == qualityStateData.qualityLevel)
                return;

            qualityStateData.lastQualityChangeTimestamp = Time.unscaledTime;
            qualityStateData.qualityLevel = newQuality;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetQuality,
                qualityStateData));
        }
    }

}
