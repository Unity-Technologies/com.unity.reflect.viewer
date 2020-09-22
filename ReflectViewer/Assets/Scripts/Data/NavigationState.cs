using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public enum NavigationMode : int
    {
        Orbit = 0,
        Fly = 1,
        Walk = 2,
        AR = 3,
        VR = 4,
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NavigationModeInfo : IEquatable<NavigationModeInfo>
    {
        public string modeScenePath;
        public ToolbarType modeToolbar;

        public override string ToString()
        {
            return ToString("(Scene {0}, modeToolbar {1}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                this.modeScenePath,
                (object) this.modeToolbar);
        }

        public bool Equals(NavigationModeInfo other)
        {
            return
                this.modeToolbar == other.modeToolbar &&
                this.modeScenePath == other.modeScenePath;
        }

        public override bool Equals(object obj)
        {
            return obj is NavigationModeInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = modeScenePath.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)modeToolbar;

            return hashCode;
        }
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NavigationState : IEquatable<NavigationState>
    {
        public static readonly NavigationState defaultData = new NavigationState()
        {
            navigationMode = NavigationMode.Orbit,
        };

        public NavigationMode navigationMode;

        public NavigationState(NavigationMode navigationMode)
        {
            this.navigationMode = navigationMode;
        }

        public static NavigationState Validate(NavigationState state)
        {
            return state;
        }

        public override string ToString()
        {
            return ToString("(NavigationMode {0}, OrbitType {1}, ClippingTool {2}), MeasureTool {3}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object) this.navigationMode);
        }

        public bool Equals(NavigationState other)
        {
            return
                this.navigationMode == other.navigationMode;
        }

        public override bool Equals(object obj)
        {
            return obj is NavigationState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)navigationMode;
                return hashCode;
            }
        }

        public static bool operator ==(NavigationState a, NavigationState b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(NavigationState a, NavigationState b)
        {
            return !(a == b);
        }
    }
}
