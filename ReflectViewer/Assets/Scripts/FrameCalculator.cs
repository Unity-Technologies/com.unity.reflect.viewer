using SharpFlux;
using System;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.Events;

namespace UnityEngine.Reflect
{
    public class FrameCalculator : MonoBehaviour
    {
        public int FrameBufferCount = 30;

        float[] m_FrameCounts;
        int m_CurrentIndex;
        int m_CurrentValidFrameCount;
        float m_TotalFrameRate;

        float m_CurrentFrameRate;
        float m_MinFrameRate;
        float m_MaxFrameRate;
        bool m_DispatchEnabled;

        public event Action<float> fpsChanged;

        void Start()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_FrameCounts = new float[FrameBufferCount];
            for (int i = 0; i < m_FrameCounts.Length; ++i)
            {
                m_FrameCounts[i] = -1;
            }
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DispatchEnabled = data.activeDialog == DialogType.StatsInfo;
        }

        void Update()
        {
            m_FrameCounts[m_CurrentIndex] = 1f / Time.deltaTime;
            ++m_CurrentIndex;
            m_CurrentIndex %= m_FrameCounts.Length;

            Calculate();
            if(m_DispatchEnabled)
                DispatchStatsInfoData();
        }

        void Calculate()
        {
            m_CurrentValidFrameCount = 0;
            m_TotalFrameRate = 0;
            m_MinFrameRate = float.MaxValue;
            m_MaxFrameRate = float.MinValue;
            for (int i = 0; i < m_FrameCounts.Length; ++i)
            {
                var value = m_FrameCounts[i];
                if (value <= 0)
                    continue;

                ++m_CurrentValidFrameCount;
                m_TotalFrameRate += value;

                if (m_MinFrameRate > value) m_MinFrameRate = value;
                if (m_MaxFrameRate < value) m_MaxFrameRate = value;
            }

            if (m_CurrentValidFrameCount > 0)
            {
                m_CurrentFrameRate = m_TotalFrameRate / m_CurrentValidFrameCount;
                fpsChanged?.Invoke(m_CurrentFrameRate);
            }
        }

        void DispatchStatsInfoData()
        {
            var statsInfoData = UIStateManager.current.debugStateData.statsInfoData;
            statsInfoData.fpsAvg = Mathf.Clamp((int) m_CurrentFrameRate, 0, 99);
            statsInfoData.fpsMax = Mathf.Clamp((int) m_MaxFrameRate, 0, 99);
            statsInfoData.fpsMin = Mathf.Clamp((int) m_MinFrameRate, 0, 99);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatsInfo,
                statsInfoData));
        }
    }
}
