using System;
using System.Collections;
using System.Collections.Generic;
using Unity.SpatialFramework.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Sets the transparency of renderer materials
    /// </summary>
    /// <remarks>
    /// Only works with the standard shader.
    /// May not show up correctly in build if material with same shader keywords enabled is included.
    /// </remarks>
    public class FadeHighlight : BaseHighlight
    {
        /// <summary>
        /// Used to store original material with modified highlight material.
        /// </summary>
        struct HighlightMaterialPair
        {
            public Material OriginalMaterial;
            public Material HighlightMaterial;

            public HighlightMaterialPair(Material originalMaterial, Material highlightMaterial)
            {
                OriginalMaterial = originalMaterial;
                HighlightMaterial = highlightMaterial;
            }
        }

        const string k_ShaderColorParameter = "_Color";
        const string k_ShaderModeParameter = "_Mode";
        const string k_ShaderSrcBlendParameter = "_SrcBlend";
        const string k_ShaderDstBlendParameter = "_DstBlend";
        const string k_ShaderZWriteParameter = "_ZWrite";

        const string k_ShaderAlphaTestOn = "_ALPHATEST_ON";
        const string k_ShaderAlphaBlendOn = "_ALPHABLEND_ON";
        const string k_ShaderAlphaPremultiplyOn = "_ALPHAPREMULTIPLY_ON";

        const int k_ShaderMinRenderQueue = 3000;

        [SerializeField, Range(0f, 1f), Tooltip("Fades materials by this factor. 0 is completely transparent, and 1 is the starting opacity " +
             "of the material")]
        float m_FadeAmount = 0.5f;

        [SerializeField, Tooltip("How long to fade in/out.")]
        UpDownTimer m_FadeTimer = new UpDownTimer();

        [SerializeField]
        UnityEvent m_OnFadeInFinished = new UnityEvent();

        [SerializeField]
        UnityEvent m_OnFadeOutFinished = new UnityEvent();

        Coroutine m_SetAlphaOverTimeCoroutine;

        MaterialPropertyBlock m_HighlightMaterialPropertyBlock;
        Dictionary<int, Color> m_OriginalColors = new Dictionary<int, Color>();

        Dictionary<int, int> m_HighLightMaterialPairMapping = new Dictionary<int, int>();

        Dictionary<int, HighlightMaterialPair> m_HighLightMaterialPairs = new Dictionary<int, HighlightMaterialPair>();

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

        void Awake()
        {

            m_HighlightMaterialPropertyBlock = new MaterialPropertyBlock();

        }

        void Start()
        {
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                m_OriginalColors[rendererID] = rend.sharedMaterial.GetColor(k_ShaderColorParameter);
            }
        }

        protected override void Highlight(bool instant)
        {
            foreach (var rend in m_Renderers)
            {
                EnableTransparency(rend);
            }

            if (m_SetAlphaOverTimeCoroutine != null)
            {
                StopCoroutine(m_SetAlphaOverTimeCoroutine);
            }

            if (!instant)
                m_SetAlphaOverTimeCoroutine = StartCoroutine(FadeAlphaOverTime(true));
            else
            {
                m_FadeTimer.SetToMax();
                SetFadeProgress(1f);
                onFadeOutFinished.Invoke();
            }
        }

        protected override void UnHighlight(bool instant)
        {
            if (m_SetAlphaOverTimeCoroutine != null)
            {
                StopCoroutine(m_SetAlphaOverTimeCoroutine);
            }

            if (!instant)
                m_SetAlphaOverTimeCoroutine = StartCoroutine(FadeAlphaOverTime(false));
            else
            {
                m_FadeTimer.SetToMin();
                SetFadeProgress(0f);
                OnFadeInFinished();
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
                if (!m_OriginalColors.ContainsKey(rendererID))
                    continue;
                var originalColor = m_OriginalColors[rendererID];
                var fadeColor = originalColor;
                fadeColor.a *= fadeAmount;
                var lerpedColor = Vector4.Lerp(originalColor, fadeColor, progress);
                if (rend is SpriteRenderer sprite)
                {
                    sprite.color = lerpedColor;
                    continue;
                }

                m_HighlightMaterialPropertyBlock.SetColor(k_ShaderColorParameter, lerpedColor);
                rend.SetPropertyBlock(m_HighlightMaterialPropertyBlock);
            }
        }

        /// <summary>
        /// Switches the renderer's material to a clone of the primary material with the shader keywords
        /// for transparency enabled. Caches the original material and fade material for later use.
        /// </summary>
        /// <param name="rend">Renderer on which to enable transparency</param>
        void EnableTransparency(Renderer rend)
        {
            var rendererID = rend.GetInstanceID();
            if (rend is SpriteRenderer spriteRenderer)
            {
                if (!m_OriginalColors.ContainsKey(rendererID))
                    m_OriginalColors[rendererID] = spriteRenderer.color;

                return;
            }

            if (!m_HighLightMaterialPairMapping.ContainsKey(rendererID))
            {
                var material = rend.sharedMaterial;
                var originalMaterialID = material.GetInstanceID();
                if (!m_HighLightMaterialPairs.ContainsKey(originalMaterialID))
                {
                    var originalMaterial = material;

                    const string fadeSuffix = "_fade";
                    var fadeMaterial = new Material(material)
                    {
                        name = $"{material.name}{fadeSuffix}"
                    };
                    fadeMaterial.DisableKeyword(k_ShaderAlphaTestOn);
                    fadeMaterial.EnableKeyword(k_ShaderAlphaBlendOn);
                    fadeMaterial.DisableKeyword(k_ShaderAlphaPremultiplyOn);
                    fadeMaterial.renderQueue = Math.Max(fadeMaterial.renderQueue, k_ShaderMinRenderQueue);

                    fadeMaterial.SetFloat(k_ShaderModeParameter, 2);
                    fadeMaterial.SetInt(k_ShaderSrcBlendParameter, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    fadeMaterial.SetInt(k_ShaderDstBlendParameter, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    fadeMaterial.SetInt(k_ShaderZWriteParameter, 0);

                    m_HighLightMaterialPairs[originalMaterialID] = new HighlightMaterialPair(originalMaterial, fadeMaterial);
                }

                m_HighLightMaterialPairMapping[rendererID] = originalMaterialID;

                if (!m_OriginalColors.ContainsKey(rendererID))
                {
                    m_OriginalColors[rendererID] =
                        m_HighLightMaterialPairs[m_HighLightMaterialPairMapping[rendererID]].OriginalMaterial.GetColor(k_ShaderColorParameter);
                }
            }

            rend.material = m_HighLightMaterialPairs[m_HighLightMaterialPairMapping[rendererID]].HighlightMaterial;
        }

        /// <summary>
        /// Switches the primary material on the renderer back to the original shared material
        /// </summary>
        /// <param name="rend">Renderer on which to disable transparency</param>
        void DisableTransparency(Renderer rend)
        {
            var rendererID = rend.GetInstanceID();
            if (!m_HighLightMaterialPairMapping.ContainsKey(rendererID))
            {
                return;
            }

            rend.material = m_HighLightMaterialPairs[m_HighLightMaterialPairMapping[rendererID]].OriginalMaterial;
            m_HighlightMaterialPropertyBlock.SetColor(k_ShaderColorParameter, m_OriginalColors[rendererID]);
            rend.SetPropertyBlock(m_HighlightMaterialPropertyBlock);
        }

        void OnFadeInFinished()
        {
            foreach (var rend in m_Renderers)
            {
                DisableTransparency(rend);
            }

            onFadeInFinished.Invoke();
        }

        protected override void OnRenderersRefreshed() { }
    }
}
