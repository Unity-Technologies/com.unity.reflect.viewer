using System;
using System.Runtime.InteropServices;

namespace Unity.Reflect.Viewer.UI
{

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SettingsToolStateData: IEquatable<SettingsToolStateData>
    {
        public bool bimFilterEnabled;
        public bool sceneOptionEnabled;
        public bool sunStudyEnabled;

        public static readonly SettingsToolStateData defaultData = new SettingsToolStateData()
        {
            bimFilterEnabled = true,
            sceneOptionEnabled = true,
            sunStudyEnabled = true,
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
            return bimFilterEnabled == other.bimFilterEnabled && sceneOptionEnabled == other.sceneOptionEnabled &&
                sunStudyEnabled == other.sunStudyEnabled;
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
                hashCode = (hashCode * 397) ^ sceneOptionEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ sunStudyEnabled.GetHashCode();
                return hashCode;
            }
        }
    }
}
