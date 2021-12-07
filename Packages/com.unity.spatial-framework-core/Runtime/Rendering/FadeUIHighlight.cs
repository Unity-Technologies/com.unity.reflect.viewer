using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SpatialFramework.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Sets the transparency of canvas renderers.
    /// </summary>
    public class FadeUIHighlight : MonoBehaviour
    {
        [SerializeField, Tooltip("Used to set the mode of capturing renderers on an object or to use only manually set renderers.")]
        RendererCaptureDepth m_RendererCaptureDepth = RendererCaptureDepth.AllChildRenderers;

        [SerializeField, Tooltip("Manually set canvas renderers to be affected by the highlight.")]
        CanvasRenderer[] m_ManuallySetRenderers = new CanvasRenderer[0];

        [SerializeField, Range(0f, 1f), Tooltip("Fades canvas renderers by this factor. 0 is completely transparent, " +
             "and 1 is the starting alpha of the renderer.")]
        float m_FadeAmount = 0.5f;

        [SerializeField, Tooltip("How long to fade in/out.")]
        UpDownTimer m_FadeTimer = new UpDownTimer();

        [SerializeField]
        UnityEvent m_OnFadeInFinished = new UnityEvent();

        [SerializeField]
        UnityEvent m_OnFadeOutFinished = new UnityEvent();

        CanvasRenderer[] m_Renderers;
        bool m_HighlightActive;
        Coroutine m_SetAlphaOverTimeCoroutine;
        Dictionary<int, float> m_OriginalAlphas = new Dictionary<int, float>();

        /// <summary>
        /// Used to set the mode of capturing renderers on an object or to use only manually set renderers.
        /// </summary>
        public RendererCaptureDepth rendererCaptureDepth
        {
            get => m_RendererCaptureDepth;
            set => m_RendererCaptureDepth = value;
        }

        /// <summary>
        /// Manually set canvas renderers to be affected by the highlight.
        /// </summary>
        public CanvasRenderer[] manuallySetRenderers
        {
            get => m_ManuallySetRenderers;
            set => m_ManuallySetRenderers = value;
        }

        /// <summary>
        /// Fades materials by this factor. 0 is completely transparent, and 1 is the starting opacity of the material
        /// </summary>
        public float fadeAmount
        {
            get => m_FadeAmount;
            set => m_FadeAmount = Mathf.Clamp01(value);
        }

        /// <summary>
        /// The length of time that a fade out transition will take
        /// </summary>
        public float fadeOutDuration
        {
            get => m_FadeTimer.timeToMax;
            set => m_FadeTimer.timeToMax = value;
        }

        /// <summary>
        /// The length of time that a fade in transition will take
        /// </summary>
        public float fadeInDuration
        {
            get => m_FadeTimer.timeToMin;
            set => m_FadeTimer.timeToMin = value;
        }

        /// <summary>
        /// Event called when fade-in has finished.
        /// </summary>
        public UnityEvent onFadeInFinished => m_OnFadeInFinished;

        /// <summary>
        /// Event called when fade-out has finished.
        /// </summary>
        public UnityEvent onFadeOutFinished => m_OnFadeOutFinished;

        protected virtual void OnEnable()
        {
            if (RefreshHighlightRenderers() < 1)
            {
                enabled = false;
            }
        }

        protected virtual void OnDisable()
        {
            Deactivate(true);
        }

        protected virtual void OnDestroy()
        {
            Deactivate(true);
        }

        /// <summary>
        /// Used to cache and verify number of renderers used in the highlight.
        /// </summary>
        /// <returns>Number of cached highlight renderers</returns>
        int RefreshHighlightRenderers()
        {
            var renderers = new HashSet<CanvasRenderer>(manuallySetRenderers.Where(r => r != null));
            switch (rendererCaptureDepth)
            {
                case RendererCaptureDepth.AllChildRenderers:
                    renderers.UnionWith(GetComponentsInChildren<CanvasRenderer>());
                    break;
                case RendererCaptureDepth.CurrentRenderer:
                    renderers.UnionWith(GetComponents<CanvasRenderer>());
                    break;
                case RendererCaptureDepth.ManualOnly:
                    break;
                default:
                    Debug.LogError($"{gameObject.name} highlight has an invalid renderer capture mode {m_RendererCaptureDepth}.", this);
                    enabled = false;
                    break;

            }

            if (renderers.Count > 0)
            {
                m_Renderers = renderers.ToArray();
                foreach (var rend in m_Renderers)
                {
                    var rendererID = rend.GetInstanceID();
                    if (!m_OriginalAlphas.ContainsKey(rendererID))
                    {
                        m_OriginalAlphas[rendererID] = rend.GetAlpha();
                    }
                }
            }
            else
            {
                m_Renderers = new CanvasRenderer[0];
                Debug.LogWarning($"{gameObject.name} Fade UI Highlight has no renderers set.", this);
            }

            return m_Renderers.Length;
        }

        /// <summary>
        /// Clears and repopulates the container of original alphas for each renderer.
        /// </summary>
        public void RefreshOriginalAlphas()
        {
            m_OriginalAlphas.Clear();
            RefreshHighlightRenderers();
        }

        /// <summary>
        /// Activates the highlight after refreshing cached renderers.
        /// </summary>
        /// <param name="instant">Whether activation is instantaneous or occurs over a period of time</param>
        /// <param name="force">Setting this to true will reset the fade timer and force activation.</param>
        public void Activate(bool instant = false, bool force = false)
        {
            if (force)
            {
                m_HighlightActive = false;
                m_FadeTimer.SetToMin();
            }

            if (m_HighlightActive || RefreshHighlightRenderers() == 0)
                return;

            m_HighlightActive = true;
            if (m_SetAlphaOverTimeCoroutine != null)
            {
                StopCoroutine(m_SetAlphaOverTimeCoroutine);
            }

            if (!instant && gameObject.activeInHierarchy)
                m_SetAlphaOverTimeCoroutine = StartCoroutine(FadeAlphaOverTime(true));
            else
            {
                m_FadeTimer.SetToMax();
                SetFadeProgress(1f);
                onFadeOutFinished.Invoke();
            }
        }

        /// <summary>
        /// Deactivates the highlight after refreshing cached renderers.
        /// </summary>
        /// <param name="instant">Whether deactivation is instantaneous or occurs over a period of time</param>
        /// <param name="force">Setting this to true will reset the fade timer and force deactivation.</param>
        public void Deactivate(bool instant = false, bool force = false)
        {
            if (force)
            {
                m_HighlightActive = true;
                m_FadeTimer.SetToMax();
            }

            if (!m_HighlightActive || RefreshHighlightRenderers() == 0)
                return;

            m_HighlightActive = false;
            if (m_SetAlphaOverTimeCoroutine != null)
            {
                StopCoroutine(m_SetAlphaOverTimeCoroutine);
            }

            if (!instant && gameObject.activeInHierarchy)
                m_SetAlphaOverTimeCoroutine = StartCoroutine(FadeAlphaOverTime(false));
            else
            {
                m_FadeTimer.SetToMin();
                SetFadeProgress(0f);
                onFadeInFinished.Invoke();
            }
        }

        IEnumerator FadeAlphaOverTime(bool fadeOut)
        {
            if (fadeOut)
            {
                while (!m_FadeTimer.CountUp(Time.unscaledDeltaTime))
                {
                    SetFadeProgress(m_FadeTimer.current);
                    yield return null;
                }

                SetFadeProgress(1f);
                onFadeOutFinished.Invoke();
            }
            else
            {
                while (!m_FadeTimer.CountDown(Time.unscaledDeltaTime))
                {
                    SetFadeProgress(m_FadeTimer.current);
                    yield return null;
                }

                SetFadeProgress(0f);
                onFadeInFinished.Invoke();
            }

            m_SetAlphaOverTimeCoroutine = null;
        }

        void SetFadeProgress(float progress)
        {
            progress = Mathf.Clamp01(progress);
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                if (!m_OriginalAlphas.ContainsKey(rendererID))
                    continue;
                var originalAlpha = m_OriginalAlphas[rendererID];
                var fadeAlpha = originalAlpha * fadeAmount;
                rend.SetAlpha(Mathf.Lerp(originalAlpha, fadeAlpha, progress));
            }
        }
    }
}
