using System;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    [Serializable]
    public struct UserIdentity : IEquatable<UserIdentity>
    {
        public string matchmakerId;
        public int colorIndex;
        public string fullName;
        public DateTime connectionTimeStamp;
        public string vivoxParticipantId;

        public UserIdentity(string matchmakerId, int colorIndex, string fullName, DateTime connectionTimeStamp,
            string vivoxParticipantId)
        {
            this.matchmakerId = matchmakerId;
            this.colorIndex = colorIndex;
            this.fullName = fullName;
            this.connectionTimeStamp = connectionTimeStamp;
            this.vivoxParticipantId = vivoxParticipantId;
        }
        public static bool operator ==(UserIdentity a, UserIdentity b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UserIdentity a, UserIdentity b)
        {
            return !(a == b);
        }

        public bool Equals(UserIdentity other)
        {
            return matchmakerId == other.matchmakerId &&
                colorIndex == other.colorIndex &&
                connectionTimeStamp == other.connectionTimeStamp &&
                fullName == other.fullName &&
                vivoxParticipantId == other.vivoxParticipantId;
        }

        public override bool Equals(object obj)
        {
            return obj is UserIdentity other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = matchmakerId.GetHashCode();
                hashCode = (hashCode * 397) ^ colorIndex.GetHashCode();
                hashCode = (hashCode * 397) ^ connectionTimeStamp.GetHashCode();
                hashCode = (hashCode * 397) ^ fullName.GetHashCode();
                hashCode = (hashCode * 397) ^ vivoxParticipantId.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return string.Format("id: {0}, color: {1}, name: {2}", matchmakerId, colorIndex, fullName);
        }
    }
}
