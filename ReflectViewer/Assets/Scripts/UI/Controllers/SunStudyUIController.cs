using SharpFlux;
using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class SunStudyUIController : MonoBehaviour
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
        Button m_DialogButton;
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

        [SerializeField]
        GameObject m_Panel;
        [SerializeField]
        TextMeshProUGUI m_TimeOfDayLabel;
        [SerializeField]
        TextMeshProUGUI m_TimeOfYearLabel;
        [SerializeField]
        TextMeshProUGUI m_UtcOffsetLabel;
        [SerializeField]
        TextMeshProUGUI m_LatitudeLabel;
        [SerializeField]
        TextMeshProUGUI m_LongitudeLabel;
        [SerializeField]
        Image m_TimeOfDaySlider;
        [SerializeField]
        Image m_TimeOfYearSlider;
        [SerializeField]
        Image m_UtcOffsetSlider;
        [SerializeField]
        Image m_LatitudeSlider;
        [SerializeField]
        Image m_LongitudeSlider;

#pragma warning restore CS0649

        DialogWindow m_DialogWindow;
        Image m_DialogButtonImage;
        SunStudyData m_CurrentSunStudyData;
        Color m_TextColor, m_SliderColor, m_SliderBackgroundColor;
        bool m_CurrentIsStaticMode;

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

        public static (int hour, int min) GetHourMinute (int minuteOfDay)
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

        void ChangeSliderAlpha(TextMeshProUGUI label, TextMeshProUGUI text, MinMaxPropertyControl control, Image slider, float alpha)
        {
            var textColor = m_TextColor;
            var sliderColor = m_SliderColor;
            var sliderBackColor = m_SliderBackgroundColor;
            textColor.a = alpha;
            sliderColor.a = alpha;
            sliderBackColor.a = alpha;

            label.color = textColor;
            text.color = textColor;
            control.GetComponent<Image>().color = sliderBackColor;
            slider.color = sliderColor;
        }

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;

            m_DialogButtonImage = m_DialogButton.GetComponent<Image>();
            m_DialogWindow = GetComponent<DialogWindow>();

            m_TextColor = m_TimeOfDayLabel.color;
            m_SliderBackgroundColor = m_TimeOfDayControl.GetComponent<Image>().color;
            m_SliderColor = m_TimeOfDaySlider.color;
        }

        void Start()
        {
            m_DialogButton.onClick.AddListener(OnDialogButtonClicked);
            m_TimeOfDayControl.onIntValueChanged.AddListener(OnTimeOfDayControlChanged);
            m_TimeOfYearControl.onIntValueChanged.AddListener(OnTimeOfYearControlChanged);
            m_UtcOffsetControl.onIntValueChanged.AddListener(OnUtcOffsetControlChanged);
            m_LatitudeControl.onIntValueChanged.AddListener(OnLatitudeControlChanged);
            m_LongitudeControl.onIntValueChanged.AddListener(OnLongitudeControlChanged);
            m_NorthAngleControl.onIntValueChanged.AddListener(OnNorthAngleControlChanged);
        }

        void OnDialogButtonClicked()
        {
            var dialogType = m_DialogWindow.open ? DialogType.None : DialogType.SunStudy;
            var toolbarType = m_DialogWindow.open ? TimeRadialUIController.m_previousToolbar : ToolbarType.TimeOfDayYearDial;
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = m_DialogWindow.open ? ToolType.None : ToolType.SunstudyTool;

            var data = UIStateManager.current.stateData;
            if (m_DialogWindow.open || data.dialogMode == DialogMode.Help)
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
            else
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, TimeRadialUIController.GetTimeStatusMessage(data.sunStudyData)));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenDialog, dialogType));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.OpenSubDialog, DialogType.None));

            // don't use dial in VR or Help Mode
            if (data.navigationState.navigationMode != NavigationMode.VR && data.dialogMode != DialogMode.Help)
            {
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, toolbarType));
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            }
            // Always change mode back to Geographic since dial will be Time or closed
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, false));
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_DialogButtonImage.enabled = data.activeDialog == DialogType.SunStudy;

            if (m_CurrentSunStudyData == data.sunStudyData)
            {
                return;
            }

            if (m_CurrentIsStaticMode != data.sunStudyData.isStaticMode)
            {
                if (data.sunStudyData.isStaticMode)
                {
                    m_Panel.SetActive(true);
                    ChangeSliderAlpha(m_TimeOfDayLabel, m_TimeOfDayText, m_TimeOfDayControl, m_TimeOfDaySlider, 0.5f);
                    ChangeSliderAlpha(m_TimeOfYearLabel, m_TimeOfYearText, m_TimeOfYearControl, m_TimeOfYearSlider, 0.5f);
                    ChangeSliderAlpha(m_UtcOffsetLabel, m_UtcOffsetText, m_UtcOffsetControl, m_UtcOffsetSlider, 0.5f);
                    ChangeSliderAlpha(m_LatitudeLabel, m_LatitudeText, m_LatitudeControl, m_LatitudeSlider, 0.5f);
                    ChangeSliderAlpha(m_LongitudeLabel, m_LongitudeText, m_LongitudeControl, m_LongitudeSlider, 0.5f);
                }
                else
                {
                    m_Panel.SetActive(false);
                    ChangeSliderAlpha(m_TimeOfDayLabel, m_TimeOfDayText, m_TimeOfDayControl, m_TimeOfDaySlider, 1f);
                    ChangeSliderAlpha(m_TimeOfYearLabel, m_TimeOfYearText, m_TimeOfYearControl, m_TimeOfYearSlider, 1f);
                    ChangeSliderAlpha(m_UtcOffsetLabel, m_UtcOffsetText, m_UtcOffsetControl, m_UtcOffsetSlider, 1f);
                    ChangeSliderAlpha(m_LatitudeLabel, m_LatitudeText, m_LatitudeControl, m_LatitudeSlider, 1f);
                    ChangeSliderAlpha(m_LongitudeLabel, m_LongitudeText, m_LongitudeControl, m_LongitudeSlider, 1f);
                }
                m_CurrentIsStaticMode = data.sunStudyData.isStaticMode;
            }

            m_TimeOfDayControl.SetValue(data.sunStudyData.timeOfDay);
            m_TimeOfYearControl.SetValue(data.sunStudyData.timeOfYear);
            m_UtcOffsetControl.SetValue(data.sunStudyData.utcOffset);
            m_LatitudeControl.SetValue(data.sunStudyData.latitude);
            m_LongitudeControl.SetValue(data.sunStudyData.longitude);
            m_NorthAngleControl.SetValue(data.sunStudyData.northAngle);

            m_TimeOfDayText.SetText(GetFormattedMinuteOfDay(data.sunStudyData.timeOfDay));
            m_TimeOfYearText.SetText(GetFormattedDayOfYear(DateTime.Now.Year, data.sunStudyData.timeOfYear));
            m_UtcOffsetText.SetText(GetFormattedUtc(data.sunStudyData.utcOffset));
            m_LatitudeText.SetText($"{data.sunStudyData.latitude}");
            m_LongitudeText.SetText($"{data.sunStudyData.longitude}");
            m_NorthAngleText.SetText($"{data.sunStudyData.northAngle}");

            m_CurrentSunStudyData = data.sunStudyData;
        }

        void OnTimeOfDayControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            if (!data.isStaticMode)
            {
                data.timeOfDay = value;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            }
        }

        void OnTimeOfYearControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            if (!data.isStaticMode)
            {
                data.timeOfYear = value;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            }
        }

        void OnUtcOffsetControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            if (!data.isStaticMode)
            {
                data.utcOffset = value;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            }
        }

        void OnLatitudeControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            if (!data.isStaticMode)
            {
                data.latitude = value;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            }
        }

        void OnLongitudeControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            if (!data.isStaticMode)
            {
                data.longitude = value;
                Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            }
        }

        void OnNorthAngleControlChanged(int value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.northAngle = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }
    }
}
