using System;
using UnityEngine;

namespace Unity.SpatialFramework.Interaction
{
    /// <summary>
    /// Set of properties for the visual appearance of a interaction line including width, color, and bendiness
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "NewLineSettings.asset", menuName = "Spatial Framework/Interaction Line Settings")]
    public class InteractionLineSettings : ScriptableObject
    {
        [SerializeField, Tooltip("The width of the line (in centimeters).")]
        float m_LineWidth = 0.2f;

        [SerializeField, Tooltip("The relative width of the line from the start to the end.")]
        AnimationCurve m_WidthCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);

        [SerializeField, Tooltip("The color of the line as a gradient from start to end.")]
        Gradient m_LineColorGradient;

        [SerializeField, Tooltip("The minimum number of vertices used to draw the line. Increase this for a smoother color gradients, width changes, and bending.")]
        int m_MinimumVertexCount = 32;

        [SerializeField, Tooltip("If enabled, the line will be drawn to bend from the direction the ray origin is pointing to the actual endpoint.")]
        bool m_Bendable;

        [SerializeField, Tooltip("If enabled, the end point of the line will smoothly follow the end of the ray.")]
        bool m_SmoothEndpoint;

        [SerializeField, Tooltip("Sets the speed that the line's endpoint will follow the end of the ray.")]
        float m_FollowTightness = 50f;

        /// <summary>
        /// The width of the line (in centimeters).
        /// </summary>
        public float lineWidth
        {
            get => m_LineWidth;
            set => m_LineWidth = value;
        }

        /// <summary>
        /// The relative width of the line from the start to the end.
        /// </summary>
        public AnimationCurve widthCurve
        {
            get => m_WidthCurve;
            set => m_WidthCurve = value;
        }

        /// <summary>
        /// The color of the line as a gradient from start to end.
        /// </summary>
        public Gradient lineColorGradient => m_LineColorGradient;

        /// <summary>
        /// The minimum number of vertices used to draw the line. Increase this for a smoother color gradients, width changes, and bending.
        /// </summary>
        public int minimumVertexCount => m_MinimumVertexCount;

        /// <summary>
        /// If enabled, the line will be drawn to bend from the direction the ray origin is pointing to the actual endpoint.
        /// </summary>
        public bool bendable
        {
            get => m_Bendable;
            set => m_Bendable = value;
        }

        /// <summary>
        /// Sets the speed that the line's endpoint will follow the end of the ray.
        /// </summary>
        public float followTightness
        {
            get => m_FollowTightness;
            set => m_FollowTightness = value;
        }

        /// <summary>
        /// Sets the speed that the line's endpoint will follow the end of the ray.
        /// </summary>
        public bool smoothEndpoint
        {
            get => m_SmoothEndpoint;
            set => m_SmoothEndpoint = value;
        }
    }
}
