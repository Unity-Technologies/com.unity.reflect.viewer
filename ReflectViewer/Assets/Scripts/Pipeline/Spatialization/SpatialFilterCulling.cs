using Unity.Collections;
using UnityEngine.Rendering;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class SpatialFilterCulling
    {
        // temporary variables to avoid garbage collection
        Vector3 m_Min,
            m_Max,
            m_Point,
            m_ScreenMin,
            m_ScreenMax,
            m_Normal,
            m_BoundsMin,
            m_BoundsMax,
            m_DirectionalLightForward,
            m_DirectionalLightForwardOffset,
            m_CamPos;

        Vector4 m_Point4;
        readonly int m_DepthCullingResolution;
        Bounds m_Bounds = new Bounds();

        float m_Distance,
            m_Dot,
            m_CamNearClipPlane,
            m_CamFarClipPlane,
            m_Z,
            m_Dz,
            m_ScreenAreaRatio,
            m_DepthOffset,
            m_AvoidCullingWithinSqrDistance,
            m_ShadowDistance;

        float[,] m_DepthMapSlice, m_PrevDepthMapSlice;
        readonly float[][,] m_DepthMap;
        Matrix4x4 m_CamWorldToViewportMatrix;
        NativeArray<float> m_DepthTextureArray;
        NativeArray<Color> m_DepthTextureArrayNoAsync;
        readonly RenderTexture m_DepthRenderTexture;
        readonly Texture2D m_DepthTexture;
        readonly Rect m_DepthTextureRect;
        bool m_IsCapturingDepthMap, m_UseShadowCullingAvoidance;
        float m_MinScreenAreaRatioMesh;

        readonly bool m_SupportsAsyncGpuReadback;
        Camera m_Camera;
        Transform m_CameraTransform;
        readonly Plane[] m_FrustumPlanes = new Plane[6];
        readonly Vector3[] m_FrustumNormals = new Vector3[6];
        readonly float[] m_FrustumDistances = new float[6];
        Plane m_CamForwardPlane = new Plane();
        readonly Transform m_DirectionalLight;

        readonly SpatialFilterCullingSettings m_Settings;

        public SpatialFilterCulling(SpatialFilterCullingSettings cullingSettings, IExposedPropertyTable resolver)
        {
            m_Settings = cullingSettings;

            m_DirectionalLight = m_Settings.directionalLight.Resolve(resolver);

            m_SupportsAsyncGpuReadback = SystemInfo.supportsAsyncGPUReadback;
            m_DepthRenderTexture = m_SupportsAsyncGpuReadback
                ? m_Settings.depthRenderTexture
                : m_Settings.depthRenderTextureNoAsync;

            var depthSize = Mathf.Min(m_DepthRenderTexture.width, m_DepthRenderTexture.height);
            m_DepthCullingResolution = (int) Mathf.Log(depthSize, 2);
            m_DepthMap = new float[m_DepthCullingResolution + 1][,];
            for (int i = 0, size = 1; i <= m_DepthCullingResolution; ++i, size <<= 1)
                m_DepthMap[i] = new float[size, size];

            if (m_SupportsAsyncGpuReadback)
                return;

            // depth culling is disabled by default on devices that don't support AsyncGPUReadback
            // but still init the slower (non-async) version's assets in case a user wants to enable it anyway
            m_DepthTexture = new Texture2D(depthSize, depthSize, TextureFormat.RGBAFloat, false);
            m_DepthTextureRect = new Rect(0, 0, depthSize, depthSize);
        }

        public void OnUpdate()
        {
            if (m_IsCapturingDepthMap)
                return;

            CalculateCameraData();

            if (!m_Settings.useDepthCulling)
                return;

            m_IsCapturingDepthMap = true;
            CaptureDepthMap();
        }

        public void SetCamera(Camera camera)
        {
            m_Camera = camera;
            m_CameraTransform = camera.transform;
        }

        public bool IsVisible(ISpatialObject obj)
        {
            m_BoundsMin = obj.min;
            m_BoundsMax = obj.max;

            if (m_Settings.useDistanceCullingAvoidance)
            {
                m_Bounds.SetMinMax(m_BoundsMin, m_BoundsMax);
                if (m_Bounds.SqrDistance(m_CamPos) < m_AvoidCullingWithinSqrDistance)
                    return true;
            }

            if (m_UseShadowCullingAvoidance) // use member because we check if shadows are enabled in QualitySettings
            {
                // expand bounds to include potential shadow casting area
                m_DirectionalLightForwardOffset = m_DirectionalLightForward * m_ShadowDistance;
                m_BoundsMin = Vector3.Min(m_BoundsMin, m_BoundsMin + m_DirectionalLightForwardOffset);
                m_BoundsMax = Vector3.Max(m_BoundsMax, m_BoundsMax + m_DirectionalLightForwardOffset);
            }

            // do the frustum check, necessary for size and depth culling since both techniques are based on the screen projection
            if (!IsInCameraFrustum(m_BoundsMin, m_BoundsMax))
                return false;

            // early exit before screen projection if it won't be used
            if (!m_Settings.useSizeCulling && !m_Settings.useDepthCulling)
                return true;

            // if screen rect is invalid, safer to assume object is visible
            if (!TryCalculateScreenRect(m_BoundsMin, m_BoundsMax, out m_ScreenMin, out m_ScreenMax))
                return true;

            if (m_Settings.useSizeCulling)
            {
                m_ScreenAreaRatio = (m_ScreenMax.x - m_ScreenMin.x) * (m_ScreenMax.y - m_ScreenMin.y);
                if (m_ScreenAreaRatio < m_MinScreenAreaRatioMesh)
                    return false;
            }

            return !m_Settings.useDepthCulling || !IsDepthOccluded(m_ScreenMin, m_ScreenMax);
        }

        void CalculateCameraData()
        {
            GeometryUtility.CalculateFrustumPlanes(m_Camera, m_FrustumPlanes);
            for (var i = 0; i < m_FrustumPlanes.Length; ++i)
            {
                var plane = m_FrustumPlanes[i];
                var normal = plane.normal;
                plane.Translate(normal * m_Settings.cameraFrustumNormalOffset);
                m_FrustumNormals[i] = normal;
                m_FrustumDistances[i] = plane.distance;
            }

            m_CamWorldToViewportMatrix = m_Camera.projectionMatrix * m_CameraTransform.worldToLocalMatrix;
            m_CamNearClipPlane = m_Camera.nearClipPlane;
            m_CamFarClipPlane = m_Camera.farClipPlane;
            m_CamPos = m_CameraTransform.position;
            m_CamForwardPlane.SetNormalAndPosition(m_CameraTransform.forward, m_CamPos);
            m_DirectionalLightForward = m_DirectionalLight.forward;

            if (m_Settings.useDistanceCullingAvoidance)
                m_AvoidCullingWithinSqrDistance = m_Settings.avoidCullingWithinDistance * m_Settings.avoidCullingWithinDistance;

            m_UseShadowCullingAvoidance = m_Settings.useShadowCullingAvoidance && QualitySettings.shadows != ShadowQuality.Disable;
            m_ShadowDistance = QualitySettings.shadowDistance;

            if (m_Settings.useSizeCulling)
                m_MinScreenAreaRatioMesh = Mathf.Pow(10, -m_Settings.minimumScreenAreaRatioMesh);
        }

        bool TryCalculateScreenRect(Vector3 boundsMin, Vector3 boundsMax, out Vector3 min, out Vector3 max)
        {
            m_Min = min = boundsMin;
            m_Max = max = boundsMax;

            for (byte i = 0; i < 8; ++i)
            {
                // calculate each corner of the bounding box
                m_Point.x = (i >> 2) % 2 > 0 ? m_Max.x : m_Min.x;
                m_Point.y = (i >> 1) % 2 > 0 ? m_Max.y : m_Min.y;
                m_Point.z = i % 2 > 0 ? m_Max.z : m_Min.z;

                // screen rect will be invalid if a point is behind the camera due to the projection matrix
                // GetDistanceToPoint is faster here than GetSide since it uses floats instead of doubles internally
                if (m_CamForwardPlane.GetDistanceToPoint(m_Point) < 0f)
                    return false;

                m_Point = WorldToViewportPoint(m_Point);

                if (i == 0)
                {
                    min = max = m_Point;
                    continue;
                }

                if (m_Point.x < min.x) min.x = m_Point.x;
                if (m_Point.y < min.y) min.y = m_Point.y;
                if (m_Point.z < min.z) min.z = m_Point.z;
                if (m_Point.x > max.x) max.x = m_Point.x;
                if (m_Point.y > max.y) max.y = m_Point.y;
                if (m_Point.z > max.z) max.z = m_Point.z;
            }

            // normalize the z value to match depth shader value
            min.z = Mathf.InverseLerp(m_CamNearClipPlane, m_CamFarClipPlane, min.z);
            max.z = Mathf.InverseLerp(m_CamNearClipPlane, m_CamFarClipPlane, max.z);

            return true;
        }

        Vector3 WorldToViewportPoint(Vector3 point)
        {
            // convert to Vector4 for Matrix4x4 multiplication
            m_Point4.Set(point.x, point.y, point.z, 1f);
            m_Point4 = m_CamWorldToViewportMatrix * m_Point4;

            // normalize
            point = m_Point4;
            point /= -m_Point4.w;

            // convert from clip to Unity viewport space, z is distance from camera
            point.x += 1f;
            point.x /= 2f;
            point.y += 1f;
            point.y /= 2f;
            point.z = -m_Point4.w;

            return point;
        }

        bool IsInCameraFrustum(Vector3 min, Vector3 max)
        {
            for (var i = 0; i < m_FrustumPlanes.Length; ++i)
            {
                m_Normal = m_FrustumNormals[i];
                m_Distance = m_FrustumDistances[i];

                // get the closest bounding box vertex along each plane's normal
                m_Min.x = m_Normal.x < 0 ? min.x : max.x;
                m_Min.y = m_Normal.y < 0 ? min.y : max.y;
                m_Min.z = m_Normal.z < 0 ? min.z : max.z;

                // the object is hidden if a closest point is behind any plane
                m_Dot = m_Normal.x * m_Min.x + m_Normal.y * m_Min.y + m_Normal.z * m_Min.z;
                if (m_Dot + m_Distance < 0)
                    return false;
            }

            return true;
        }

        void CaptureDepthMap()
        {
            Graphics.Blit(Texture2D.whiteTexture, m_DepthRenderTexture, m_Settings.depthRenderMaterial);

            if (!m_SupportsAsyncGpuReadback)
            {
                RenderTexture.active = m_DepthRenderTexture;
                m_DepthTexture.ReadPixels(m_DepthTextureRect, 0, 0);
                RenderTexture.active = null;
                m_DepthTextureArrayNoAsync = m_DepthTexture.GetRawTextureData<Color>();
                GenerateDepthMaps();
                return;
            }

            // send the gpu request
            AsyncGPUReadback.Request(m_DepthRenderTexture, 0, CaptureDepthMapAsyncCallback);
        }

        void CaptureDepthMapAsyncCallback(AsyncGPUReadbackRequest request)
        {
            if (!request.done || request.hasError)
            {
                m_IsCapturingDepthMap = false;
                return;
            }

            m_DepthTextureArray = request.GetData<float>();

            GenerateDepthMaps();
        }

        void GenerateDepthMaps()
        {
            m_DepthOffset = m_Settings.depthOffset;

            // save the highest resolution depth map, then sample down
            for (int i = m_DepthCullingResolution, size = 1 << i; i >= 0; --i, size >>= 1)
            {
                var isFirstIteration = i == m_DepthCullingResolution;
                m_DepthMapSlice = m_DepthMap[i];
                m_PrevDepthMapSlice = isFirstIteration ? m_DepthMap[i] : m_DepthMap[i + 1];

                for (int x = 0, dx = 0; x < size; ++x, dx += 2)
                {
                    for (int y = 0, dy = 0; y < size; ++y, dy += 2)
                    {
                        if (isFirstIteration)
                        {
                            // add m_DepthOffset to reduce flickering since the depth map is not at the full screen resolution
                            m_DepthMapSlice[x, y] = m_DepthOffset + (m_SupportsAsyncGpuReadback
                                ? m_DepthTextureArray[x + y * size]
                                // the depth is stored in the R channel for the non-async array
                                : m_DepthTextureArrayNoAsync[x + y * size].r);
                            continue;
                        }

                        // we need the largest depth value rather than the average, else we'd just use the texture mip maps
                        // doing manual comparisons to avoid garbage allocations in Mathf.Max()
                        m_Z = m_PrevDepthMapSlice[dx, dy];

                        m_Dz = m_PrevDepthMapSlice[dx + 1, dy];
                        if (m_Dz > m_Z)
                            m_Z = m_Dz;

                        m_Dz = m_PrevDepthMapSlice[dx, dy + 1];
                        if (m_Dz > m_Z)
                            m_Z = m_Dz;

                        m_Dz = m_PrevDepthMapSlice[dx + 1, dy + 1];
                        if (m_Dz > m_Z)
                            m_Z = m_Dz;

                        m_DepthMapSlice[x, y] = m_Z;
                    }
                }
            }

            m_IsCapturingDepthMap = false;
        }

        bool IsDepthOccluded(Vector3 min, Vector3 max)
        {
            // crop the rect so it fits in screen space
            if (min.x < 0f)
                min.x = 0f;

            if (max.x > 1f)
                max.x = 1f;

            if (min.y < 0f)
                min.y = 0f;

            if (max.y > 1f)
                max.y = 1f;

            // recursion
            return IsDepthOccluded(min, max, 0);
        }

        bool IsDepthOccluded(Vector3 min, Vector3 max, int resolution, int x = 0, int y = 0)
        {
            if (min.z > m_DepthMap[resolution][x, y])
                return true;

            ++resolution;

            if (resolution > m_DepthCullingResolution)
                return false;

            var mapSize = 1 << resolution;
            x <<= 1;
            y <<= 1;

            // find the indices for the 4 rect points in the depth map
            var xMin = Mathf.Max((int) (mapSize * min.x), x);
            var xMax = Mathf.Min((int) (mapSize * max.x), x + 1);
            var yMin = Mathf.Max((int) (mapSize * min.y), y);
            var yMax = Mathf.Min((int) (mapSize * max.y), y + 1);

            for (x = xMin; x <= xMax; ++x)
                for (y = yMin; y <= yMax; ++y)
                    if (!IsDepthOccluded(min, max, resolution, x, y))
                        return false;

            return true;
        }
    }
}
