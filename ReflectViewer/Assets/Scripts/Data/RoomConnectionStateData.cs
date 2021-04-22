using System;
using System.Collections.Generic;
using Unity.Reflect.Multiplayer;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer
{
    [Serializable]
    public struct RoomConnectionStateData : IEquatable<RoomConnectionStateData>
    {
        public NetworkUserData localUser;
        public List<NetworkUserData> users;

        public static bool operator ==(RoomConnectionStateData a, RoomConnectionStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RoomConnectionStateData a, RoomConnectionStateData b)
        {
            return !(a == b);
        }

        public bool Equals(RoomConnectionStateData other)
        {
            return localUser == other.localUser &&
                EnumerableExtension.SafeSequenceEquals(users, other.users);
        }

        public override bool Equals(object obj)
        {
            return obj is RoomConnectionStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = localUser.GetHashCode();
                foreach (var user in users)
                    hashCode = (hashCode * 397) ^ user.GetHashCode();
                return hashCode;
            }
        }
    }

}
