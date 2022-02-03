using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct SkyboxData : IEquatable<SkyboxData>
    {
        public SkyboxType skyboxType;
        public Color customColor;

        public bool Equals(SkyboxData other)
        {
            return skyboxType == other.skyboxType && customColor.Equals(other.customColor);
        }

        public override bool Equals(object obj)
        {
            return obj is SkyboxData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) skyboxType * 397) ^ customColor.GetHashCode();
            }
        }
        public static bool operator ==(SkyboxData a, SkyboxData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SkyboxData a, SkyboxData b)
        {
            return !(a == b);
        }
    }

    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SceneOptionData : IEquatable<SceneOptionData>, ISceneOptionData<SkyboxData>
    {
        // View Options
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableTexture { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableLightData { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SkyboxData skyboxData { get; set; }

        // Climate Data
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableClimateSimulation { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public WeatherType weatherType { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float temperature { get; set; }

        // Stats info and Debug
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableStatsInfo { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool filterHlods { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableDebugOption { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetOrbitTypeAction.OrbitType touchOrbitType { get; set; }

        public bool Equals(SceneOptionData other)
        {
            return enableTexture == other.enableTexture && enableLightData == other.enableLightData && skyboxData.Equals(other.skyboxData) &&
                enableClimateSimulation == other.enableClimateSimulation && weatherType == other.weatherType &&
                temperature.Equals(other.temperature) && enableStatsInfo == other.enableStatsInfo && enableDebugOption == other.enableDebugOption &&
                filterHlods == other.filterHlods && touchOrbitType == other.touchOrbitType;
        }

        public override bool Equals(object obj)
        {
            return obj is SceneOptionData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = enableTexture.GetHashCode();
                hashCode = (hashCode * 397) ^ enableLightData.GetHashCode();
                hashCode = (hashCode * 397) ^ skyboxData.GetHashCode();
                hashCode = (hashCode * 397) ^ enableClimateSimulation.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) weatherType;
                hashCode = (hashCode * 397) ^ temperature.GetHashCode();
                hashCode = (hashCode * 397) ^ enableStatsInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ enableDebugOption.GetHashCode();
                hashCode = (hashCode * 397) ^ filterHlods.GetHashCode();
                hashCode = (hashCode * 397) ^ touchOrbitType.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SceneOptionData a, SceneOptionData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SceneOptionData a, SceneOptionData b)
        {
            return !(a == b);
        }
    }
}
