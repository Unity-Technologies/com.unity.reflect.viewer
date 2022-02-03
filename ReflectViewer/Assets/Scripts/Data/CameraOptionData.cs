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
    public struct CameraViewOption : ICameraViewOption
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetCameraViewTypeAction.CameraViewType cameraViewType { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int numberOfClick { get; set; }
    }

    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct CameraOptionData : IEquatable<CameraOptionData>, ICameraOptionsDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetCameraProjectionTypeAction.CameraProjectionType cameraProjectionType { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableJoysticks { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public JoystickPreference joystickPreference { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool enableAutoNavigationSpeed { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int navigationSpeed { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ICameraViewOption cameraViewOption { get; set; }

        public bool Equals(CameraOptionData other)
        {
            return cameraProjectionType == other.cameraProjectionType && cameraViewOption == other.cameraViewOption &&
                enableJoysticks == other.enableJoysticks && joystickPreference == other.joystickPreference &&
                enableAutoNavigationSpeed == other.enableAutoNavigationSpeed && navigationSpeed == other.navigationSpeed;
        }

        public override bool Equals(object obj)
        {
            return obj is CameraOptionData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) cameraProjectionType;
                hashCode = (hashCode * 397) ^ cameraViewOption.GetHashCode();
                hashCode = (hashCode * 397) ^ enableJoysticks.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) joystickPreference;
                hashCode = (hashCode * 397) ^ enableAutoNavigationSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ navigationSpeed;
                return hashCode;
            }
        }

        public static bool operator ==(CameraOptionData a, CameraOptionData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CameraOptionData a, CameraOptionData b)
        {
            return !(a == b);
        }
    }
}
