using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Unity.Reflect.Viewer.UI
{
    public enum InstructionUI
    {
        Init,
        CrossPlatformFindAPlane,
        AimToPlaceBoundingBox,
        ConfirmPlacement,
        OnBoardingComplete,
    };

    [Serializable]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct UIARStateData : IEquatable<UIARStateData>
    {
        public int numPlanesAdded;
        public int detectionThreshold;
        public InstructionUI instructionUI;
        public int numProxyInstances;
        public static readonly UIARStateData defaultData = new UIARStateData()
        {
            numPlanesAdded = 0,
            detectionThreshold = 1,
            instructionUI = InstructionUI.Init,
            numProxyInstances = 0
        };

        public UIARStateData(int numPlanesAdded, int detectionThreshold, InstructionUI instructionUI, int numProxyInstances)
        {
            this.numPlanesAdded = numPlanesAdded;
            this.detectionThreshold = detectionThreshold;
            this.instructionUI = instructionUI;
            this.numProxyInstances = numProxyInstances;
        }

        public static UIARStateData Validate(UIARStateData stateData)
        {
            return stateData;
        }

        public override string ToString()
        {
            return ToString("numPlanesAdded{0}, detectionThreshold{1}, instructionUI{2}, numOfProxyInstances");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                numPlanesAdded,
                detectionThreshold,
                (object) instructionUI,
                numProxyInstances);

        }

        public bool Equals(UIARStateData other)
        {
            return numPlanesAdded == other.numPlanesAdded &&
                   detectionThreshold == other.detectionThreshold &&
                   instructionUI == other.instructionUI &&
                   numProxyInstances == other.numProxyInstances;
        }

        public override bool Equals(object obj)
        {
            return obj is UIARStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = numPlanesAdded;
                hashCode = (hashCode * 397) ^ detectionThreshold;
                hashCode = (hashCode * 397) ^ (int)instructionUI;
                hashCode = (hashCode * 397) ^ numProxyInstances;
                return hashCode;
            }
        }

        public static bool operator ==(UIARStateData a, UIARStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIARStateData a, UIARStateData b)
        {
            return !(a == b);
        }
    }
}
