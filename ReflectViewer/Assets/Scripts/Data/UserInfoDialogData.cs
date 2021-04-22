
using System;
using UnityEngine;

namespace Unity.Reflect.Viewer
{
    [Serializable]
    public struct UserInfoDialogData : IEquatable<UserInfoDialogData>
    {
        public string matchmakerId;
        public Vector3 dialogPosition;

        public UserInfoDialogData(string userIdentity, Vector2 dialogPosition)
        {
            this.matchmakerId = userIdentity;
            this.dialogPosition = dialogPosition;
        }

        public static bool operator ==(UserInfoDialogData a, UserInfoDialogData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UserInfoDialogData a, UserInfoDialogData b)
        {
            return !(a == b);
        }

        public bool Equals(UserInfoDialogData other)
        {
            return matchmakerId == other.matchmakerId &&
                dialogPosition == other.dialogPosition;
        }

        public override bool Equals(object obj)
        {
            return obj is UserInfoDialogData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = matchmakerId.GetHashCode();
                hashCode = (hashCode * 397) ^ dialogPosition.GetHashCode();
                return hashCode;
            }
        }
    }
}
