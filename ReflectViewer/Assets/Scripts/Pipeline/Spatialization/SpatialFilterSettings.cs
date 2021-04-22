using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class MemoryLevelEvent : UnityEvent<MemoryLevel> { }

    [Serializable]
    public class SpatialFilterSettings
    {
        [Tooltip("Disable to bypass this node")]
        public bool isActive = true;

        [Header("RTree")]
        [Tooltip("Max objects allowed in each node before forcing it to split")]
        public int maxPerNode = 16;
        [Tooltip("Min objects required in each node during a split")]
        public int minPerNode = 6;

        [Header("Streaming")]
        [Tooltip("Controls # objects streamed per frame based on framerate")]
        [Range(0f, 1f)] public float streamFactor = 1f;
        [Tooltip("Continue to load hidden objects in the background when visible objects are all processed")]
        public bool loadHiddenObjects = true;

        [Header("Priority")]
        [Range(0f, 10f)] public float priorityWeightAngle = 10f;
        [Range(0f, 10f)] public float priorityWeightDistance = 10f;
        [Range(0f, 10f)] public float priorityWeightSize = 10f;

        [Header("Visibility")]
        [Tooltip("Max objects visible at once, including bounding boxes. This should probably be at least 1000. Default is 10000. " +
                 "Values up to 100000 or more can be used, however AT YOUR OWN RISK since they may cause performance drops.")]
        public int visibleObjectsMax = 10000;
        [Tooltip("If enabled, the maximum number of displayed objects will dynamically change between 0 and visibleObjectsMax. Use this" +
            " feature if you develop on mobile device, where RAM is limited.")]
        public bool useDynamicNbVisibleObjectsMobile = true;
        [Tooltip("If enabled, the maximum number of displayed objects will dynamically change between 0 and visibleObjectsMax. This " +
            "generally can be turned off for viewers running on desktop.")]
        public bool useDynamicNbVisibleObjectsDesktop = false;

        [Header("Culling")]
        public bool useCulling = true;
        public SpatialFilterCullingSettings cullingSettings;

        [Header("Selection")]
        public int selectedObjectsMax = 10;

        [Header("Debug")]
        public bool drawNodes = false;
        [Tooltip("Node gradient is based on the node depth in the spatial collection (tree)")]
        public Gradient drawNodesGradient = null;
        public bool drawObjects = false;
        [Tooltip("Object gradient is based on streaming priority")]
        public Gradient drawObjectsGradient;
        [Range(0, 20)] public int drawMaxDepth = 20;

        [HideInInspector]
        public MemoryLevelEvent memoryLevelChanged;
    }

    [Serializable]
    public class SpatialFilterCullingSettings
    {
        [Header("Distance")]
        public bool useDistanceCullingAvoidance = true;
        public float avoidCullingWithinDistance = 5f;

        [Header("Shadows")]
        public bool useShadowCullingAvoidance = true;
        public ExposedReference<Transform> directionalLight;

        [Header("Frustum")]
        public float cameraFrustumNormalOffset = 0f;

        [Header("Size")]
        public bool useSizeCulling = true;
        [Tooltip("The actual size used is 10^-value")]
        [Range(0, 10)] public float minimumScreenAreaRatioMesh = 4;

        [Header("Depth")]
        public bool useDepthCulling = true;
        public float depthOffset = 0.001f;

        [Header("Assets")]
        public RenderTexture depthRenderTexture = null;
        public RenderTexture depthRenderTextureNoAsync = null;
        public Material depthRenderMaterial = null;
    }
}
