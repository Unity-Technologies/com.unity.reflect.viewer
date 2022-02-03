using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ProgressData : IEquatable<ProgressData>, IProgressDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetProgressStateAction.ProgressState progressState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int currentProgress { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int totalCount { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string message { get; set; }

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
