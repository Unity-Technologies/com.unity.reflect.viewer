using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class BoundsEvent : UnityEvent<Bounds> { }

    [Serializable]
    public class SpatialFilterSettings
    {
        [Tooltip("Disable to bypass this node")]
        public bool isActive = true;
        [Tooltip("Max objects allowed in each node before forcing it to split")]
        public int maxPerNode = 16;
        [Tooltip("Min objects required in each node during a split")]
        public int minPerNode = 6;

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
        public bool displayUnloadedObjectBoundingBoxes = true;
        public bool displayOnlyBoundingBoxes = false;
        public GameObject boundingBoxPrefab = null;
        public ExposedReference<Transform> boundingBoxRoot;

        [Header("Selection")]
        public int selectedObjectsMax = 10;

        [HideInInspector]
        public BoundsEvent onGlobalBoundsCalculated;

        [Header("Debug")]
        public bool drawNodes = false;
        public Gradient drawNodesGradient = null;
        public bool drawObjects = false;
        public Gradient drawObjectsGradient;
        [Range(0, 20)] public int drawMaxDepth = 20;
    }
}
