using System;
using Unity.XRTools.Rendering;
using Unity.XRTools.Utils;
using UnityEngine;

namespace Unity.SpatialFramework.UI
{
    /// <summary>
    /// Draws a circle in space with a line renderer. The circle is controlled via the ICircle interface
    /// </summary>
    public class SpatialCircle : MonoBehaviour, ICircle
    {
        [SerializeField, Tooltip("The line renderer for drawing the circle")]
        XRLineRenderer m_LineRenderer;

        /// <summary>
        /// The line renderer for drawing the circle
        /// </summary>
        public XRLineRenderer lineRenderer
        {
            get => m_LineRenderer;
            set => m_LineRenderer = value;
        }

        Func<Vector3> ICircle.getCenter { get; set; }

        Func<Vector3> ICircle.getNormal { get; set; }

        Func<Vector3> ICircle.getRadius { get; set; }

        Vector3 m_PrevCenter, m_PrevRadius, m_PrevNormal;

        void Update()
        {
            UpdateCircle();
        }

        void UpdateCircle()
        {
            // Get updated values and check if changed
            var circle = ((ICircle)this);
            var center = circle.getCenter();
            var radius = circle.getRadius();
            var normal = circle.getNormal();

            // Calculate the number of vertices to use for the circle based on main camera world to screen point
            var mainCamera = Camera.main;
            var circleVertexCount = 32;
            if (mainCamera != null)
            {
                var centerPoint = mainCamera.WorldToViewportPoint(center);
                var edgePoint = mainCamera.WorldToViewportPoint(center + radius);
                const float resolution = 512;
                const int minVertices = 12;
                const int maxVertices = 128;
                var viewSize = Vector2.Distance(centerPoint, edgePoint); // XY distance only
                circleVertexCount = Mathf.CeilToInt(viewSize * resolution);
                circleVertexCount = Mathf.Clamp(circleVertexCount, minVertices, maxVertices);
            }

            // Set the line renderer points
            m_LineRenderer.SetVertexCount(circleVertexCount);
            for (var i = 0; i < circleVertexCount; i++)
            {
                var time = ((float)i) / (circleVertexCount - 1);
                m_LineRenderer.SetPosition(i, center + Quaternion.AngleAxis(time * 360f, normal) * radius);
            }
        }

        bool IPooledUI.active
        {
            set
            {
                if (value && !gameObject.activeSelf) // Update before activating so scaler gets updated position immediately
                    Update();

                gameObject.SetActive(value);
            }
            get => gameObject.activeSelf;
        }

        void IPooledUI.Destroy()
        {
            UnityObjectUtils.Destroy(gameObject);
        }
    }
}
