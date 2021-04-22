using System;
using UnityEngine.Reflect.MeasureTool;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct ExternalToolStateData : IEquatable<ExternalToolStateData>
    {
        public MeasureToolStateData measureToolStateData;

        public override string ToString()
        {
            return ToString("MeasureToolStateData {0}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                measureToolStateData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = measureToolStateData.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is UIStateData other && Equals(other);
        }

        public bool Equals(ExternalToolStateData other)
        {
            return measureToolStateData == other.measureToolStateData;
        }

        public static bool operator ==(ExternalToolStateData a, ExternalToolStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ExternalToolStateData a, ExternalToolStateData b)
        {
            return !(a == b);
        }

    }
}
