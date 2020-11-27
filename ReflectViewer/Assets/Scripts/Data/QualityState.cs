using System;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    [Serializable]
    public struct QualityState : IEquatable<QualityState>
    {
        public static readonly QualityState defaultData = new QualityState()
        {
            fpsThresholdQualityDecrease = 24,
            fpsThresholdQualityIncrease = 60,
            qualityLevel = QualitySettings.GetQualityLevel(),
            lastQualityChangeTimestamp = -1
        };

        public int fpsThresholdQualityDecrease;
        public int fpsThresholdQualityIncrease;
        public int qualityLevel;
        public float lastQualityChangeTimestamp;

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
                hashCode = (hashCode * 397) ^ fpsThresholdQualityDecrease;
                hashCode = (hashCode * 397) ^ fpsThresholdQualityIncrease;
                hashCode = (hashCode * 397) ^ lastQualityChangeTimestamp.GetHashCode();
                return hashCode;
            }
        }
    }

}
