using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.SpatialFramework.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Draws an outline on an object when highlighting. Can either transition the color and or size of the out line
    /// as selected or be instant on.
    /// </summary>
    public class OutlineHighlight : BaseHighlight
    {
        public enum MaterialHighlightMode
        {
            /// <summary>Source material is used to generate the highlight material</summary>
            Material,
            /// <summary>Shader is used to generate the highlight material</summary>
            Shader,
        }

        const float k_OutlineWidth = 0.005f;
        const string k_ShaderColorParameter = "_Color";
        const string k_ShaderWidthParameter = "g_flOutlineWidth";
        static readonly int k_GFlOutlineWidth = Shader.PropertyToID(k_ShaderWidthParameter);
        static readonly int k_Color = Shader.PropertyToID(k_ShaderColorParameter);
        
#pragma warning disable 649
        [SerializeField, Tooltip("Selects source for the highlight material. Either using a shader or material.")]
        MaterialHighlightMode m_HighlightMode = MaterialHighlightMode.Shader;

        [SerializeField, Tooltip("Outline highlight shader to use for highlight material.")]
        Shader m_Shader;

        [SerializeField, Tooltip("Material used for drawing the outline highlight.")]
        Material m_HighlightMaterial;

        [SerializeField, Tooltip("Transition outline width over time")]
        bool m_TransitionWidth;

        [SerializeField, Range(0f, 1f), Tooltip("The outline width used if no transition or the end value for transition width of outline")]
        float m_OutlineScale = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("Starting value for transition width of outline")]
        float m_StartingOutlineScale;

        [SerializeField, Tooltip("Transition outline color over time")]
        bool m_TransitionColor;

        [SerializeField, Tooltip("The outline color used if no transition or the end value for transition color of outline")]
        Color m_OutlineColor = new Color(0.3f, 0.6f, 1f, 1f);

        [SerializeField, Tooltip("Starting value for transition color of outline")]
        Color m_StartingOutlineColor = Color.black;

        [SerializeField, Tooltip("Time it takes to transition from start to end on highlight")]
        float m_TransitionDuration = 0.3f;

        [SerializeField, Tooltip("Use material values for starting color and width")]
        bool m_StartWithMaterialValues;
