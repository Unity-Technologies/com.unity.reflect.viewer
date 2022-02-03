using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{
    [Serializable, GeneratePropertyBag]
    public struct ForceNavigationModeData : IEquatable<ForceNavigationModeData>, IForceNavigationModeDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetForceNavigationModeAction.ForceNavigationModeTrigger navigationMode { get; set; }

        public bool Equals(ForceNavigationModeData other)
        {
            return navigationMode.Equals(other.navigationMode);
        }

        public override bool Equals(object obj)
        {
            return obj is ForceNavigationModeData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = navigationMode.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ForceNavigationModeData a, ForceNavigationModeData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ForceNavigationModeData a, ForceNavigationModeData b)
        {
            return !(a == b);
        }
    }
}
