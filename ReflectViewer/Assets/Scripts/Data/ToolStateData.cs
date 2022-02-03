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
    public struct ToolStateData : IEquatable<ToolStateData>, IToolStateDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetActiveToolAction.ToolType activeTool { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetOrbitTypeAction.OrbitType orbitType { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetClippingToolAction.ClippingTool clippingTool { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetInfoTypeAction.InfoType infoType { get; set; }

        public FollowUserTool followUserTool;

        public bool Equals(ToolStateData other)
        {
            return activeTool == other.activeTool &&
                orbitType == other.orbitType &&
                clippingTool == other.clippingTool &&
                infoType == other.infoType &&
                followUserTool == other.followUserTool;
        }

        public override bool Equals(object obj)
        {
            return obj is ToolStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int)activeTool;
                hashCode = (hashCode * 397) ^ (int)orbitType;
                hashCode = (hashCode * 397) ^ (int)clippingTool;
                hashCode = (hashCode * 397) ^ (int)infoType;
                hashCode = (hashCode * 397) ^ followUserTool.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ToolStateData a, ToolStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ToolStateData a, ToolStateData b)
        {
            return !(a == b);
        }
    }
}
