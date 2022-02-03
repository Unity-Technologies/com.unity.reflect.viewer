
using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UserInfoDialogData : IEquatable<UserInfoDialogData>, IUserInfoDialogDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string matchmakerId { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 dialogPosition { get; set; }

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
