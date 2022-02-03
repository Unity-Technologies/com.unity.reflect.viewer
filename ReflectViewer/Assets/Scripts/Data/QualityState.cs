using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    [Serializable, GeneratePropertyBag]
    public struct QualityState : IEquatable<QualityState>, IQualitySettingsDataProvider
    {
        public static readonly QualityState defaultData = new QualityState()
        {
            fpsThresholdQualityDecrease = 15,
            fpsThresholdQualityIncrease = 60,
            qualityLevel = QualitySettings.GetQualityLevel(),
            isAutomatic = true,
            lastQualityChangeTimestamp = -1
        };

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int fpsThresholdQualityDecrease { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int fpsThresholdQualityIncrease { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int qualityLevel { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isAutomatic { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float lastQualityChangeTimestamp { get; set; }

        public static bool operator ==(QualityState a, QualityState b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(QualityState a, QualityState b)
        {
            return !(a == b);
        }

        public bool Equals(QualityState other)
        {
            return qualityLevel == other.qualityLevel &&
                   isAutomatic == other.isAutomatic &&
                lastQualityChangeTimestamp == other.lastQualityChangeTimestamp &&
                fpsThresholdQualityDecrease == other.fpsThresholdQualityDecrease &&
                fpsThresholdQualityIncrease == other.fpsThresholdQualityIncrease;
        }

        public override bool Equals(object obj)
        {
            return obj is QualityState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = qualityLevel;
                hashCode = (hashCode * 397) ^ isAutomatic.GetHashCode();
                hashCode = (hashCode * 397) ^ fpsThresholdQualityDecrease;
                hashCode = (hashCode * 397) ^ fpsThresholdQualityIncrease;
                hashCode = (hashCode * 397) ^ lastQualityChangeTimestamp.GetHashCode();
                return hashCode;
            }
        }
    }

}
