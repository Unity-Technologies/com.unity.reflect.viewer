using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.MeasureTool;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    public struct ExternalToolStateData : IEquatable<ExternalToolStateData>
    {
        public MeasureToolStateData measureToolStateData;

        public SunStudyData sunStudyData;

        public override string ToString()
        {
            return ToString("MeasureToolStateData {0}, SunStudyData {1},");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                measureToolStateData,
                sunStudyData);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = measureToolStateData.GetHashCode();
                hashCode = (hashCode * 397) ^ sunStudyData.GetHashCode();

                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is UIStateData other && Equals(other);
        }

        public bool Equals(ExternalToolStateData other)
        {
            return measureToolStateData == other.measureToolStateData &&
                   sunStudyData == other.sunStudyData;
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
