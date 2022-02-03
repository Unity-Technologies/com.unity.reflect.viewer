using System;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class TimeRadialUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        DialControl m_HourDialControl;
        [SerializeField]
        DialControl m_MonthDialControl;
        [SerializeField]
        Button m_MainButton;
        [SerializeField]
        Button m_SecondaryButton;
        [SerializeField]
        Button m_ResetButton;
#pragma warning restore CS0649

        int m_DefaultHour;
        int m_DefaultMonth;
        public static SetActiveToolBarAction.ToolbarType m_previousToolbar;
        readonly MonthLabelConverter m_MonthLabels = new MonthLabelConverter();
        readonly HourLabelConverter m_HourLabels = new HourLabelConverter();
        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_MonthDialControl.labelConverter = m_MonthLabels;
            m_HourDialControl.labelConverter = m_HourLabels;

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.timeOfDay),
                (day) => m_HourDialControl.selectedValue = GetFloatFromMin(day)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.timeOfYear),
                (year) => m_MonthDialControl.selectedValue = GetFloatFromDay(year)));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<SetActiveToolBarAction.ToolbarType>(UIStateContext.current, nameof(IToolBarDataProvider.activeToolbar),
                (toolbar) =>
            {
                if (toolbar != SetActiveToolBarAction.ToolbarType.TimeOfDayYearDial &&
                    toolbar != SetActiveToolBarAction.ToolbarType.AltitudeAzimuthDial)
                {
                    m_previousToolbar = toolbar;
                }
            }));

        }

        void Start()
        {
            m_HourDialControl.onSelectedValueChanged.AddListener(OnHourDialValueChanged);
            m_MonthDialControl.onSelectedValueChanged.AddListener(OnMonthDialValueChanged);
            m_ResetButton.onClick.AddListener(OnResetButtonClicked);

            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_SecondaryButton.onClick.AddListener(OnSecondaryButtonClicked);
        }

        // See old commit for previous implementation
        void OnMonthDialValueChanged(float value)
        {

        }

        void OnHourDialValueChanged(float value)
        {

        }

        void OnResetButtonClicked()
        {

        }

        void OnMainButtonClicked()
        {

        }

        void OnSecondaryButtonClicked()
        {

        }

        public static string GetTimeStatusMessage(SunStudyData sunStudyData)
        {
            var time = SunStudyUIController.GetFormattedMinuteOfDay(sunStudyData.timeOfDay);
            var day = SunStudyUIController.GetFormattedDayOfYear(DateTime.Now.Year, sunStudyData.timeOfYear);
            return $"{day},  " + time;
        }

        class MonthLabelConverter: ILabelConverter
        {
            public string ConvertTickLabels(float value)
            {
                return SunStudyUIController.NameOfMonth((int)value);
            }
            string ILabelConverter.ConvertSelectedValLabel(float value, bool isInt)
            {
                return SunStudyUIController.NameOfMonth((int)value); // Just display month name of current month
            }
        }
        class HourLabelConverter: ILabelConverter
        {
            public string ConvertTickLabels(float value)
            {
                int intNum = (int)value;
                return intNum.ToString();
            }
            string ILabelConverter.ConvertSelectedValLabel(float value, bool isInt)
            {
                int intNum = (int)value;
                return intNum.ToString(); // Similarly, just dispaly current hour (i.e. 1:50 -> "1hr")
            }
        }

        // Get Day of Year from the dial's float value
        public static int GetDayFromFloat(float value)
        {
            int month = (int)Mathf.Clamp(value, 1, 12);
            int numDays = DateTime.DaysInMonth(DateTime.Now.Year, month);
            float percentage = value - month;
            int day =  (int)Math.Round(numDays * percentage);
            return SunStudyUIController.GetDayOfYear(DateTime.Now.Year, month, day);
        }
        public static float GetFloatFromDay(int dayOfYear)
        {
            (int month, int day) = SunStudyUIController.GetMonthDay(DateTime.Now.Year, dayOfYear);
            int numDays = DateTime.DaysInMonth(DateTime.Now.Year, month);
            float floatval = month + (float)(day - 1) / numDays;
            return floatval;
        }
        // Get Time of Day from dial's float value
        public static int GetMinFromFloat(float value)
        {
            int hour = (int)value;
            float percentage = value - hour;
            int min = (int)Math.Round(60 * percentage);
            return SunStudyUIController.GetMinuteOfDay(hour, min);
        }
        public static float GetFloatFromMin(int timeOfDay)
        {
            (int hour, int min) = SunStudyUIController.GetHourMinute(timeOfDay);
            float floatval = hour + (float)min / 60;
            return floatval;
        }
    }
}
