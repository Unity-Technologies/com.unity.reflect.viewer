using System;
using System.Runtime.InteropServices;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DebugOptionsData : IEquatable<DebugOptionsData>
    {
        public bool gesturesTrackingEnabled;
        public bool ARAxisTrackingEnabled;

        public static readonly DebugOptionsData defaultData = new DebugOptionsData()
        {
            gesturesTrackingEnabled = false,
            ARAxisTrackingEnabled = false,
        };

        public DebugOptionsData(bool gesturesTrackingEnabled, bool ARAxisTrackingEnabled)
        {
            this.gesturesTrackingEnabled = gesturesTrackingEnabled;
            this.ARAxisTrackingEnabled = ARAxisTrackingEnabled;
        }

        public static DebugOptionsData Validate(DebugOptionsData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("gesturesTrackingEnabled{0}, ARAxisTrackingEnabled{1}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                gesturesTrackingEnabled,
                ARAxisTrackingEnabled);
        }

        public bool Equals(DebugOptionsData other)
        {
            return gesturesTrackingEnabled == other.gesturesTrackingEnabled &&
                ARAxisTrackingEnabled == other.ARAxisTrackingEnabled;
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
