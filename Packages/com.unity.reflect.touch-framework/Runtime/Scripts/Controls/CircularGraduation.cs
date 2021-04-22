using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.TouchFramework
{
    public enum Orientation
    {
        Left,
        Right
    }

    // Scaler lets us support non linear scales.
    public interface IScaler
    {
        float ValueToAngle(float minVal, float maxVal, float angularRange, float value);
        float AngleToValue(float minVal, float maxVal, float angularRange, float angle);
        bool isDirty { get; }
        void MarkClean();
    }

    class DefaultScaler : IScaler
    {
        public float ValueToAngle(float minVal, float maxVal, float angularRange, float value)
        {
            if (Mathf.Approximately(maxVal, minVal))
                return 0;

            var normalizedValue = (maxVal - value) / (maxVal - minVal);
            return Mathf.Lerp(-angularRange, angularRange, normalizedValue);
        }

        public float AngleToValue(float minVal, float maxVal, float angularRange, float angle)
        {
            if (Mathf.Approximately(maxVal, minVal))
                return 0;

            var normalizedAngle = (angularRange - angle) / (angularRange * 2);
            return Mathf.Lerp(minVal, maxVal, normalizedAngle);
        }

        public bool isDirty => false;

        public void MarkClean() {}
    }

    public class CircularGraduation : IDisposable
    {
        const int k_ScaleResolution = 2048;
        // Note: no concurrent access assumed.
        static byte[] s_ScalePixels = new byte[k_ScaleResolution];
        // Shader uniforms.
        static readonly int k_ShaderScaleTex = Shader.PropertyToID("_ScaleTex");
        static readonly int k_ShaderAngularRange = Shader.PropertyToID("_AngularRange");
        static readonly int k_ShaderRotation = Shader.PropertyToID("_Rotation");
        static readonly int k_ShaderOrientation = Shader.PropertyToID("_Orientation");
        static readonly int k_ShaderRadiusMinMax = Shader.PropertyToID("_RadiusMinMax");
        static readonly int k_ShaderColor = Shader.PropertyToID("_Color");
        static readonly int k_ShaderAntialiasing = Shader.PropertyToID("_Antialiasing");

        public struct Parameters
        {
            public Vector2 radiusMinMax;
            public float antialiasing;
            public Color color;
            public float angularRange;
            public Orientation orientation;
            public float scaleDensityHint;
            public int entryLineWidth;
            public int lineWidth;
        }

        Material m_ScaleMaterial;
        Texture2D m_ScaleTexture;

        public void Dispose()
        {
            if (m_ScaleMaterial != null)
            {
                UnityEngine.Object.Destroy(m_ScaleMaterial);
                m_ScaleMaterial = null;
            }

            if (m_ScaleTexture != null)
            {
                UnityEngine.Object.Destroy(m_ScaleTexture);
                m_ScaleTexture = null;
            }
        }

        public Material Update(Parameters parms, IScaler scaler, Vector2 range, List<float> entries = null)
        {
            if (m_ScaleMaterial == null)
            {
                var circularGraduationMaterial = Resources.Load<Material>("Materials/CircularGraduation");
                Assert.IsNotNull(circularGraduationMaterial);
                m_ScaleMaterial = new Material(circularGraduationMaterial);
                m_ScaleMaterial.hideFlags = HideFlags.DontSave;
            }

            UpdateShaderUniforms(m_ScaleMaterial, parms);

            if (m_ScaleTexture == null)
            {
                // TODO: have buffer size match its circular projection in screen space.
                m_ScaleTexture = new Texture2D(k_ScaleResolution, 1, TextureFormat.Alpha8, false); // TODO use Alpha8
                m_ScaleTexture.filterMode = FilterMode.Bilinear;
                m_ScaleTexture.hideFlags = HideFlags.HideAndDontSave;
            }

            for (var i = 0; i < k_ScaleResolution; ++i)
                s_ScalePixels[i] = 0;

            if (entries != null)
            {
                foreach (var val in entries)
                {
                    var angle = scaler.ValueToAngle(range.x, range.y, parms.angularRange, val);
                    var normalized = (parms.angularRange - angle) / (parms.angularRange * 2);
                    RenderPoint(s_ScalePixels, normalized, parms.entryLineWidth, 1);
                }
            }

            if (Math.Abs(parms.scaleDensityHint) > Mathf.Epsilon)
            {
                RenderScale(s_ScalePixels, parms, scaler, range, 0.5f);
            }

            m_ScaleTexture.LoadRawTextureData(s_ScalePixels);
            m_ScaleTexture.Apply();
            m_ScaleMaterial.SetTexture(k_ShaderScaleTex, m_ScaleTexture);
            return m_ScaleMaterial;
        }

        static void RenderScale(byte[] target, Parameters parms, IScaler scaler, Vector2 range, float intensity)
        {
            var delta = Math.Abs(range.y - range.x) / Mathf.Max(1, parms.scaleDensityHint); // value delta between two points.

            // Find power of 10 above ptValDelta.
            var gradDelta = Mathf.Pow(10, Mathf.Ceil(Mathf.Log(delta) / Mathf.Log(10)));

            // Subdivide that power of 10 to get closer to target.
            while (gradDelta > 2 * delta)
                gradDelta *= 0.5f;

            var pos = Mathf.Ceil(range.x / gradDelta) * gradDelta;
            do
            {
                var angle = scaler.ValueToAngle(range.x, range.y, parms.angularRange, pos);
                var normalized = (parms.angularRange - angle) / (parms.angularRange * 2);
                RenderPoint(target, normalized, parms.lineWidth, intensity);
                pos += gradDelta;
            }
            while (pos < range.y);
        }

        static void RenderPoint(byte[] target, float normalizedPosition, float pxWidth, float intensity)
        {
            var centerPixel = normalizedPosition * (k_ScaleResolution - 1);
            var startPixel = (int)(Mathf.Max(0, Mathf.Floor(centerPixel - pxWidth * 0.5f)));
            var endPixel = (int)(Mathf.Min(k_ScaleResolution - 1, Mathf.Ceil(centerPixel + pxWidth * 0.5f)));
            var normalizedPxWidth = 1.0f / k_ScaleResolution;

            for (int i = startPixel; i <= endPixel; ++i)
            {
                var pxDist = Mathf.Abs(i - centerPixel);
                var antialiasing = Utilities.Smoothstep(pxWidth + normalizedPxWidth * 0.5f, pxWidth - normalizedPxWidth * 0.5f, pxDist);
                target[i] = (byte)Mathf.Max(target[i], 255 * intensity * antialiasing);
            }
        }

        static void UpdateShaderUniforms(Material mat, Parameters parms)
        {
            mat.SetFloat(k_ShaderAngularRange, parms.angularRange);
            mat.SetFloat(k_ShaderRotation, 90);
            mat.SetFloat(k_ShaderOrientation, parms.orientation == Orientation.Left ? 1 : -1);
            mat.SetVector(k_ShaderRadiusMinMax, parms.radiusMinMax);
            mat.SetFloat(k_ShaderAntialiasing, parms.antialiasing);
            mat.SetColor(k_ShaderColor, parms.color);
        }
    }
}
