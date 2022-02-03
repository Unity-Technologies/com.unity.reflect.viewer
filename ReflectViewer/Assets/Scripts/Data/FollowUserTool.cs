using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct FollowUserTool : IEquatable<FollowUserTool>, IFollowUserDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string userId { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public GameObject userObject { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isFollowing { get; set; }

        public bool Equals(FollowUserTool other)
        {
            return userId == other.userId &&
                userObject == other.userObject &&
                isFollowing == other.isFollowing;
        }

        public override bool Equals(object obj)
        {
            return obj is FollowUserTool other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = userId != null ? userId.GetHashCode() : 1;
                hashCode = (hashCode * 397) ^ (ReferenceEquals(userObject, null)? 1: userObject.GetHashCode());
                hashCode = (hashCode * 397) ^ (ReferenceEquals(isFollowing, null)? 1: isFollowing.GetHashCode());
                return hashCode;
            }
        }

        public static bool operator ==(FollowUserTool a, FollowUserTool b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FollowUserTool a, FollowUserTool b)
        {
            return !(a == b);
        }
    }
}
