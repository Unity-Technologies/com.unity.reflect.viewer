using System;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.TouchFramework
{
    public class ProgressIndicatorControl : MonoBehaviour
    {

#pragma warning disable CS0649
        [SerializeField]
        GameObject m_ProgressBgImage;

        [SerializeField]
        Image m_ProgressImage;

        [SerializeField]
        float m_LoopingDuration = 2.0f;

#pragma warning restore CS0649

        float m_CurrentProgress = 0;
        FloatTween m_ProgressTween;
        FloatTween m_LoopingTween;
        TweenRunner<FloatTween> m_TweenRunner;

        void Awake()
        {
            m_CurrentProgress = 0;
            m_ProgressTween = new FloatTween
            {
                duration = 0.1f,
                ignoreTimeScale = true,
            };
            m_ProgressTween.AddOnChangedCallback(OnProgressTweenChanged);

            m_LoopingTween = new FloatTween
            {
                duration = m_LoopingDuration,
                ignoreTimeScale = true,
                startValue = 0,
                targetValue = -360,
            };
            m_LoopingTween.AddOnChangedCallback(OnLoopingTweenChanged);


            m_TweenRunner = new TweenRunner<FloatTween>();
            m_TweenRunner.Init(this);
        }

        void OnLoopingTweenChanged(float rotation)
        {
            m_ProgressBgImage.transform.localRotation = Quaternion.Euler(0, 0, rotation);
        }

        void OnProgressTweenChanged(float progress)
        {
            m_CurrentProgress = progress;
            m_ProgressImage.fillAmount = progress;
        }

        public void StopLooping()
        {
            m_TweenRunner.StopTween();
        }

        public void StartLooping()
        {
            m_ProgressImage.fillAmount = 0.75f;
            m_TweenRunner.StartTween(m_LoopingTween, EaseType.Linear, true, TweenLoopType.Loop);
        }

        public void SetProgress(float progress, bool instant = false)
        {
            m_ProgressBgImage.transform.localRotation = Quaternion.identity;
            if (instant)
            {
                m_CurrentProgress = m_ProgressImage.fillAmount = progress;
            }
            else
            {
                m_ProgressTween.startValue = m_CurrentProgress;
                m_ProgressTween.targetValue = progress;
                m_TweenRunner.StartTween(m_ProgressTween);
            }
        }
    }
}
