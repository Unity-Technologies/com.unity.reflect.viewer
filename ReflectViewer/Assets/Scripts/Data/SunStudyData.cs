using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Viewer.UI;
using Unity.SunStudy;
using UnityEngine;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct SunStudyData : IEquatable<SunStudyData>
    {
        public int timeOfDay;
        public int timeOfYear;
        public int utcOffset;
        public int latitude;
        public int longitude;
        public int northAngle;
        public float altitude;
        public float azimuth;
        public bool isStaticMode;

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

        public static void SetSunStudyData(Unity.SunStudy.SunStudy sunStudy, SunStudyData sunStudyData)
        {
            sunStudy.MinuteOfDay = sunStudyData.timeOfDay;
            sunStudy.DayOfYear = sunStudyData.timeOfYear;
            sunStudy.UtcOffset = sunStudyData.utcOffset / 100f;
            sunStudy.CoordLatitude = sunStudyData.latitude;
            sunStudy.CoordLongitude = sunStudyData.longitude;
            sunStudy.NorthAngle = sunStudyData.northAngle;
            sunStudy.Azimuth = sunStudyData.azimuth;
            sunStudy.Altitude = sunStudyData.altitude;
            sunStudy.PlacementMode = sunStudyData.isStaticMode ? SunPlacementMode.Static : SunPlacementMode.Geographical;
        }
        public static SunStudyData Format(SunStudyData sunStudyData)
        {
            sunStudyData.utcOffset -= (sunStudyData.utcOffset % 25); // only 15 minutes increments
            return sunStudyData;
        }
        public static string GetSunStatusMessage(ToolbarType toolbarType, SunStudyData sunStudyData)
        {
            // Set status message if sun study dials are open
            string statusMessage = toolbarType == ToolbarType.AltitudeAzimuthDial ? AltitudeAzimuthRadialUIController.GetAltAzStatusMessage(sunStudyData) :
                            toolbarType == ToolbarType.TimeOfDayYearDial ? TimeRadialUIController.GetTimeStatusMessage(sunStudyData) : String.Empty;
            return statusMessage;
        }
    }
}
