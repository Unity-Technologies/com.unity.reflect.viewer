using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using Unity.Reflect.Actors;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DebugOptionsData : IEquatable<DebugOptionsData>, IDebugOptionDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool gesturesTrackingEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool ARAxisTrackingEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 spatialPriorityWeights { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool useDebugBoundingBoxMaterials { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool useCulling { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool useSpatialManifest { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool useHlods{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int hlodDelayMode { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int hlodPrioritizer { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int targetFps { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool showActorDebug { get; set; }

        public static readonly DebugOptionsData defaultData = new DebugOptionsData()
        {
            gesturesTrackingEnabled = false,
            ARAxisTrackingEnabled = false,
            spatialPriorityWeights = Vector3.one,
            useDebugBoundingBoxMaterials = false,
            useCulling = true,
            useSpatialManifest = true,
            useHlods = true,
            hlodDelayMode = 0,
            hlodPrioritizer = 0,
            targetFps = SyncTreeActor.Settings.k_DefaultTargetFps,
            showActorDebug = false,
        };

        public DebugOptionsData(bool gesturesTrackingEnabled, bool ARAxisTrackingEnabled, Vector3 spatialPriorityWeights,
            bool useDebugBoundingBoxMaterials, bool useCulling, bool useSpatialManifest, bool useHlods, int hlodDelayMode,
            int hlodPrioritizer, int targetFps, bool showActorDebug, SetOrbitTypeAction.OrbitType touchOrbitType)
        {
            this.gesturesTrackingEnabled = gesturesTrackingEnabled;
            this.ARAxisTrackingEnabled = ARAxisTrackingEnabled;
            this.spatialPriorityWeights = spatialPriorityWeights;
            this.useDebugBoundingBoxMaterials = useDebugBoundingBoxMaterials;
            this.useCulling = useCulling;
            this.useSpatialManifest = useSpatialManifest;
            this.useHlods = useHlods;
            this.hlodDelayMode = hlodDelayMode;
            this.hlodPrioritizer = hlodPrioritizer;
            this.targetFps = targetFps;
            this.showActorDebug = showActorDebug;
        }

        public static DebugOptionsData Validate(DebugOptionsData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("gesturesTrackingEnabled{0}, ARAxisTrackingEnabled{1}, spatialPriorityWeights{2}, " +
                            "useDebugBoundingBoxMaterials{3}, useCulling{4}, useStaticBatching{5}, useSpatialManifest{6}, " +
                            "useHlods{7}, hlodDelayMode{8}, hlodPrioritizer{9}, targetFps{10}, showActorDebug{11}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                gesturesTrackingEnabled,
                ARAxisTrackingEnabled,
                spatialPriorityWeights,
                useDebugBoundingBoxMaterials,
                useCulling,
                useSpatialManifest,
                useHlods,
                hlodDelayMode,
                hlodPrioritizer,
                targetFps,
                showActorDebug);
        }

        public bool Equals(DebugOptionsData other)
        {
            return gesturesTrackingEnabled == other.gesturesTrackingEnabled &&
                   ARAxisTrackingEnabled == other.ARAxisTrackingEnabled &&
                   spatialPriorityWeights == other.spatialPriorityWeights &&
                   useDebugBoundingBoxMaterials == other.useDebugBoundingBoxMaterials &&
                   useCulling == other.useCulling &&
                   useSpatialManifest == other.useSpatialManifest &&
                   useHlods == other.useHlods &&
                   hlodDelayMode == other.hlodDelayMode &&
                   hlodPrioritizer == other.hlodPrioritizer &&
                   targetFps == other.targetFps &&
                   showActorDebug == other.showActorDebug;
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
                hashCode = (hashCode * 397) ^ useSpatialManifest.GetHashCode();
                hashCode = (hashCode * 397) ^ useHlods.GetHashCode();
                hashCode = (hashCode * 397) ^ hlodDelayMode.GetHashCode();
                hashCode = (hashCode * 397) ^ hlodPrioritizer.GetHashCode();
                hashCode = (hashCode * 397) ^ targetFps.GetHashCode();
                hashCode = (hashCode * 397) ^ showActorDebug.GetHashCode();
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

    [Serializable, GeneratePropertyBag]
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
