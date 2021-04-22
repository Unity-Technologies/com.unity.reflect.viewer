
using System;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct FollowUserTool : IEquatable<FollowUserTool>
    {
        public string userId;
        public GameObject userObject { get; set; }

        public bool Equals(FollowUserTool other)
        {
            return userId == other.userId &&
                userObject == other.userObject;
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
