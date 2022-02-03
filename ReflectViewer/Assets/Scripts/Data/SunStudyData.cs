using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.Reflect.Viewer.UI;
using Unity.SunStudy;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable, GeneratePropertyBag]
    public struct SunStudyData : IEquatable<SunStudyData>, ISunstudyDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int timeOfDay { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int timeOfYear{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int utcOffset{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int latitude{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int longitude{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public int northAngle{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float altitude{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float azimuth{ get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool isStaticMode { get; set; }

        public bool Equals(SunStudyData other)
        {
            return timeOfDay == other.timeOfDay && timeOfYear == other.timeOfYear &&
                    utcOffset == other.utcOffset && latitude == other.latitude &&
                    longitude == other.longitude && northAngle == other.northAngle && altitude == other.altitude &&
                    azimuth == other.azimuth && isStaticMode == other.isStaticMode;
        }

        public override bool Equals(object obj)
        {
            return obj is SunStudyData other && Equals(other);
        }

        public static bool operator ==(SunStudyData a, SunStudyData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(SunStudyData a, SunStudyData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = timeOfDay;
                hashCode = (hashCode * 397) ^ timeOfYear;
                hashCode = (hashCode * 397) ^ utcOffset;
                hashCode = (hashCode * 397) ^ latitude;
                hashCode = (hashCode * 397) ^ longitude;
                hashCode = (hashCode * 397) ^ northAngle;
                hashCode = (hashCode * 397) ^ altitude.GetHashCode();
                hashCode = (hashCode * 397) ^ azimuth.GetHashCode();
                hashCode = (hashCode * 397) ^ isStaticMode.GetHashCode();
                return hashCode;
            }
        }

        public static SunStudyData Format(SunStudyData sunStudyData)
        {
            sunStudyData.utcOffset -= (sunStudyData.utcOffset % 25); // only 15 minutes increments
            return sunStudyData;
        }
    }
}
