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
        [Range(1000, 10000)] public int visibleObjectsMax = 10000;
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
