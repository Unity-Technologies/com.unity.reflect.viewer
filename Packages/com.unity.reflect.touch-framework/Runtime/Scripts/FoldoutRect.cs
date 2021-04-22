using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Unity.TouchFramework
{

    /// <summary>
    /// Component for handling the folding and unfolding of a rect. Can be attached to DialogWindows to make a foldable dialog or controls to make foldable controls.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class FoldoutRect : MonoBehaviour
    {
        bool m_IsFolded;
        public bool isFolded => m_IsFolded;

        [SerializeField] Rect m_FoldedRect;
        [SerializeField] Rect m_UnfoldedRect;

        RectTween m_FoldTween;
        RectTween m_UnfoldTween;

        TweenRunner<RectTween> m_TweenRunner;

        RectTransform m_RectTransform;
        public RectTransform rectTransform
        {
            get
            {
                if (m_RectTransform == null)
                    m_RectTransform = GetComponent<RectTransform>();
                return m_RectTransform;
            }
        }
        public UnityEvent rectFolded { get; } = new UnityEvent();

        public UnityEvent rectUnfolded { get; } = new UnityEvent();

        void Awake()
        {
            m_FoldTween = new RectTween()
            {
                duration = UIConfig.widgetsFoldTime,
                ignoreTimeScale = true
            };
            m_FoldTween.AddOnChangedCallback(OnSetRect);
            m_FoldTween.AddOnCompleteCallback(OnFoldTransitionComplete);

            m_UnfoldTween = new RectTween()
            {
                duration = UIConfig.widgetsFoldTime,
                ignoreTimeScale = true
            };
            m_UnfoldTween.AddOnChangedCallback(OnSetRect);
            m_UnfoldTween.AddOnCompleteCallback(OnUnfoldTransitionComplete);

            m_TweenRunner = new TweenRunner<RectTween>();
            m_TweenRunner.Init(this);
        }

        public void Fold(bool instant = false)
        {
            if (!m_IsFolded)
                rectFolded.Invoke();

            m_IsFolded = true;

            if (instant || !Application.isPlaying)
            {
                OnFoldTransitionComplete();
            }
            else
            {

                m_FoldTween.startValue = new Rect(rectTransform.anchoredPosition, rectTransform.sizeDelta);
                m_FoldTween.targetValue = m_FoldedRect;
                m_TweenRunner.StartTween(m_FoldTween, EaseType.EaseInCubic);
            }
        }

        public void Unfold(bool instant = false)
        {
            if (m_IsFolded)
                rectUnfolded.Invoke();

            m_IsFolded = false;

            if (instant || !Application.isPlaying)
            {
                OnUnfoldTransitionComplete();
            }
            else
            {
                m_UnfoldTween.startValue = new Rect(rectTransform.anchoredPosition, rectTransform.sizeDelta);
                m_UnfoldTween.targetValue = m_UnfoldedRect;
                m_TweenRunner.StartTween(m_UnfoldTween, EaseType.EaseInCubic);
            }
        }
        private void OnFoldTransitionComplete()
        {
            OnSetRect(m_FoldedRect);
        }

        private void OnUnfoldTransitionComplete()
        {
            OnSetRect(m_UnfoldedRect);
        }
        private void OnSetRect(Rect value)
        {
            rectTransform.sizeDelta = value.size;
            rectTransform.anchoredPosition = value.position;
        }

#if UNITY_EDITOR
        [ContextMenu(nameof(SetCurrentAsFolded))]
        void SetCurrentAsFolded()
        {
            m_FoldedRect = new Rect(rectTransform.anchoredPosition, rectTransform.sizeDelta);
        }
        [ContextMenu(nameof(SetCurrentAsUnfolded))]
        void SetCurrentAsUnfolded()
        {
            m_UnfoldedRect = new Rect(rectTransform.anchoredPosition, rectTransform.sizeDelta);
        }
        [ContextMenu("Unfold")]
        void ContextUnfold() => Unfold(false);
        [ContextMenu("Fold")]
        void ContextFold() => Fold(false);
#endif
    }
}
