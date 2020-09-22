using System;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct UISessionStateData : IEquatable<UISessionStateData>
    {
        public SessionState sessionState;

        public override string ToString()
        {
            return ToString("(SessionState {0})");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object)this.sessionState);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = sessionState.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UISessionStateData))
                return false;
            return this.Equals((UISessionStateData)obj);
        }

        public bool Equals(UISessionStateData other)
        {
            return
                this.sessionState == other.sessionState;
        }

        public static bool operator ==(UISessionStateData a, UISessionStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UISessionStateData a, UISessionStateData b)
        {
            return !(a == b);
        }
    }
}
