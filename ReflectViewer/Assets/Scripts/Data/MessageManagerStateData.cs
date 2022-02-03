using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MessageManagerStateData : IEquatable<MessageManagerStateData>, IStatusMessageData
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public StatusMessageData statusMessageData { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isClearAll { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isInstructionMode { get; set; }

        public override string ToString()
        {
            return ToString("(Status {0} Message {1} ");
        }

        public string ToString(string format)
        {
            return string.Format(format, statusMessageData);
        }

        public override int GetHashCode()
        {
            var hashCode = statusMessageData.GetHashCode();
            hashCode = (hashCode * 397) ^ isClearAll.GetHashCode();
            hashCode = (hashCode * 397) ^ isInstructionMode.GetHashCode();
            return hashCode;
        }

        public override bool Equals(object obj)
        {
            return obj is MessageManagerStateData data && this.Equals(data);
        }

        public bool Equals(MessageManagerStateData other)
        {
            return statusMessageData.text == other.statusMessageData.text
                && statusMessageData.type == other.statusMessageData.type
                && isClearAll == other.isClearAll
                && isInstructionMode == other.isInstructionMode;
        }

        public static bool operator ==(MessageManagerStateData a, MessageManagerStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MessageManagerStateData a, MessageManagerStateData b)
        {
            return !(a == b);
        }
    }
}
