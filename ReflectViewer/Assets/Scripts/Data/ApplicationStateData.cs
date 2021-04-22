using System;


namespace Unity.Reflect.Viewer
{
    [Serializable]
    public struct ApplicationStateData : IEquatable<ApplicationStateData>
    {
        public QualityState qualityStateData;

        public bool Equals(ApplicationStateData other)
        {
            return qualityStateData.Equals(other.qualityStateData);
        }

        public override bool Equals(object obj)
        {
            return obj is ApplicationStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = qualityStateData.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ApplicationStateData a, ApplicationStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ApplicationStateData a, ApplicationStateData b)
        {
            return !(a == b);
        }
    }
}
