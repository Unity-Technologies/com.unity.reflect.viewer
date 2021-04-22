using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Set the emissive quality of materials
    /// </summary>
    /// <remarks>
    /// Only works with the built-in Standard Shader.
    /// Material must have emission enabled.
    /// </remarks>
    public class EmissionHighlight : BaseHighlight
    {
        const string k_ShaderEmissionColorParameter = "_EmissionColor";
        static readonly int k_EmissionColor = Shader.PropertyToID(k_ShaderEmissionColorParameter);

        [SerializeField, Tooltip("Changes emission on materials by this factor. 0 is base emission, and 1 is this value")]
        float m_EmissionValue = 1f;

        [SerializeField, Tooltip("The length of time that a fade transition will take")]
        float m_TransitionDuration = 0.1f;

        [SerializeField, Tooltip("Emissive Color when highlighted")]
        Color m_EmissionColor = Color.blue;

        [SerializeField, Tooltip("If pulse is active, highlight will be activated then deactivated after the lerp time")]
        bool m_Pulse;

        Color m_HDREmissionColor;
        Coroutine m_SetEmissionCoroutine;
        MaterialPropertyBlock m_HighlightMaterialPropertyBlock;
        Dictionary<int, Color> m_OriginalColors = new Dictionary<int, Color>();

        /// <summary>
        /// Changes emission on materials by this factor. 0 is base emission, and 1 is this value
        /// </summary>
        public float emissionValue
        {
            get => m_EmissionValue;
            set => m_EmissionValue = value;
        }

        /// <summary>
        /// The length of time that a fade transition will take
        /// </summary>
        public float transitionDuration
        {
            get => m_TransitionDuration;
            set => m_TransitionDuration = value;
        }

        /// <summary>
        /// Emissive Color when highlighted
        /// </summary>
        public Color emissionColor
        {
            get => m_EmissionColor;
            set => m_EmissionColor = value;
        }

        /// <summary>
        /// If pulse is active, highlight will be activated then deactivated after the lerp time
        /// </summary>
        public bool pulse
        {
            get => m_Pulse;
            set => m_Pulse = value;
        }

        void Start()
        {
            m_HDREmissionColor = m_EmissionColor * Mathf.LinearToGammaSpace(m_EmissionValue);
            m_HighlightMaterialPropertyBlock = new MaterialPropertyBlock();
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                m_OriginalColors[rendererID] = rend.sharedMaterial.GetColor(k_EmissionColor);
            }
        }

        void OnValidate()
        {
            if (m_TransitionDuration < 0f)
            {
                m_TransitionDuration = 0f;
            }
        }

        protected override void Highlight(bool instant)
        {
            if (m_SetEmissionCoroutine != null)
                StopCoroutine(m_SetEmissionCoroutine);
            if (!instant)
                m_SetEmissionCoroutine = StartCoroutine(IncreaseEmissionOverTime());
            else
                SetEmission(1f);
        }

        protected override void UnHighlight(bool instant)
        {
            if (m_SetEmissionCoroutine != null)
                StopCoroutine(m_SetEmissionCoroutine);
            if (!instant)
                m_SetEmissionCoroutine = StartCoroutine(DecreaseEmissionOverTime());
            else
                SetEmission(0f);
        }

        void SetEmission(float amount)
        {
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                if (!m_OriginalColors.ContainsKey(rendererID))
                {
                    m_OriginalColors[rendererID] = rend.sharedMaterial.GetColor(k_EmissionColor);
                }

                var highlightColor = Vector4.Lerp(m_OriginalColors[rendererID], m_HDREmissionColor, amount);
                m_HighlightMaterialPropertyBlock.SetColor(k_EmissionColor, highlightColor);
                rend.SetPropertyBlock(m_HighlightMaterialPropertyBlock);
            }
        }

        IEnumerator IncreaseEmissionOverTime()
        {
            var time = 0f;
            while (time <= m_TransitionDuration && m_TransitionDuration > 0f)
            {
                var a = time / m_TransitionDuration;
                SetEmission(a);
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            SetEmission(1f);
            m_SetEmissionCoroutine = null;

            if (m_Pulse)
            {
                Deactivate();
            }
        }

        IEnumerator DecreaseEmissionOverTime()
        {
            var time = 0f;
            while (time <= m_TransitionDuration && m_TransitionDuration > 0f)
            {
                var a = 1f - time / m_TransitionDuration;
                SetEmission(a);
                time += Time.unscaledDeltaTime;
                yield return null;
            }

            SetEmission(0f);
            m_SetEmissionCoroutine = null;
        }
    }
}
