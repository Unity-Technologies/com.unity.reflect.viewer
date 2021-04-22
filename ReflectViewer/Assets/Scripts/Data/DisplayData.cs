using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Unity.Reflect.Viewer.UI
{

    [Serializable]
    public enum ScreenSizeQualifier
    {
        XSmall,
        Small,
        Medium,
        Large,
        XLarge
    }

    public enum DisplayType
    {
        Desktop,
        Tablet,
        Phone
    }

    [Serializable]
    public struct DisplayData : IEquatable<DisplayData>
    {
        public Vector2 screenSize;
        public Vector2 scaledScreenSize;
        public ScreenSizeQualifier screenSizeQualifier;
        public float targetDpi;
        public float dpi;
        public float scaleFactor;
        public DisplayType displayType;

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
