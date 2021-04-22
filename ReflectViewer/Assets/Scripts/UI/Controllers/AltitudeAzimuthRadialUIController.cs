using SharpFlux;
using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class AltitudeAzimuthRadialUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        DialControl m_AzimuthDialControl;
        [SerializeField]
        DialControl m_AltitudeDialControl;
        [SerializeField]
        Button m_MainButton;
        [SerializeField]
        Button m_SecondaryButton;
        [SerializeField]
        Button m_ResetButton;
#pragma warning restore CS0649

        float m_DefaultAzimuth;
        float m_DefaultAltitude;

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            m_AzimuthDialControl.onSelectedValueChanged.AddListener(OnAzimuthDialValueChanged);
            m_AltitudeDialControl.onSelectedValueChanged.AddListener(OnAltitudeDialValueChanged);
            m_ResetButton.onClick.AddListener(OnResetButtonClicked);

            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_SecondaryButton.onClick.AddListener(OnSecondaryButtonClicked);

            m_DefaultAzimuth = UIStateManager.current.stateData.sunStudyData.azimuth;
            m_DefaultAltitude = UIStateManager.current.stateData.sunStudyData.altitude;
            m_AzimuthDialControl.selectedValue = m_DefaultAzimuth;
            m_AltitudeDialControl.selectedValue = m_DefaultAltitude;
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_AzimuthDialControl.selectedValue = data.sunStudyData.azimuth;
            m_AltitudeDialControl.selectedValue = data.sunStudyData.altitude;
        }

        public static string GetAltAzStatusMessage(SunStudyData sunStudyData)
        {

            return "Altitude: " + Math.Round(sunStudyData.altitude, 2, MidpointRounding.AwayFromZero) +
                ", Azimuth: " + Math.Round(sunStudyData.azimuth, 2);
        }

        void OnAltitudeDialValueChanged(float value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.altitude = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }

        void OnAzimuthDialValueChanged(float value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.azimuth = value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }

        void OnResetButtonClicked()
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.azimuth = m_DefaultAzimuth;
            data.altitude = m_DefaultAltitude;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
        }

        void OnMainButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, TimeRadialUIController.m_previousToolbar));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.None;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
        }

        void OnSecondaryButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudyMode, false));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.TimeOfDayYearDial));
            var toolState = UIStateManager.current.stateData.toolState;
            toolState.activeTool = ToolType.SunstudyTool;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetToolState, toolState));
            var sunStudyData = UIStateManager.current.stateData.sunStudyData;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, TimeRadialUIController.GetTimeStatusMessage(sunStudyData)));
        }
    }
}
