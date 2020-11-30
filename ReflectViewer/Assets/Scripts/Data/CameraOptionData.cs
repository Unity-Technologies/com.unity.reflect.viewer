using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    public enum CameraProjectionType
    {
        Perspective = 0,
        Orthographic = 1
    }

    public enum CameraViewType
    {
        Default = 0,
        Top = 1,
        Left = 2,
        Right = 3
    }

    public enum JoystickPreference
    {
        RightHanded = 0,
        LeftHanded = 1
    }
    
    [Serializable]
    public struct CameraOptionData : IEquatable<CameraOptionData>
    {
        public CameraProjectionType cameraProjectionType;
        public CameraViewType cameraViewType;

        public bool enableJoysticks;
        public JoystickPreference joystickPreference;

        public bool enableAutoNavigationSpeed;
        public int navigationSpeed;

        public int numberOfCLick;

        public bool Equals(CameraOptionData other)
        {
            return cameraProjectionType == other.cameraProjectionType && cameraViewType == other.cameraViewType &&
                enableJoysticks == other.enableJoysticks && joystickPreference == other.joystickPreference &&
                enableAutoNavigationSpeed == other.enableAutoNavigationSpeed && navigationSpeed == other.navigationSpeed
                && numberOfCLick == other.numberOfCLick;
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
                hashCode = (hashCode * 397) ^ (int) cameraViewType;
                hashCode = (hashCode * 397) ^ enableJoysticks.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) joystickPreference;
                hashCode = (hashCode * 397) ^ enableAutoNavigationSpeed.GetHashCode();
                hashCode = (hashCode * 397) ^ navigationSpeed;
                hashCode = (hashCode * 397) ^ numberOfCLick;
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