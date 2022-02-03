using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Unity.SpatialFramework.Rendering
{
    /// <summary>
    /// Used to change the materials array of an object when highlighted. Can either add the highlight material to the
    /// renderers materials array or replace the renderers materials with the highlight material.
    /// </summary>
    /// <remarks>
    /// Layering a material will only draw the layer on the last sub mesh in a mesh renderer.
    /// </remarks>
    public class MaterialHighlight : BaseHighlight
    {
        public enum MaterialHighlightMode
        {
            /// <summary>Add the highlight material to the renderers materials array</summary>
            Layer,
            /// <summary>Replace the renderers materials with the highlight material</summary>
            Replace,
        }

        [SerializeField, Tooltip("How the highlight material will be applied to the renderer's material array.")]
        MaterialHighlightMode m_HighlightMode = MaterialHighlightMode.Replace;

        [SerializeField, Tooltip("Material to use for highlighting")]
        Material m_HighlightMaterial;

        Dictionary<int, Material[]> m_OriginalMaterials = new Dictionary<int, Material[]>();
        Dictionary<int, Material[]> m_HighlightMaterials = new Dictionary<int, Material[]>();

        /// <summary>
        /// How the highlight material will be applied to the renderer's material array.
        /// </summary>
        public MaterialHighlightMode highlightMode
        {
            get => m_HighlightMode;
            set => m_HighlightMode = value;
        }

        /// <summary>
        /// Material to use for highlighting
        /// </summary>
        public Material highlightMaterial
        {
            get => m_HighlightMaterial;
            set => m_HighlightMaterial = value;
        }

        void Start()
        {
            foreach (var rend in m_Renderers)
            {
                UpdateCachedMaterials(rend);
            }
        }

        protected override void Highlight(bool instant)
        {
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                UpdateCachedMaterials(rend);
                rend.materials = m_HighlightMaterials[rendererID];
            }
        }

        protected override void UnHighlight(bool instant)
        {
            foreach (var rend in m_Renderers)
            {
                var rendererID = rend.GetInstanceID();
                UpdateCachedMaterials(rend);
                rend.materials = m_OriginalMaterials[rendererID];
            }
        }

        protected override void OnRenderersRefreshed()
        {
            foreach (var rend in m_Renderers)
            {
                UpdateCachedMaterials(rend);
            }
        }

        void UpdateCachedMaterials(Renderer rend)
        {
            var rendererID = rend.GetInstanceID();
            if (m_OriginalMaterials.ContainsKey(rendererID))
                return;
            m_OriginalMaterials[rendererID] = rend.sharedMaterials;
            Material[] highlightMaterials;
            switch (m_HighlightMode)
            {
                case MaterialHighlightMode.Layer:
                    highlightMaterials = m_OriginalMaterials[rendererID].Concat(
                        Enumerable.Repeat(m_HighlightMaterial, 1)).ToArray();
                    break;
                case MaterialHighlightMode.Replace:
                    highlightMaterials = new Material[m_OriginalMaterials[rendererID].Length];
                    for (var i = 0; i < highlightMaterials.Length; i++)
                    {
                        highlightMaterials[i] = m_HighlightMaterial;
                    }

                    break;
                default:
                    Debug.LogError($"{gameObject.name} Material highlight has an invalid highlight mode {m_HighlightMode}. Original materials will be used.", this);
                    highlightMaterials = m_OriginalMaterials[rendererID];
                    break;
            }

            m_HighlightMaterials[rendererID] = highlightMaterials;
        }
    }
}
