using System;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class BoundsEvent : UnityEvent<Bounds> { }

    public enum StreamState
    {
        Asset,
        FilteredAsset,
        Instance,
        InstanceData,
        GameObject,
        Removed,
        Invalid
    }

    [Serializable]
    public struct BoundingBoxMaterial
    {
        public StreamState streamState;
        public Material material;
    }

    [Serializable]
    public class BoundingBoxSettings
    {
        public bool displayOnlyBoundingBoxes = false;
        public GameObject boundingBoxPrefab = null;
        public ExposedReference<Transform> boundingBoxRoot;
        public int initialBoundingBoxPoolSize = 10000;
        public bool useStaticBatching;

        public bool useDebugMaterials;
        public BoundingBoxMaterial[] defaultBoundingBoxMaterials;
        public BoundingBoxMaterial[] debugBoundingBoxMaterials;

        [HideInInspector]
        public BoundsEvent onGlobalBoundsCalculated;
    }
}
