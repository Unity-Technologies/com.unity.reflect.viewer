using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    public struct DragStateData : IEquatable<DragStateData>, IDragStateData
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public DragState dragState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 position { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int hashObjectDragged { get; set; }

        public bool Equals(DragStateData other)
        {
            return dragState.Equals(other.dragState) &&
                position.Equals(other.position) &&
                hashObjectDragged.Equals(other.hashObjectDragged);
        }

        public override bool Equals(object obj)
        {
            return obj is DragStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = dragState.GetHashCode();
                hashCode = (hashCode * 397) ^ position.GetHashCode();
                hashCode = (hashCode * 397) ^ hashObjectDragged.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DragStateData a, DragStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DragStateData a, DragStateData b)
        {
            return !(a == b);
        }
    }
}
