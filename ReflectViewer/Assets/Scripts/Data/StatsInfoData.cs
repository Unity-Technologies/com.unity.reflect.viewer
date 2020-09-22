using System;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct StatsInfoData : IEquatable<StatsInfoData>
    {
        public int fpsMax;
        public int fpsAvg;
        public int fpsMin;

        public StreamCountData assetsCountData;
        public StreamCountData instancesCountData;
        public StreamCountData gameObjectsCountData;

        public static bool operator ==(StatsInfoData a, StatsInfoData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(StatsInfoData a, StatsInfoData b)
        {
            return !(a == b);
        }

        public bool Equals(StatsInfoData other)
        {
            return fpsMax == other.fpsMax && fpsAvg == other.fpsAvg && fpsMin == other.fpsMin &&
                assetsCountData.Equals(other.assetsCountData) &&
                instancesCountData.Equals(other.instancesCountData) &&
                gameObjectsCountData.Equals(other.gameObjectsCountData);
        }

        public override bool Equals(object obj)
        {
            return obj is StatsInfoData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = fpsMax;
                hashCode = (hashCode * 397) ^ fpsAvg;
                hashCode = (hashCode * 397) ^ fpsMin;
                hashCode = (hashCode * 397) ^ assetsCountData.GetHashCode();
                hashCode = (hashCode * 397) ^ instancesCountData.GetHashCode();
                hashCode = (hashCode * 397) ^ gameObjectsCountData.GetHashCode();
                return hashCode;
            }
        }
    }
}
