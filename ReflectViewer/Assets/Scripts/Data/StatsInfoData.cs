using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    public struct StatsInfoData : IEquatable<StatsInfoData>, IStatsInfoStreamDataProvider<StreamCountData>, IStatsInfoFPSDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int fpsMax { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int fpsAvg { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int fpsMin { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public StreamCountData assetsCountData { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public StreamCountData instancesCountData { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public StreamCountData gameObjectsCountData { get; set; }

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
