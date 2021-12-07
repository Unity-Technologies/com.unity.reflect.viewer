using System.Collections;
using TMPro;
using Unity.Tweening;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Default notification controller. Handles fading/animation, text presentation, and destruction.
    /// </summary>
    public class Notification : MonoBehaviour, INotification
    {
#pragma warning disable 649
        /// <summary>
        /// Reference to the Text MonoBehaviour, useful when setting the notification's text content.
        /// </summary>
        [SerializeField, Tooltip("Reference to the Text MonoBehaviour, useful when setting the notification's text content.")]
        TMP_Text m_NotificationText;

        /// <summary>
        /// Reference to the UI content.  Useful for handling the visibility of the notification.
        /// </summary>
        [SerializeField, Tooltip("Reference to the UI content.  Useful for handling the visibility of the notification.")]
        CanvasGroup m_CanvasGroup;

        /// <summary>
        /// Time, in seconds, it takes for the notification to fade in to full visibility.
        /// </summary>
        [SerializeField, Tooltip("Time, in seconds, it takes for the notification to fade in to full visibility.")]
        float m_FadeInDuration = 0.2f;

        /// <summary>
        /// The time it takes, in seconds, to scale from 0 to the final scale value.
        /// </summary>
        [SerializeField, Tooltip("The time it takes, in seconds, to scale from 0 to the final scale value.")]
        float m_ScaleOutDuration = 0.5f;

        /// <summary>
        /// The time the notification is displayed at full opacity, in seconds.
        /// </summary>
        [SerializeField, Tooltip("The time the notification is displayed at full opacity, in seconds.")]
        float m_DisplayDuration = 3.3f;

        /// <summary>
        /// The time it takes, in seconds, to fade out once the notification is displayed.
        /// </summary>
        [SerializeField, Tooltip("The time it takes, in seconds, to fade out once the notification is displayed.")]
        float m_FadeOutDuration = 0.5f;

        /// <summary>
        /// The final scale of the notification.
        /// </summary>
        [SerializeField, Tooltip("The final scale of the notification.")]
        float m_FinalNotificationScale = 0.00075f;
#pragma warning restore 649

        bool m_IsDone;
        Coroutine m_Animation;

        /// <inheritdoc />
        public Vector3 position { set => transform.localPosition = value; }

        /// <inheritdoc />
        public string text { set { m_NotificationText.text = value; } }

        /// <inheritdoc />
        public bool isDone => m_IsDone;

        float alpha { set { m_CanvasGroup.alpha = value; } }

        void Start()
        {
            m_Animation = StartCoroutine(Animate());
        }

        IEnumerator Animate()
        {
            if (m_CanvasGroup == null)
                Debug.LogWarning("Cannot Perform Fading without assigning a Canvas Group", this);

            alpha = 0.0f;
            transform.localScale = Vector3.zero;

            // Scale it out
            StartCoroutine(TweenHelper.Interpolate(interpolationValue =>
            {
                if (transform != null)
                    transform.localScale = Vector3.one * interpolationValue * m_FinalNotificationScale;
            }, m_ScaleOutDuration, TweenHelper.EaseType.CubicEaseIn));

            // Fade In
            yield return TweenHelper.Interpolate(interpolationValue => { alpha = interpolationValue; }, m_FadeInDuration);

            // Hold
            yield return new WaitForSeconds(m_DisplayDuration);

            // Fade Out
            yield return TweenHelper.Interpolate(interpolationValue => { alpha = 1.0f - interpolationValue; }, m_FadeOutDuration);

            m_IsDone = true;
        }

        void OnDestroy()
        {
            if (m_Animation != null)
                StopCoroutine(m_Animation);
            m_Animation = null;
        }
    }
}
