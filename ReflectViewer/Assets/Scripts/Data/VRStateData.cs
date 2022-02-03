using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;


namespace Unity.Reflect.Viewer
{
    [Serializable, GeneratePropertyBag]
    public struct VRStateData : IEquatable<VRStateData>, IVREnableDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool VREnable { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Transform RightController { get; set; }

        public bool Equals(VRStateData other)
        {
            return VREnable.Equals(other.VREnable) &&
                RightController == other.RightController;
        }

        public override bool Equals(object obj)
        {
            return obj is VRStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = VREnable.GetHashCode();
                hashCode = (hashCode * 397) ^ RightController.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(VRStateData a, VRStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(VRStateData a, VRStateData b)
        {
            return !(a == b);
        }
    }
}
