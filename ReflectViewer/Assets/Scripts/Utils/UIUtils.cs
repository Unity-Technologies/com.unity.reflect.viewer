using System;
using System.Text.RegularExpressions;
using Unity.Reflect.Viewer.UI;

namespace UnityEngine.Reflect.Utils
{
    public class UIUtils
    {
        public static string CreateInitialsFor(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return string.Empty;
            }

            // first remove all: punctuation, separator chars, control chars, and numbers (unicode style regexes)
            string initials = Regex.Replace(fullName, @"[\p{P}\p{S}\p{C}\p{N}]+", "");

            // Replacing all possible whitespace/separator characters (unicode style), with a single, regular ascii space.
            initials = Regex.Replace(initials, @"\p{Z}+", " ");

            // Remove all Sr, Jr, I, II, III, IV, V, VI, VII, VIII, IX at the end of names
            initials = Regex.Replace(initials.Trim(), @"\s+(?:[JS]R|I{1,3}|I[VX]|VI{0,3})$", "", RegexOptions.IgnoreCase);

            // Extract up to 2 initials from the remaining cleaned name.
            initials = Regex.Replace(initials, @"^(\p{L})[^\s]*(?:\s+(?:\p{L}+\s+(?=\p{L}))?(?:(\p{L})\p{L}*)?)?$", "$1$2").Trim();

            if (initials.Length > 2)
            {
                // Worst case scenario, everything failed, just grab the first two letters of what we have left.
                initials = initials.Substring(0, 2);
            }

            return initials.ToUpperInvariant();
        }

        public const string TimeIntervalJustNow = "just now";
        public const string TimeIntervalAMinute = "one minute ago";
        public const string TimeIntervalMinutes = "minutes ago";
        public const string TimeIntervalAnHour = "one hour ago";
        public const string TimeIntervalHours = "hours ago";
        public const string TimeIntervalYesteday = "yesterday";
        public const string TimeIntervalDays = "days ago";
        public const string TimeIntervalAWeek = "last week";
        public const string TimeIntervalWeeks = "weeks ago";
        public const string TimeIntervalAMonth = "last month";
        public const string TimeIntervalMonths = "months ago";
        public const string TimeIntervalAYear = "last year";
        public const string TimeIntervalYears = "years ago";

        public static string GetTimeIntervalSinceNow(DateTime dateTime)
        {
            var now = DateTime.Now;
            var timeElapsed = now - dateTime;

            int seconds    = (int)timeElapsed.TotalSeconds;
            if(seconds < 60)
            {
                return TimeIntervalJustNow;
            }

            int minutes = (int)timeElapsed.TotalMinutes ;
            if(minutes < 60)
            {
                if(minutes == 1)
                {
                    return TimeIntervalAMinute;
                }
                return $"{minutes.ToString()} {TimeIntervalMinutes}";
            }

            int hours = (int)timeElapsed.TotalHours;
            if(hours < 24)
            {
                if(hours == 1)
                {
                    return TimeIntervalAnHour;
                }
                return $"{hours.ToString()} {TimeIntervalHours}";
            }

            int days = (int)timeElapsed.TotalDays;
            if(days < 7)
            {
                if(days == 1)
                {
                    return TimeIntervalYesteday;
                }
                return $"{days.ToString()} {TimeIntervalDays}";
            }

            int weeks = (int)timeElapsed.TotalDays / 7;
            if(weeks < 4)
            {
                if(weeks == 1)
                {
                    return TimeIntervalAWeek;
                }
                return $"{weeks.ToString()} {TimeIntervalWeeks}";
            }

            var months = timeElapsed.TotalDays / 30;
            if(months < 12)
            {
                if(months < 2)
                {
                    return TimeIntervalAMonth;
                }
                return $"{Math.Round(months).ToString()} {TimeIntervalMonths}";
            }

            var years = timeElapsed.TotalDays / 365;
            if(years < 2)
            {
                return TimeIntervalAYear;
            }
            return $"{Math.Round(years).ToString()} {TimeIntervalYears}";
        }


