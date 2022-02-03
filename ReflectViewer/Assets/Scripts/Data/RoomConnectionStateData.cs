using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    [Serializable, GeneratePropertyBag]
    public struct RoomConnectionStateData : IEquatable<RoomConnectionStateData>, IRoomConnectionDataProvider<NetworkUserData>, IVivoxDataProvider<VivoxManager>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string userToMute { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public NetworkUserData localUser { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public List<NetworkUserData> users { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public VivoxManager vivoxManager { get; set; }

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
                EnumerableExtension.SafeSequenceEquals(users, other.users) &&
                vivoxManager == other.vivoxManager &&
                userToMute == other.userToMute;
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
                hashCode = (hashCode * 397) ^ userToMute.GetHashCode();
                return hashCode;
            }
        }
    }

}
