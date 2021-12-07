using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    [Serializable, GeneratePropertyBag]
    public struct ApplicationSettingsStateData : IEquatable<ApplicationSettingsStateData>, IApplicationSettingsDataProvider<QualityState>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public QualityState qualityStateData { get; set; }

        public bool Equals(ApplicationSettingsStateData other)
        {
            return qualityStateData.Equals(other.qualityStateData);
        }

        public override bool Equals(object obj)
        {
            return obj is ApplicationSettingsStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = qualityStateData.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ApplicationSettingsStateData a, ApplicationSettingsStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ApplicationSettingsStateData a, ApplicationSettingsStateData b)
        {
            return !(a == b);
        }
    }
}