        const int k_XLargeScreenSizeThreshold = 1920;
        const int k_LargeScreenSizeThreshold = 1280;
        const int k_MediumScreenSizeThreshold = 960;
        const int k_SmallScreenSizeThreshold = 600;
        public static ScreenSizeQualifier QualifyScreenSize(Vector2 screenSize)
        {
            if (screenSize.x >= k_XLargeScreenSizeThreshold || screenSize.y >= k_XLargeScreenSizeThreshold)
            {
                return ScreenSizeQualifier.XLarge;
            }
            if (screenSize.x >= k_LargeScreenSizeThreshold || screenSize.y >= k_LargeScreenSizeThreshold)
            {
                return ScreenSizeQualifier.Large;
            }
            if (screenSize.x >= k_MediumScreenSizeThreshold || screenSize.y >= k_MediumScreenSizeThreshold)
            {
                return ScreenSizeQualifier.Medium;
            }
            if (screenSize.x >= k_SmallScreenSizeThreshold || screenSize.y >= k_SmallScreenSizeThreshold)
            {
                return ScreenSizeQualifier.Small;
            }
            return ScreenSizeQualifier.XSmall;
        }

        const int k_MinimumHeightTablet = 720;
        const int k_MinimumHeightPhone = 500;
        const float k_DiagonalSizeForTabletsInInches = 6.5f;
        const float k_TabletAspectRatio = 2f;
        const float k_TargetDPIWindows = 96;
        const float k_FallbackDpi = 96;
        const float k_FallbackDpiIpad = 264f;

        public static float GetTargetDpi(float dpi, DisplayType displayType)
        {
            return displayType == DisplayType.Desktop? dpi : k_TargetDPIWindows;
        }

        public static float GetScaleFactor(float width, float height, float dpi, DisplayType displayType)
        {
            var scaleFactor =  dpi / GetTargetDpi(dpi, displayType);
            Vector2 screenSize = new Vector2(width / scaleFactor, height / scaleFactor);
            var minimalHeight = FindMinimumDeviceHeight(displayType);
            if (screenSize.y < minimalHeight)
            {
                scaleFactor = height / minimalHeight;
            }
            return scaleFactor;
        }

        static float FindMinimumDeviceHeight(DisplayType displayType)
        {
            if (displayType == DisplayType.Tablet)
            {
                return k_MinimumHeightTablet;
            }

            return k_MinimumHeightPhone;
        }

        public static DisplayType GetDeviceType(float width, float height, float dpi)
        {
            if (Application.isMobilePlatform)
            {
                return GetDeviceTypeByDimension(width, height, dpi);
            }
            Debug.Log(SystemInfo.deviceType.ToString());

            return DisplayType.Desktop;
        }

        public static float GetScreenDpi()
        {
            var dpi = Screen.dpi;
            if(dpi == 0)
            {
                if (SystemInfo.deviceModel == "iPad8,11" || SystemInfo.deviceModel == "iPad8,12")
                {
                    return k_FallbackDpiIpad;
                }

                return k_FallbackDpi;
            }
            return dpi;
        }

        static DisplayType GetDeviceTypeByDimension(float width, float height, float dpi)
        {
            float aspectRatio = Mathf.Max(width, height) / Mathf.Min(width, height);
            var diagonalSizeInches = DeviceDiagonalSizeInInches(width, height, dpi);
            bool isTablet = (diagonalSizeInches > k_DiagonalSizeForTabletsInInches && aspectRatio < k_TabletAspectRatio);

            if (isTablet)
            {
                return DisplayType.Tablet;
            }
            return DisplayType.Phone;
        }

        static float DeviceDiagonalSizeInInches(float width, float height, float dpi)
        {
            float screenWidth = width / dpi;
            float screenHeight = height / dpi;
            return Mathf.Sqrt(screenWidth * screenWidth + screenHeight * screenHeight);
        }
    }
}
