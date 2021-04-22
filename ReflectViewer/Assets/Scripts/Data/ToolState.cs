using System;

namespace Unity.Reflect.Viewer.UI
{
    public enum OrbitType
    {
        None = -1,
        WorldOrbit = 0,
        OrbitAtSelection = 1,
        OrbitAtPoint = 2,
    }

    public enum InfoType
    {
        Info = 0,
        Debug = 1,
    }

    public enum ClippingTool
    {
        AddXPlane = 0,
        AddYPlane = 1,
        AddZPlane = 2,
    }

    public enum MeasureTool
    {
        Distance = 0,
        Angle = 1,
        Delete = 2,
        Settings = 3,
    }


    [Serializable]
    public struct ToolState : IEquatable<ToolState>
    {
        public ToolType activeTool;
        public OrbitType orbitType;
        public ClippingTool clippingTool;
        public InfoType infoType;
        public FollowUserTool followUserTool;

        public bool Equals(ToolState other)
        {
            return activeTool == other.activeTool &&
                orbitType == other.orbitType &&
                clippingTool == other.clippingTool &&
                infoType == other.infoType &&
                followUserTool == other.followUserTool;
        }

        public override bool Equals(object obj)
        {
            return obj is ToolState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) activeTool;
                hashCode = (hashCode * 397) ^ (int) orbitType;
                hashCode = (hashCode * 397) ^ (int) clippingTool;
                hashCode = (hashCode * 397) ^ (int) infoType;
                hashCode = (hashCode * 397) ^ followUserTool.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ToolState a, ToolState b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ToolState a, ToolState b)
        {
            return !(a == b);
        }
    }
}
