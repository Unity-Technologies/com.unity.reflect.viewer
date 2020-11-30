using SharpFlux;
using System;
using System.Collections;
using System.Collections.Generic;
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

        public void Start()
        {
            m_CachedQualityStateData = UIStateManager.current.applicationStateData.qualityStateData;

            if (m_FrameCalculator == null)
                m_FrameCalculator = FindObjectOfType<FrameCalculator>();

            m_FrameCalculator.fpsChanged += OnFpsChanged;

            UIStateManager.applicationStateChanged += OnApplicationStateChanged;
        }

        void OnApplicationStateChanged(ApplicationStateData data)
        {
            if(data.qualityStateData != m_CachedQualityStateData)
            {
                m_CachedQualityStateData = data.qualityStateData;
            }
        }

        void OnFpsChanged(float fps)
        {
            if (Time.unscaledTime > m_CachedQualityStateData.lastQualityChangeTimestamp + qualityChangeWaitInterval)
            {
                if (fps < m_CachedQualityStateData.fpsThresholdQualityDecrease)
                    ChangeQuality(-1);
                else if (fps > m_CachedQualityStateData.fpsThresholdQualityIncrease)
                    ChangeQuality(+1);
            }
        }

        void ChangeQuality(int modifier)
        {
            var oldQuality = QualitySettings.GetQualityLevel();
            QualitySettings.SetQualityLevel(oldQuality + modifier);
            var newQuality = QualitySettings.GetQualityLevel();

            if(newQuality != oldQuality)
            {
                m_CachedQualityStateData.lastQualityChangeTimestamp = Time.unscaledTime;
                m_CachedQualityStateData.qualityLevel = newQuality;
            }
        }
    }

}
