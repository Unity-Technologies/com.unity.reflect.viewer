using SharpFlux;
using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class SunStudyUIController: MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        MinMaxPropertyControl m_TimeOfDayControl;
        [SerializeField]
        MinMaxPropertyControl m_TimeOfYearControl;
        [SerializeField]
        MinMaxPropertyControl m_UtcOffsetControl;
        [SerializeField]
        MinMaxPropertyControl m_LatitudeControl;
        [SerializeField]
        MinMaxPropertyControl m_LongitudeControl;
        [SerializeField]
        MinMaxPropertyControl m_NorthAngleControl;
        [SerializeField]
        ToolButton m_DialogButton;
        [SerializeField]
        TextMeshProUGUI m_TimeOfDayText;
        [SerializeField]
        TextMeshProUGUI m_TimeOfYearText;
        [SerializeField]
        TextMeshProUGUI m_UtcOffsetText;
        [SerializeField]
        TextMeshProUGUI m_LatitudeText;
        [SerializeField]
        TextMeshProUGUI m_LongitudeText;
        [SerializeField]
        TextMeshProUGUI m_NorthAngleText;
#pragma warning restore CS0649

        DialogWindow m_DialogWindow;

        readonly string k_TimeOfDayQueryKey = "dm";
        readonly string k_TimeOfYearQueryKey = "yd";
        readonly string k_NorthAngleQueryKey = "na";
        readonly string k_UtcOffsetQueryKey = "uo";
        readonly string k_LatitudeQueryKey = "lat";
        readonly string k_LongitudeQueryKey = "lng";

        IUISelector m_ActiveDialogSelector;
        IUISelector<int> NorthAngleSelector;
        IUISelector<int> TimeOfDaySelector;
        IUISelector<int> TimeOfYearSelector;
        IUISelector<int> UtcOffsetSelector;
        IUISelector<int> LatitudeSelector;
        IUISelector<int> LongitudeSelector;

        public string NorthAngleQueryValue() => $"{NorthAngleSelector.GetValue()}";
        public string TimeOfDayQueryValue() => $"{TimeOfDaySelector.GetValue()}";
        public string TimeOfYearQueryValue() => $"{TimeOfYearSelector.GetValue()}";
        public string UtcOffsetQueryValue() => $"{UtcOffsetSelector.GetValue()}";
        public string LatitudeQueryValue() => $"{LatitudeSelector.GetValue()}";
        public string LongitudeQueryValue() => $"{LongitudeSelector.GetValue()}";

        public static string GetFormattedUtc(int offset)
        {
            offset -= (offset % 25); // only 15 minutes increments
            var UtcOffset = offset / 100f;
            var hours = (int) UtcOffset;
            var minutes = (int) Math.Abs((60 * (UtcOffset - hours)));

            return $"{hours}:{minutes:00}";
        }

        public static int GetMinuteOfDay(int hour, int minute)
        {
            return hour * 60 + minute;
        }

        public static (int hour, int min) GetHourMinute(int minuteOfDay)
        {
            minuteOfDay = Mathf.Clamp(minuteOfDay, 0, 1439);
            var hour = (minuteOfDay / 60);
            var minute = (minuteOfDay % 60);
            return (hour, minute);
        }

        public static string GetFormattedMinuteOfDay(int minuteOfDay)
        {
            minuteOfDay = Mathf.Clamp(minuteOfDay, 0, 1439);
            var hour = (minuteOfDay / 60);
            var minute = (minuteOfDay % 60);
            return $"{hour}:{minute:00}";
        }

        public static int GetDayOfYear(int year, int month, int day)
        {
            day = Mathf.Clamp(day, 1, DateTime.DaysInMonth(year, month));
            return new DateTime(year, month, day).DayOfYear;
        }

        public static (int month, int day) GetMonthDay(int year, int dayOfYear)
        {
            dayOfYear = Mathf.Clamp(dayOfYear, 1, GetDayOfYear(year, 12, 31));
            var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);
            return (date.Month, date.Day);
        }

        public static string GetFormattedDayOfYear(int year, int dayOfYear)
        {
            dayOfYear = Mathf.Clamp(dayOfYear, 1, GetDayOfYear(year, 12, 31));
            var date = new DateTime(year, 1, 1).AddDays(dayOfYear - 1);

            return $"{NameOfMonth(date.Month)} {date.Day}";
        }

        public static string NameOfMonth(int monthNb)
        {
            switch (monthNb)
            {
                case 1:
                    return "Jan";
                case 2:
                    return "Feb";
                case 3:
                    return "Mar";
                case 4:
                    return "Apr";
                case 5:
                    return "May";
                case 6:
                    return "Jun";
                case 7:
                    return "Jul";
                case 8:
                    return "Aug";
                case 9:
                    return "Sept";
                case 10:
                    return "Oct";
                case 11:
                    return "Nov";
                case 12:
                    return "Dec";
                case 13:
                    return ""; // empty tick for dial, for days in December
                default:
                    return "Dec";
            }
        }

        void Awake()
        {
            m_DialogWindow = GetComponent<DialogWindow>();

            m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog),
                type =>
                {
                    m_DialogButton.selected = type == OpenDialogAction.DialogType.SunStudy;
                });

            TimeOfDaySelector = UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.timeOfDay), OnTimeOfDayChanged);
            TimeOfYearSelector = UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.timeOfYear), OnTimeOfYearChanged);
            UtcOffsetSelector = UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.utcOffset), OnUtcChanged);
            LatitudeSelector = UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.latitude), OnLatitudeChanged);
            LongitudeSelector = UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.longitude), OnLongitudeChanged);
            NorthAngleSelector = UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.northAngle), OnNorthAngleChanged);

            // Initialize controls with current values
            OnTimeOfDayChanged(TimeOfDaySelector.GetValue());
            OnTimeOfYearChanged(TimeOfYearSelector.GetValue());
            OnUtcChanged(UtcOffsetSelector.GetValue());
            OnLatitudeChanged(LatitudeSelector.GetValue());
            OnLongitudeChanged(LongitudeSelector.GetValue());
            OnNorthAngleChanged(NorthAngleSelector.GetValue());

            QueryArgHandler.Register(this, k_NorthAngleQueryKey, OnNorthAngleControlChanged, NorthAngleQueryValue);
            QueryArgHandler.Register(this, k_TimeOfDayQueryKey, OnTimeOfDayControlChanged, TimeOfDayQueryValue);
            QueryArgHandler.Register(this, k_TimeOfYearQueryKey, OnTimeOfYearControlChanged, TimeOfYearQueryValue);
            QueryArgHandler.Register(this, k_UtcOffsetQueryKey, OnUtcOffsetControlChanged, UtcOffsetQueryValue);
            QueryArgHandler.Register(this, k_LatitudeQueryKey, OnLatitudeControlChanged, LatitudeQueryValue);
            QueryArgHandler.Register(this, k_LongitudeQueryKey, OnLongitudeControlChanged, LongitudeQueryValue);
        }

        void OnDestroy()
        {
            m_ActiveDialogSelector?.Dispose();
            TimeOfDaySelector?.Dispose();
            TimeOfYearSelector?.Dispose();
            UtcOffsetSelector?.Dispose();
            LatitudeSelector?.Dispose();
            LongitudeSelector?.Dispose();
            NorthAngleSelector?.Dispose();

            m_DialogButton.buttonClicked -= OnDialogButtonClicked;

            QueryArgHandler.Unregister(this);
        }

        void Start()
        {
            m_DialogButton.buttonClicked += OnDialogButtonClicked;
            m_TimeOfDayControl.onIntValueChanged.AddListener(OnTimeOfDayControlChanged);
            m_TimeOfYearControl.onIntValueChanged.AddListener(OnTimeOfYearControlChanged);
            m_UtcOffsetControl.onIntValueChanged.AddListener(OnUtcOffsetControlChanged);
            m_LatitudeControl.onIntValueChanged.AddListener(OnLatitudeControlChanged);
            m_LongitudeControl.onIntValueChanged.AddListener(OnLongitudeControlChanged);
            m_NorthAngleControl.onIntValueChanged.AddListener(OnNorthAngleControlChanged);
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? OpenDialogAction.DialogType.None : OpenDialogAction.DialogType.SunStudy;

            Dispatcher.Dispatch(OpenDialogAction.From(dialogType));
            Dispatcher.Dispatch(OpenSubDialogAction.From(OpenDialogAction.DialogType.None));

            // TODO Always change mode back to Geographic since dial will be Time or closed
        }

        void OnTimeOfDayControlChanged(int value)
        {
            Dispatcher.Dispatch(SetTimeOfDayAction.From(value));
        }

        void OnTimeOfYearControlChanged(int value)
        {
            Dispatcher.Dispatch(SetTimeOfYearAction.From(value));
        }

        void OnUtcOffsetControlChanged(int value)
        {
            Dispatcher.Dispatch(SetUtcAction.From(value));
        }

        void OnLatitudeControlChanged(int value)
        {
            Dispatcher.Dispatch(SetLatitudeAction.From(value));
        }

        void OnLongitudeControlChanged(int value)
        {
            Dispatcher.Dispatch(SetLongitudeAction.From(value));
        }

        void OnNorthAngleControlChanged(int value)
        {
            Dispatcher.Dispatch(SetNorthAngleAction.From(value));
        }

        void OnTimeOfDayChanged(int timeOfDay)
        {
            m_TimeOfDayControl.SetValue(timeOfDay);
            m_TimeOfDayText.SetText(GetFormattedMinuteOfDay(timeOfDay));
        }

        void OnTimeOfYearChanged(int timeOfYear)
        {
            m_TimeOfYearControl.SetValue(timeOfYear);
            m_TimeOfYearText.SetText(GetFormattedDayOfYear(DateTime.Now.Year, timeOfYear));
        }

        void OnUtcChanged(int utc)
        {
            m_UtcOffsetControl.SetValue(utc);
            m_UtcOffsetText.SetText(GetFormattedUtc(utc));
        }

        void OnLatitudeChanged(int latitude)
        {
            m_LatitudeControl.SetValue(latitude);
            m_LatitudeText.SetText($"{latitude}");
        }

        void OnLongitudeChanged(int longitude)
        {
            m_LongitudeControl.SetValue(longitude);
            m_LongitudeText.SetText($"{longitude}");
        }

        void OnNorthAngleChanged(int northAngle)
        {
            m_NorthAngleControl.SetValue(northAngle);
            m_NorthAngleText.SetText($"{northAngle}");
        }
    }
}
