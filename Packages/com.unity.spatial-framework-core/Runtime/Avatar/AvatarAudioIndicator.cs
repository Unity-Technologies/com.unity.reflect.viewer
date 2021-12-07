using UnityEngine;
using UnityEngine.UI;

namespace Unity.SpatialFramework.Avatar
{
    /// <summary>
    /// Controls the avatar audio visual indicators.
    /// This includes visual affects on the level indicator, mute icon, outline, particles, and mouth blend shapes.
    /// </summary>
    public class AvatarAudioIndicator : MonoBehaviour
    {
        const float k_MicLevelFallRate = 2f;
        const float k_BlendLerpSpeed = 30.0f;

        [SerializeField, Tooltip("Particle systems that have their emission controlled by the audio level. Their rate over time will be scaled by the audio level and the ParticleRate property.")]
        ParticleSystem[] m_ParticleSystems;

        [SerializeField, Tooltip("Scales the particle emission rate over time property so that it can be tuned to the particle system.")]
        float m_ParticleRate = 4f;

        [SerializeField, Tooltip("The skinned mesh renderer that has a blend shape that moves with the audio level. This is used to open and close the mouth.")]
        SkinnedMeshRenderer m_SkinnedMeshRenderer;

        [SerializeField, Tooltip("The blend shape index that is used to drive the blend shape. This is used to open and close the mouth.")]
        int m_BlendShapeIndex;

        [SerializeField, Tooltip("The image that is an outline that becomes opaque with the audio level.")]
        Image m_Outline;

        [SerializeField, Tooltip("Rect transform that is scaled in its Y axis with the audio level.")]
        RectTransform m_MicIndicator;

        [SerializeField, Tooltip("Image that displays an icon indicating that the avatar's audio is muted.")]
        Image m_MutedIcon;

        [SerializeField, Tooltip("Image that displays an icon indicating that the avatar's audio is not muted.")]
        Image m_UnmutedIcon;

        ParticleSystem.EmissionModule[] m_ParticleEmissionModules;
        bool m_Muted;
        float m_MouthSize;
        float m_SmoothedMicLevel;
        float m_SmoothedMicVelocity;
        float m_NormalizedMic;

        /// <summary>
        /// The current microphone level normalized from 0 to 1. Setting this will drive all the level related visuals such as opening and closing the mouth.
        /// </summary>
        public float normalizedMicLevel
        {
            set => m_NormalizedMic = value;
        }

        /// <summary>
        /// The current mute state of the avatar's audio. Setting this will display a mute icon on the avatar.
        /// </summary>
        public bool muted
        {
            set
            {
                if (m_Muted != value)
                {
                    m_Muted = value;
                    SetMuteIcon(value);
                }
            }
        }

        void Start()
        {
            SetMuteIcon(m_Muted);
            if (m_ParticleSystems != null)
            {
                m_ParticleEmissionModules = new ParticleSystem.EmissionModule[m_ParticleSystems.Length];
                for (var index = 0; index < m_ParticleSystems.Length; index++)
                    m_ParticleEmissionModules[index] = m_ParticleSystems[index].emission;
            }
        }

        void Update()
        {
            SetMouth(m_NormalizedMic);
            CalculateSmoothedVolumeLevel(m_NormalizedMic);
            EmitParticles();
            SetNameTag();
        }

        void EmitParticles()
        {
            if (m_ParticleEmissionModules != null)
            {
                var rate = m_SmoothedMicLevel * m_ParticleRate;
                for (var index = 0; index < m_ParticleEmissionModules.Length; index++)
                {
                    m_ParticleEmissionModules[index].rateOverTime = rate;
                }
            }
        }

        void SetMouth(float mouthOpenValue)
        {
            // Use the current voice volume (a value between 0 - 1) to calculate the target mouth size (between 0.1 and 1.0)
            var targetMouthSize = Mathf.Lerp(0.1f, 1.0f, mouthOpenValue);

            // Animate the mouth size towards the target mouth size to keep the open / close animation smooth
            m_MouthSize = Mathf.Lerp(m_MouthSize, targetMouthSize, k_BlendLerpSpeed * Time.unscaledDeltaTime);

            // Apply the mouth size to the blendshape
            m_SkinnedMeshRenderer.SetBlendShapeWeight(m_BlendShapeIndex, 100f - (m_MouthSize * 100f));
        }

        void SetMuteIcon(bool isMuted)
        {
            if (m_MutedIcon != null)
                m_MutedIcon.enabled = isMuted;

            if (m_MutedIcon != null)
                m_UnmutedIcon.enabled = !isMuted;
        }

        void SetNameTag()
        {
            // Set outline color alpha
            var newColor = m_Outline.color;
            newColor.a = m_SmoothedMicLevel;
            m_Outline.color = newColor;

            // Scale mic indicator
            var localScale = m_MicIndicator.localScale;
            localScale.y = m_SmoothedMicLevel;
            m_MicIndicator.localScale = localScale;
        }

        void CalculateSmoothedVolumeLevel(float currentValue)
        {
            // Mic level visual indicator jumps up to current level, but falls down at smooth fixed rate
            if (m_SmoothedMicLevel > currentValue)
            {
                var unscaledDeltaTime = Time.unscaledDeltaTime;
                m_SmoothedMicLevel = Mathf.Clamp01(m_SmoothedMicLevel + m_SmoothedMicVelocity * unscaledDeltaTime);
                m_SmoothedMicVelocity += -k_MicLevelFallRate * unscaledDeltaTime;
            }
            else
            {
                m_SmoothedMicLevel = currentValue;
                m_SmoothedMicVelocity = 0f;
            }
        }
    }
}
