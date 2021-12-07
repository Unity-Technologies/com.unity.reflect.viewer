using System;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer
{

    [Serializable, GeneratePropertyBag]
    public struct PipelineStateData : IEquatable<PipelineStateData>, IPipelineDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Transform rootNode { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetVREnableAction.DeviceCapability deviceCapability { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ViewerReflectBootstrapper reflect { get; set; }

        public bool Equals(PipelineStateData other)
        {
            return rootNode.Equals(other.rootNode) &&
                deviceCapability.Equals(other.deviceCapability);
        }

        public override bool Equals(object obj)
        {
            return obj is PipelineStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = rootNode.GetHashCode();
                hashCode = (hashCode * 397) ^ deviceCapability.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(PipelineStateData a, PipelineStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(PipelineStateData a, PipelineStateData b)
        {
            return !(a == b);
        }
    }
}
