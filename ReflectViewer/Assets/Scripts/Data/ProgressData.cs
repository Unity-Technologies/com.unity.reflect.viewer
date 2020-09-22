using System;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct ProgressData : IEquatable<ProgressData>
    {
        public enum ProgressState
        {
            NoPendingRequest,
            PendingIndeterminate,
            PendingDeterminate,
        }

        public ProgressState progressState;
        public int currentProgress;
        public int totalCount;
        public string message;

        public static bool operator ==(ProgressData a, ProgressData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProgressData a, ProgressData b)
        {
            return !(a == b);
        }

        public bool Equals(ProgressData other)
        {
            return progressState == other.progressState && currentProgress == other.currentProgress &&
                totalCount == other.totalCount && message == other.message;
        }

        public override bool Equals(object obj)
        {
            return obj is ProgressData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) progressState;
                hashCode = (hashCode * 397) ^ currentProgress;
                hashCode = (hashCode * 397) ^ totalCount;
                hashCode = (hashCode * 397) ^ (message != null ? message.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
