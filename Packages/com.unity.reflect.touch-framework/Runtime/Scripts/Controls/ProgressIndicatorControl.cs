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
        RectTransform m_LeftCorner;

        [SerializeField]
        RectTransform  m_RightCorner;

        [SerializeField]
        float m_LoopingDuration = 2.0f;

#pragma warning restore CS0649

        float m_CurrentProgress = 0;
        FloatTween m_ProgressTween;
        FloatTween m_LoopingTween;
        TweenRunner<FloatTween> m_TweenRunner;

        bool m_Initialized = false;

        void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (m_Initialized)
                return;

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

            UpdateCorners();
            m_TweenRunner = new TweenRunner<FloatTween>();
            m_TweenRunner.Init(this);

            m_Initialized = true;
        }

        void OnLoopingTweenChanged(float rotation)
        {
            m_ProgressBgImage.transform.localRotation = Quaternion.Euler(0, 0, rotation);
            UpdateCorners();
        }

        void OnProgressTweenChanged(float progress)
        {
            m_CurrentProgress = progress;
            m_ProgressImage.fillAmount = progress;
            UpdateCorners();
        }

        public void StopLooping()
        {
            m_TweenRunner.StopTween();
        }

        public void StartLooping()
        {
            m_ProgressImage.fillAmount = 0.25f;
            m_TweenRunner.StartTween(m_LoopingTween, EaseType.Linear, true, TweenLoopType.Loop);
        }

        public void SetProgress(float progress, bool instant = false)
        {
            m_ProgressBgImage.transform.localRotation = Quaternion.identity;
            if (instant)
            {
                m_CurrentProgress = m_ProgressImage.fillAmount = progress;
                UpdateCorners();
            }
            else
            {
                m_ProgressTween.startValue = m_CurrentProgress;
                m_ProgressTween.targetValue = progress;
                m_TweenRunner.StartTween(m_ProgressTween);
            }
        }

        void UpdateCorners()
        {
            var fillVector = Vector3.zero;
            float leftCornerAngle = 180.0f + 90.0f * m_ProgressImage.fillOrigin;
            switch (m_ProgressImage.fillOrigin)
            {
                //bottom
                case 0:
                    fillVector = Vector3.down;
                    break;
                //right
                case 1:
                    fillVector = Vector3.right;
                    break;
                //top
                case 2:
                    fillVector = Vector3.up;
                    break;
                //left
                case 3:
                    fillVector = Vector3.left;
                    break;
            }

            var cornerSize = m_LeftCorner.rect.size;
            var barSize = m_ProgressImage.rectTransform.rect.size;
            var barRotation = m_ProgressImage.rectTransform.localRotation;

            m_LeftCorner.localPosition = barRotation * fillVector * 0.5f * (barSize.x - cornerSize.x);
            m_LeftCorner.localRotation = barRotation * Quaternion.Euler(0.0f, 0.0f, leftCornerAngle);

            var fillRotation = Quaternion.AngleAxis(m_ProgressImage.fillAmount * 360.0f, Vector3.forward);
            var leftToRght = m_ProgressImage.fillClockwise ? Quaternion.Inverse(fillRotation) : fillRotation;

            m_RightCorner.localPosition = leftToRght * m_LeftCorner.localPosition;
            m_RightCorner.localRotation = leftToRght * m_LeftCorner.localRotation;

        }
    }
}
