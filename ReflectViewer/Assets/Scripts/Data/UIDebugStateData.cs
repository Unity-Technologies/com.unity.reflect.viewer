using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DebugOptionsData : IEquatable<DebugOptionsData>
    {
        public bool gesturesTrackingEnabled;
        public bool ARAxisTrackingEnabled;
        public Vector3 spatialPriorityWeights;
        public bool useDebugBoundingBoxMaterials;
        public bool useCulling;
        public bool useStaticBatching;

        public static readonly DebugOptionsData defaultData = new DebugOptionsData()
        {
            gesturesTrackingEnabled = false,
            ARAxisTrackingEnabled = false,
            spatialPriorityWeights = Vector3.one,
            useDebugBoundingBoxMaterials = false,
            useCulling = true,
            useStaticBatching = false,
        };

        public DebugOptionsData(bool gesturesTrackingEnabled, bool ARAxisTrackingEnabled, Vector3 spatialPriorityWeights,
            bool useDebugBoundingBoxMaterials, bool useCulling, bool useStaticBatching)
        {
            this.gesturesTrackingEnabled = gesturesTrackingEnabled;
            this.ARAxisTrackingEnabled = ARAxisTrackingEnabled;
            this.spatialPriorityWeights = spatialPriorityWeights;
            this.useDebugBoundingBoxMaterials = useDebugBoundingBoxMaterials;
            this.useCulling = useCulling;
            this.useStaticBatching = useStaticBatching;
        }

        public static DebugOptionsData Validate(DebugOptionsData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("gesturesTrackingEnabled{0}, ARAxisTrackingEnabled{1}, spatialPriorityWeights{2}, " +
                            "useDebugBoundingBoxMaterials{3}, useCulling{4}, useStaticBatching{5}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                gesturesTrackingEnabled,
                ARAxisTrackingEnabled,
                spatialPriorityWeights,
                useDebugBoundingBoxMaterials,
                useCulling,
                useStaticBatching);
        }

        public bool Equals(DebugOptionsData other)
        {
            return gesturesTrackingEnabled == other.gesturesTrackingEnabled &&
                   ARAxisTrackingEnabled == other.ARAxisTrackingEnabled &&
                   spatialPriorityWeights == other.spatialPriorityWeights &&
                   useDebugBoundingBoxMaterials == other.useDebugBoundingBoxMaterials &&
                   useCulling == other.useCulling &&
                   useStaticBatching == other.useStaticBatching;
        }

        public override bool Equals(object obj)
        {
            return obj is DebugOptionsData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = gesturesTrackingEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ ARAxisTrackingEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ spatialPriorityWeights.GetHashCode();
                hashCode = (hashCode * 397) ^ useDebugBoundingBoxMaterials.GetHashCode();
                hashCode = (hashCode * 397) ^ useCulling.GetHashCode();
                hashCode = (hashCode * 397) ^ useStaticBatching.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DebugOptionsData a, DebugOptionsData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DebugOptionsData a, DebugOptionsData b)
        {
            return !(a == b);
        }
    }

    [Serializable]
    public struct UIDebugStateData : IEquatable<UIDebugStateData>
    {
        public StatsInfoData statsInfoData;
        public DebugOptionsData debugOptionsData;

        public bool Equals(UIDebugStateData other)
        {
            return statsInfoData.Equals(other.statsInfoData) &&
                debugOptionsData.Equals(other.debugOptionsData);
        }

        public override bool Equals(object obj)
        {
            return obj is UIDebugStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = statsInfoData.GetHashCode();
                hashCode = (hashCode * 397) ^ debugOptionsData.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(UIDebugStateData a, UIDebugStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIDebugStateData a, UIDebugStateData b)
        {
            return !(a == b);
        }
    }
}
