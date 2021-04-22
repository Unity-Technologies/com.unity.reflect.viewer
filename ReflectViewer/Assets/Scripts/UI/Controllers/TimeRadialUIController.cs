using SharpFlux;
using System;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class TimeRadialUIController : MonoBehaviour
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
        public static ToolbarType m_previousToolbar;
        readonly MonthLabelConverter m_MonthLabels = new MonthLabelConverter();
        readonly HourLabelConverter m_HourLabels = new HourLabelConverter();

        SunStudyData m_CachedSunStudyData;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
            m_MonthDialControl.labelConverter = m_MonthLabels;
            m_HourDialControl.labelConverter = m_HourLabels;
        }

        void Start()
        {
            m_HourDialControl.onSelectedValueChanged.AddListener(OnHourDialValueChanged);
            m_MonthDialControl.onSelectedValueChanged.AddListener(OnMonthDialValueChanged);
            m_ResetButton.onClick.AddListener(OnResetButtonClicked);

            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_SecondaryButton.onClick.AddListener(OnSecondaryButtonClicked);

            int min, day;
            (m_DefaultHour, min) = SunStudyUIController.GetHourMinute(UIStateManager.current.stateData.sunStudyData.timeOfDay);
            (m_DefaultMonth, day) = SunStudyUIController.GetMonthDay(DateTime.Now.Year, UIStateManager.current.stateData.sunStudyData.timeOfYear);
            m_HourDialControl.selectedValue = m_DefaultHour;
            m_MonthDialControl.selectedValue = m_DefaultMonth;
        }

        void OnStateDataChanged(UIStateData data)
        {
            if (data.activeToolbar != ToolbarType.TimeOfDayYearDial &&
                data.activeToolbar != ToolbarType.AltitudeAzimuthDial)
            {
                m_previousToolbar = data.activeToolbar;
            }

            if (m_CachedSunStudyData != data.sunStudyData)
            {
                m_HourDialControl.selectedValue = GetFloatFromMin(data.sunStudyData.timeOfDay);
                m_MonthDialControl.selectedValue = GetFloatFromDay(data.sunStudyData.timeOfYear);
                m_CachedSunStudyData = data.sunStudyData;
            }
        }

        void OnMonthDialValueChanged(float value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.timeOfYear = GetDayFromFloat(value);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }

        void OnHourDialValueChanged(float value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.timeOfDay = GetMinFromFloat(value);
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }

        void OnResetButtonClicked()
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.timeOfDay = m_DefaultHour;
            data.timeOfYear = m_DefaultMonth;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }

        void OnMainButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, m_previousToolbar));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, false));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.None;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
        }

        void OnSecondaryButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, true));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.AltitudeAzimuthDial));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SunstudyTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            var sunStudyData = UIStateManager.current.stateData.sunStudyData;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, AltitudeAzimuthRadialUIController.GetAltAzStatusMessage(sunStudyData)));
        }

        public static string GetTimeStatusMessage(SunStudyData sunStudyData)
        {
            var time = SunStudyUIController.GetFormattedMinuteOfDay(sunStudyData.timeOfDay);
            var day = SunStudyUIController.GetFormattedDayOfYear(DateTime.Now.Year, sunStudyData.timeOfYear);
            return $"{day},  " + time;
        }

        class MonthLabelConverter : ILabelConverter
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
        class HourLabelConverter : ILabelConverter
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
