using System;
using SharpFlux.Dispatching;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect
{
    public class FrameCalculator: MonoBehaviour
    {
        [SerializeField]
        ViewerReflectBootstrapper m_Reflect;

        [SerializeField]
        int m_FrameBufferCount = 30;

        TimeSpan[] m_FrameTimes;
        int m_CurrentIndex;

        TimeSpan m_AvgFrameTime;
        TimeSpan m_MinFrameTime;
        TimeSpan m_MaxFrameTime;
        bool m_DispatchEnabled;

        public event Action<float> fpsChanged;
        IDisposable m_SelectorToDispose;

        void Start()
        {
            m_SelectorToDispose = UISelectorFactory.createSelector<bool>(SceneOptionContext.current, nameof(ISceneOptionData<SkyboxData>.enableStatsInfo), OnEnableStatsInfoChanged);

            m_FrameTimes = new TimeSpan[m_FrameBufferCount];
            for (var i = 0; i < m_FrameTimes.Length; ++i)
                m_FrameTimes[i] = TimeSpan.FromMilliseconds(-1);
        }

        void OnDestroy()
        {
            m_SelectorToDispose?.Dispose();
        }

        void OnEnableStatsInfoChanged(bool on)
        {
            m_DispatchEnabled = on;
        }

        void Update()
        {
            var clock = m_Reflect.Hook.Helpers.Clock;
            m_FrameTimes[m_CurrentIndex] = clock.deltaTime;
            m_CurrentIndex = ++m_CurrentIndex % m_FrameTimes.Length;

            Calculate();
            if (m_DispatchEnabled)
                DispatchStatsInfoData();
        }

        void Calculate()
        {
            var m_CurrentValidFrameCount = 0;
            var totalFrameTime = TimeSpan.Zero;
            m_MinFrameTime = TimeSpan.MaxValue;
            m_MaxFrameTime = TimeSpan.MinValue;

            for (var i = 0; i < m_FrameTimes.Length; ++i)
            {
                var value = m_FrameTimes[i];
                if (value <= TimeSpan.Zero)
                    continue;

                ++m_CurrentValidFrameCount;
                totalFrameTime += value;

                if (value < m_MinFrameTime)
                    m_MinFrameTime = value;
                if (value > m_MaxFrameTime)
                    m_MaxFrameTime = value;
            }

            if (m_CurrentValidFrameCount > 0)
            {
                m_AvgFrameTime = TimeSpan.FromMilliseconds(totalFrameTime.TotalMilliseconds / m_CurrentValidFrameCount);
                fpsChanged?.Invoke(1.0f / (float)m_AvgFrameTime.TotalSeconds);
            }
        }

        void DispatchStatsInfoData()
        {
            var avgFps = 1.0 / m_AvgFrameTime.TotalSeconds;
            var maxFps = 1.0 / m_MinFrameTime.TotalSeconds;
            var minFps = 1.0 / m_MaxFrameTime.TotalSeconds;

            var statsInfoData = new SetStatsInfoAction.SetStatsInfoData();
            statsInfoData.fpsAvg = Mathf.Clamp((int)avgFps, 0, 99);
            statsInfoData.fpsMax = Mathf.Clamp((int)maxFps, 0, 99);
            statsInfoData.fpsMin = Mathf.Clamp((int)minFps, 0, 99);

            Dispatcher.Dispatch(SetStatsInfoAction.From(statsInfoData));
        }
    }
}
