using System;
using UnityEngine.Events;
using UnityEngine.Reflect.Pipeline;
using UnityEngine.Serialization;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class BoundingBoxFilterSettings
    {
        public Bounds m_GlobalBoundingBox;

        [Serializable]
        public class BoundEvent : UnityEvent<Bounds> { }

        [Header("Events")]
        public BoundEvent onBoundsCalculated;
    }
}
