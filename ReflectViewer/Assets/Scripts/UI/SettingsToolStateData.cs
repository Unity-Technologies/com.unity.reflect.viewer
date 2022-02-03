using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SettingsToolStateData: IEquatable<SettingsToolStateData>, ISettingsToolDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool bimFilterEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool sceneSettingsEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool sunStudyEnabled { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool markerSettingsEnabled { get; set; }


        public static readonly SettingsToolStateData defaultData = new SettingsToolStateData()
        {
            bimFilterEnabled = true,
            sceneSettingsEnabled = true,
            sunStudyEnabled = true,
            markerSettingsEnabled = true
        };


        public static bool operator ==(SettingsToolStateData a, SettingsToolStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SettingsToolStateData a, SettingsToolStateData b)
        {
            return !(a == b);
        }

        public bool Equals(SettingsToolStateData other)
        {
            return bimFilterEnabled == other.bimFilterEnabled && sceneSettingsEnabled == other.sceneSettingsEnabled &&
                sunStudyEnabled == other.sunStudyEnabled && markerSettingsEnabled == other.markerSettingsEnabled;
        }

        public override bool Equals(object obj)
        {
            return obj is SettingsToolStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = bimFilterEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ sceneSettingsEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ sunStudyEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ markerSettingsEnabled.GetHashCode();
                return hashCode;
            }
        }
    }
}
