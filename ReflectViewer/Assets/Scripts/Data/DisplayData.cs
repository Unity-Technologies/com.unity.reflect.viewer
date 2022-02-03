using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct DisplayData : IEquatable<DisplayData>, IDisplayDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector2 screenSize { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector2 scaledScreenSize { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetDisplayAction.ScreenSizeQualifier screenSizeQualifier { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float targetDpi { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float dpi { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float scaleFactor { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetDisplayAction.DisplayType displayType { get; set; }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = screenSize.GetHashCode();
                hashCode = (hashCode * 397) ^ scaledScreenSize.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)screenSizeQualifier;
                hashCode = (hashCode * 397) ^ (int)(targetDpi * 100);
                hashCode = (hashCode * 397) ^ (int)(dpi * 100);
                hashCode = (hashCode * 397) ^ (int)(scaleFactor * 100);
                hashCode = (hashCode * 397) ^ (int)(displayType);
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is DisplayData other && Equals(other);
        }

        public bool Equals(DisplayData other)
        {
            return screenSize == other.screenSize &&
                scaledScreenSize == other.scaledScreenSize &&
                screenSizeQualifier == other.screenSizeQualifier &&
                targetDpi == other.targetDpi &&
                dpi == other.dpi &&
                scaleFactor == other.scaleFactor &&
                displayType == other.displayType;
        }

        public static bool operator ==(DisplayData a, DisplayData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DisplayData a, DisplayData b)
        {
            return !(a == b);
        }
    }
}