#pragma warning restore 649

        Material m_InstanceOutlineMaterial;

        Dictionary<int, Material[]> m_OriginalMaterials = new Dictionary<int, Material[]>();
        Dictionary<int, Material[]> m_HighlightMaterials = new Dictionary<int, Material[]>();

        Coroutine m_SetOutlineCoroutine;

        /// <summary>
        /// Time it takes to transition from start to end on highlight
        /// </summary>
        public float transitionDuration
        {
            get => m_TransitionDuration;
            set => m_TransitionDuration = value;
        }

        /// <summary>
        /// Transition outline width over time
        /// </summary>
        bool transitionWidth
        {
            get => m_TransitionWidth;
            set => m_TransitionWidth = value;
        }

        /// <summary>
        /// Transition outline color over time
        /// </summary>
        bool transitionColor
        {
            get => m_TransitionColor;
            set => m_TransitionColor = value;
        }

        /// <summary>
        /// The outline color used if no transition or the end value for transition color of outline
        /// </summary>
        public Color outlineColor
        {
            get => m_OutlineColor;
            set => m_OutlineColor = value;
        }

        /// <summary>
        /// Starting value for transition color of outline
        /// </summary>
        public Color startingOutlineColor
        {
            get => m_StartingOutlineColor;
            set => m_StartingOutlineColor = value;
        }

        /// <summary>
        /// The outline width used if no transition or the end value for transition width of outline
        /// </summary>
        public float outlineScale
        {
            get => m_OutlineScale;
            set => m_OutlineScale = value;
        }

        /// <summary>
        /// Starting value for transition width of outline
        /// </summary>
        public float startingOutlineScale
        {
            get => m_StartingOutlineScale;
            set => m_StartingOutlineScale = value;
        }

        /// <summary>
        /// A 0-1 relative outline scale that takes into account the ideal base outline width,
        /// multiplied by the user specified value. This allows for more intuitive adjustment of the value.
        /// This is the value used if there is no transition otherwise this is the end value of a transition.
        /// </summary>
        float relativeOutlineScale => outlineScale * k_OutlineWidth;

        /// <summary>
        /// A 0-1 relative outline scale that takes into account the ideal base outline width,
        /// multiplied by the user specified value. This allows for more intuitive adjustment of the value.
        /// This is the start value of a transition otherwise this value is not used.
        /// </summary>
        float startingRelativeOutlineScale => startingOutlineScale * k_OutlineWidth;

        void Start()
        {
            InstantiateHighlightMaterial();

            if (m_StartWithMaterialValues)
            {
                startingOutlineScale = m_HighlightMaterial.GetFloat(k_GFlOutlineWidth) / k_OutlineWidth;
                startingOutlineColor = m_HighlightMaterial.GetColor(k_Color);
            }

            foreach (var rend in m_Renderers)
            {
                UpdateCachedMaterials(rend);
            }
        }

        void OnValidate()
        {
            if (m_TransitionDuration < 0f)
            {
                m_TransitionDuration = 0f;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Destroy(m_InstanceOutlineMaterial);
        }

        protected override void Highlight(bool instant)
        {
            SetHighlightMaterials();

            if (instant || !transitionWidth && !transitionColor || Mathf.Approximately(m_TransitionDuration, 0f))
            {
                m_InstanceOutlineMaterial.SetFloat(k_GFlOutlineWidth, relativeOutlineScale);
                m_InstanceOutlineMaterial.SetColor(k_Color, m_OutlineColor);
            }
            else
            {
                this.RestartCoroutine(ref m_SetOutlineCoroutine, SetOutlineWidthOverTime(true));
            }
        }

        protected override void UnHighlight(bool instant)
        {
            if (m_InstanceOutlineMaterial == null)
                return;

            if (instant || !transitionWidth && !transitionColor || Mathf.Approximately(m_TransitionDuration, 0f))
            {
                SetOriginalMaterials();
            }
            else
            {
                this.RestartCoroutine(ref m_SetOutlineCoroutine, SetOutlineWidthOverTime(false));
            }
        }

        protected override void OnRenderersRefreshed() { }

        void InstantiateHighlightMaterial()
        {
            if (m_Shader == null && m_HighlightMaterial == null)
            {
                Debug.LogError($"{gameObject.name} has no highlight material or shader set!", this);
                enabled = false;
                return;
            }

            if (m_Renderers.Length == 0)
            {
                Debug.LogWarning($"{gameObject.name} has no renderers to highlight!", this);
                return;
            }

            const string outlineMaterialName = "Outline Material Instance";

            switch (m_HighlightMode)
            {
                case MaterialHighlightMode.Material:
                    if (m_HighlightMaterial == null)
                    {
                        Debug.LogError($"{gameObject.name} Outline highlight has no material assigned. Please assign outline material.", this);
                        enabled = false;
                        break;
                    }

                    m_InstanceOutlineMaterial = new Material(m_HighlightMaterial) { name = outlineMaterialName };
                    break;
                case MaterialHighlightMode.Shader:
                    if (m_Shader == null)
                    {
                        Debug.LogError($"{gameObject.name} Outline highlight has no shader assigned. Please assign outline shader. ", this);
                        enabled = false;
                        break;
                    }

                    m_InstanceOutlineMaterial = new Material(m_Shader) { name = outlineMaterialName };
                    break;
                default:
                    Debug.LogError($"{gameObject.name} Outline highlight has an invalid highlight mode {m_HighlightMode}.", this);
                    enabled = false;
                    break;
            }
        }

        void SetHighlightMaterials()
        {
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                if (!m_OriginalMaterials.ContainsKey(rendererID))
                    UpdateCachedMaterials(rend);

                rend.materials = m_HighlightMaterials[rendererID];
            }
        }

        void SetOriginalMaterials()
        {
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                if (!m_OriginalMaterials.ContainsKey(rendererID))
                    continue;

                rend.materials = m_OriginalMaterials[rendererID];
            }
        }

        IEnumerator SetOutlineWidthOverTime(bool enableOutline)
        {
            var time = 0f;
            while (time <= m_TransitionDuration)
            {
                var alpha = time / m_TransitionDuration;
                if (!enableOutline)
                    alpha = 1f - alpha;

                if (transitionWidth)
                {
                    var size = Mathf.Lerp(startingRelativeOutlineScale, relativeOutlineScale, alpha);
                    m_InstanceOutlineMaterial.SetFloat(k_GFlOutlineWidth, size);
                }

                if (transitionColor)
                {
                    var color = Color.Lerp(startingOutlineColor, outlineColor, alpha);
                    m_InstanceOutlineMaterial.SetColor(k_Color, color);
                }

                time += Time.unscaledDeltaTime;
                yield return null;
            }

            if (!enableOutline)
                SetOriginalMaterials();

            m_SetOutlineCoroutine = null;
        }

        void UpdateCachedMaterials(Renderer rend)
        {
            var rendererID = rend.GetInstanceID();
            m_OriginalMaterials[rendererID] = rend.sharedMaterials;
            var highlightMaterials = m_OriginalMaterials[rendererID].Concat(
                Enumerable.Repeat(m_InstanceOutlineMaterial, 1)).ToArray();
            m_HighlightMaterials[rendererID] = highlightMaterials;
        }
    }
}
