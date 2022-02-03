using System;
using System.Runtime.InteropServices;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DeltaDNAStateData : IEquatable<DeltaDNAStateData>, IDeltaDNADataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public DNALicenseInfo dnaLicenseInfo{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string buttonName{ get; set; }


        public static UISessionStateData Validate(UISessionStateData state)
        {
            return state;
        }

        public override string ToString()
        {
            return ToString("(dnaLicenseInfo {0}, buttonName {1}");
        }

        public string ToString(string format)
        {
            return string.Format(format, dnaLicenseInfo, buttonName);
        }

        public bool Equals(DeltaDNAStateData other)
        {
            return
                dnaLicenseInfo.Equals(other.dnaLicenseInfo) &&
                buttonName == other.buttonName;
        }

        public override bool Equals(object obj)
        {
            return obj is DeltaDNAStateData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode =  dnaLicenseInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ buttonName.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(DeltaDNAStateData a, DeltaDNAStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(DeltaDNAStateData a, DeltaDNAStateData b)
        {
            return !(a == b);
        }
    }
}
