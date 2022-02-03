using System;
using Unity.XRTools.Rendering;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Creates a line in space. The line is controlled via the ILine interface
    /// </summary>
    public class SpatialLine : MonoBehaviour, ILine
    {
        [SerializeField, Tooltip("The line renderer for drawing the line")]
        XRLineRenderer m_LineRenderer;

        /// <summary>
        /// The line renderer for drawing the line
        /// </summary>
        public XRLineRenderer lineRenderer
        {
            get => m_LineRenderer;
            set => m_LineRenderer = value;
        }

        int ILine.vertexCount
        {
            set => m_LineRenderer.SetVertexCount(value);
            get => m_LineRenderer.GetVertexCount();
        }

        Func<Vector3[]> getLinePositions { get; set; }

        Func<Vector3[]> ILine.getLinePositions
        {
            get => this.getLinePositions;
            set => this.getLinePositions = value;
        }

        void Update()
        {
            m_LineRenderer.SetPositions(getLinePositions());
        }

        bool IPooledUI.active
        {
            get => gameObject.activeSelf;
            set => gameObject.SetActive(value);
        }

        void IPooledUI.Destroy()
        {
            UnityObjectUtils.Destroy(gameObject);
        }
    }
}
