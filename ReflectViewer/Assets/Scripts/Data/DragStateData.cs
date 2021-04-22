using System;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    public enum DragState
    {
        None = 0,
        OnStart = 1,
        OnUpdate = 2,
        OnEnd = 3
    }

    [Serializable]
    public struct DragStateData : IEquatable<DragStateData>
    {
        public DragState dragState;
        public Vector3 position;
        public int hashObjectDragged;
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
