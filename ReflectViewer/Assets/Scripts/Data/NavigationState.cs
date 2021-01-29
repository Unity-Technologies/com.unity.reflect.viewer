using System;
using System.Collections.Generic;
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

    public enum ARMode
    {
        ViewBased,
        WallBased,
        TableTop
    }

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NavigationModeInfo : IEquatable<NavigationModeInfo>
    {
        public string modeScenePath;
        public NavigationMode navigationMode;

        public override string ToString()
        {
            return ToString("(Scene {0}, navigationMode {1}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                this.modeScenePath,
                (object) this.navigationMode);
        }

        public bool Equals(NavigationModeInfo other)
        {
            return
                this.navigationMode == other.navigationMode &&
                this.modeScenePath == other.modeScenePath;
        }

        public override bool Equals(object obj)
        {
            return obj is NavigationModeInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            var hashCode = modeScenePath.GetHashCode();
            hashCode = (hashCode * 397) ^ (int)navigationMode;

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
            orbitEnabled = true,
            panEnabled = true,
            zoomEnabled = true,
            moveEnabled = true,
            worldOrbitEnabled = true,
            teleportEnabled = true,
            gizmoEnabled = true,
            showScaleReference = false
        };

        public NavigationMode navigationMode;
        public bool freeFlyCameraEnabled;
        public bool orbitEnabled;
        public bool panEnabled;
        public bool zoomEnabled;
        public bool moveEnabled;
        public bool worldOrbitEnabled;
        public bool teleportEnabled;
        public bool gizmoEnabled;
        public bool showScaleReference;

        [SerializeField, Tooltip("List of NavigationModeInfo.")]
        public List<NavigationModeInfo> navigationModeInfos;

        public NavigationState(NavigationMode navigationMode, bool freeFlyCameraEnabled, bool orbitEnabled, bool panEnabled, bool zoomEnabled, bool moveEnabled,
            bool worldOrbitEnabled, bool teleportEnabled, bool gizmoEnabled, bool showScaleReference)
        {
            this.freeFlyCameraEnabled = freeFlyCameraEnabled;
            this.navigationMode = navigationMode;
            this.orbitEnabled = orbitEnabled;
            this.panEnabled = panEnabled;
            this.zoomEnabled = zoomEnabled;
            this.moveEnabled = moveEnabled;
            this.worldOrbitEnabled = worldOrbitEnabled;
            this.teleportEnabled = teleportEnabled;
            this.gizmoEnabled = gizmoEnabled;
            this.showScaleReference = showScaleReference;
            navigationModeInfos = new List<NavigationModeInfo>();
        }

        public static NavigationState Validate(NavigationState state)
        {
            return state;
        }

        public override string ToString()
        {
            return ToString("(NavigationMode {0}, FreeFlyCamera Enabled, {1}, Orbit Enabled {2}, Pan Enabled {3}, Zoom Enabled {4}, Move Enabled {5}" +
                            " World Orbit Enabled {6}, Teleport Enabled {7}, Gizmo Enabled {8}, Show Scale Reference {9}");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object) navigationMode,
                (object) freeFlyCameraEnabled,
                (object) orbitEnabled,
                (object) panEnabled,
                (object) zoomEnabled,
                (object) moveEnabled,
                (object) worldOrbitEnabled,
                (object) teleportEnabled,
                (object) gizmoEnabled,
                (object) showScaleReference);
        }

        public bool Equals(NavigationState other)
        {
            return
                navigationMode == other.navigationMode &&
                freeFlyCameraEnabled == other.freeFlyCameraEnabled &&
                orbitEnabled == other.orbitEnabled &&
                panEnabled == other.panEnabled &&
                zoomEnabled == other.zoomEnabled &&
                moveEnabled == other.moveEnabled &&
                worldOrbitEnabled == other.worldOrbitEnabled &&
                teleportEnabled == other.teleportEnabled &&
                gizmoEnabled == other.gizmoEnabled &&
                showScaleReference == other.showScaleReference;
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
                hashCode = (hashCode * 397) ^ freeFlyCameraEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ orbitEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ panEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ zoomEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ moveEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ worldOrbitEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ teleportEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ gizmoEnabled.GetHashCode();
                hashCode = (hashCode * 397) ^ showScaleReference.GetHashCode();
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

        public void EnableAllNavigation(bool enable)
        {
            freeFlyCameraEnabled = enable;
            orbitEnabled = enable;
            panEnabled = enable;
            zoomEnabled = enable;
            moveEnabled = enable;
            worldOrbitEnabled = enable;
            teleportEnabled = enable;
            gizmoEnabled = enable;
        }
    }
}
