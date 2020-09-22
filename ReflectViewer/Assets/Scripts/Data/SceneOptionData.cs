using System;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public enum SkyboxType
    {
        Light = 0,
        Dark = 1,
        Custom = 2
    }

    public enum WeatherType
    {
        HeavyRain = 0,
        Sunny = 1,
    }

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

    [Serializable]
    public struct SceneOptionData : IEquatable<SceneOptionData>
    {
        // View Options
        public bool enableTexture;
        public bool enableLightData;
        public SkyboxData skyboxData;

        // Climate Data
        public bool enableClimateSimulation;
        public WeatherType weatherType;
        public float temperature;
        
        public bool Equals(SceneOptionData other)
        {
            return enableTexture == other.enableTexture && enableLightData == other.enableLightData && skyboxData.Equals(other.skyboxData) &&
                enableClimateSimulation == other.enableClimateSimulation && weatherType == other.weatherType &&
                temperature.Equals(other.temperature);
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