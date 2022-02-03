using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace Unity.SpatialFramework.Avatar
{
    /// <summary>
    /// Controls the overall state of an avatar including setting the name, color, and other relevant properties.
    /// This component references other components that react to changes in the avatar properties such as text labels and material colors.
    /// </summary>
    public class AvatarControls : MonoBehaviour
    {
        [Header("Avatar Properties")]
        [SerializeField, Tooltip("The name to display for this avatar.")]
        string m_AvatarName;

        [SerializeField, Tooltip("If enabled, the avatar will auto generate initials based on the current name.")]
        bool m_AutoGenerateInitials;

        [SerializeField, Tooltip("The initials display for this avatar.")]
        string m_AvatarInitials;

        [SerializeField, Tooltip("The primary color chosen for this avatar.")]
        Color m_Color = Color.white;

        [SerializeField, Tooltip("If true, this avatar will display that its microphone is muted. Otherwise the avatar will indicate the microphone level.")]
        bool m_Muted;

        [SerializeField, Tooltip("The current microphone level, normalized between 0-1.")]
        float m_NormalizedMicLevel;

        [Header("References")]
        [SerializeField, Tooltip("The text label to show the avatar name.")]
        TMP_Text m_NameText;

        [SerializeField, Tooltip("The text label to show the avatar initials.")]
        TMP_Text m_Initials;

        [SerializeField, Tooltip("The image to display the avatar profile image. The image will be tinted by the avatar's color.")]
        Image m_ProfileImage;

        [SerializeField, Tooltip("List of images whose color should be changed to match the avatar's color.")]
        Image[] m_ColoredImages;

        [SerializeField, Tooltip("List of renderers whose color should be changed to match the avatar's color.")]
        Renderer[] m_ColoredGeo;

        [SerializeField, Tooltip("(Optional) A MicInput component to get the current microphone input level.")]
        MicInput m_Voice;

        [SerializeField, Tooltip("Reference to component that controls elements of the avatar that react to the audio input level.")]
        AvatarAudioIndicator m_AudioIndicator;

        static readonly int k_ColorProperty = Shader.PropertyToID("_Color");

        static readonly int k_URPColorProperty = Shader.PropertyToID("_BaseColor");

        int m_ColorPropertyId;

        /// <summary>
        /// The name to display for this avatar.
        /// Setting this property will update the avatar immediately.
        /// </summary>
        public string avatarName
        {
            get => m_AvatarName;
            set
            {
                m_AvatarName = value;
                UpdateName(m_AvatarName);
            }
        }

        /// <summary>
        /// The initials to display for this avatar. Setting this will turn off the option to Auto Generate Initials
        /// Setting this property will update the avatar immediately.
        /// </summary>
        public string avatarInitials
        {
            get => m_AvatarInitials;
            set
            {
                m_AvatarInitials = value;
                UpdateInitials(m_AvatarInitials);
            }
        }

        /// <summary>
        /// If enabled, the avatar will auto generate initials based on the current name. Otherwise the initials will use the AvatarInitials property
        /// Setting this property will update the avatar immediately.
        /// </summary>
        public bool autoGenerateInitials
        {
            get => m_AutoGenerateInitials;
            set
            {
                m_AutoGenerateInitials = value;
                UpdateInitials(m_AutoGenerateInitials ? AutoGenerateInitialsFromName(m_AvatarName) : m_AvatarInitials);
            }
        }

        /// <summary>
        /// The primary color chosen for this avatar.
        /// Setting this property will update the avatar immediately.
        /// </summary>
        public Color color
        {
            get => m_Color;
            set
            {
                m_Color = value;
                UpdateColor(m_Color);
            }
        }

        /// <summary>
        /// If true, this avatar will display that its microphone is muted. Otherwise the avatar will indicate the microphone level.
        /// Setting this property will update the avatar immediately.
        /// </summary>
        public bool muted
        {
            get => m_Muted;
            set
            {
                m_Muted = value;
                UpdateMuteState(m_Muted);
            }
        }

        /// <summary>
        /// The current microphone level, normalized between 0-1.
        /// Setting this property will update the avatar immediately.
        /// </summary>
        public float normalizedMicLevel
        {
            get => m_NormalizedMicLevel;
            set
            {
                m_NormalizedMicLevel = value;
                UpdateMicLevel(m_NormalizedMicLevel);
            }
        }

        /// <summary>
        /// (Optional) A MicInput component to get the current microphone input level.
        /// </summary>
        public MicInput voice
        {
            get => m_Voice;
            set => m_Voice = value;
        }

        void OnEnable()
        {
            m_ColorPropertyId = GraphicsSettings.renderPipelineAsset == null ? k_ColorProperty : k_URPColorProperty;
            UpdateName(m_AvatarName);
            UpdateColor(m_Color);
            UpdateMuteState(m_Muted);
            UpdateMicLevel(m_NormalizedMicLevel);
        }

        void Update()
        {
            if (m_Voice != null)
            {
                m_NormalizedMicLevel = Mathf.Clamp(m_Voice.MicInputLevel, 0f, 1f);
                UpdateMicLevel(m_NormalizedMicLevel);
            }

            // Update color every frame because materials may change at any point
            UpdateColor(m_Color);
        }

        void UpdateName(string newName)
        {
            // Get initials from name
            if (m_AutoGenerateInitials)
            {
                UpdateInitials(AutoGenerateInitialsFromName(newName));
            }

            if (m_NameText != null)
            {
                m_NameText.text = newName;

                // Force update so layout updates properly
                m_NameText.rectTransform.ForceUpdateRectTransforms();
            }
        }

        void UpdateInitials(string initials)
        {
            if (m_Initials != null)
                m_Initials.text = initials;
        }

        void UpdateColor(Color avatarColor)
        {
            if (m_ProfileImage != null)
                m_ProfileImage.color = avatarColor;
            foreach (var coloredImage in m_ColoredImages)
            {
                coloredImage.color = avatarColor;
            }
            foreach (var coloredRenderer in m_ColoredGeo)
            {
                coloredRenderer.material.SetColor(m_ColorPropertyId, avatarColor);
            }
        }

        void UpdateMuteState(bool isMuted)
        {
            if (m_AudioIndicator != null)
                m_AudioIndicator.muted = isMuted;
        }

        void UpdateMicLevel(float normalizedLevel)
        {
            if (m_AudioIndicator != null)
                m_AudioIndicator.normalizedMicLevel = normalizedLevel;
        }

        static string AutoGenerateInitialsFromName(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                return "";

            var initialsRegex = new Regex(@"(\b[a-zA-Z])[a-zA-Z]* ?");
            var initials = initialsRegex.Replace(newName, "$1");

            return initials;
        }
    }
}
