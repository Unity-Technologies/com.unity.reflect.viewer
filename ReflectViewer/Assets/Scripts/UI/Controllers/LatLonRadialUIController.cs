using SharpFlux;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    public class LatLonRadialUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        DialControl m_LongitudeDialControl;
        [SerializeField]
        DialControl m_LatitudeDialControl;
        [SerializeField]
        Button m_SunstudyToolButton;
        [SerializeField]
        Button m_MainButton;
        [SerializeField]
        Button m_SecondaryButton;
        [SerializeField]
        Button m_RefreshButton;
#pragma warning restore CS0649

        void Awake()
        {
            UIStateManager.stateChanged += OnStateDataChanged;
        }

        void Start()
        {
            m_LongitudeDialControl.onSelectedValueChanged.AddListener(OnLongitudeDialValueChanged);
            m_LatitudeDialControl.onSelectedValueChanged.AddListener(OnLatitudeDialValueChanged);
            m_SunstudyToolButton.onClick.AddListener(onToolButtonClicked);
            m_RefreshButton.onClick.AddListener(OnRefreshButtonClicked);

            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_SecondaryButton.onClick.AddListener(OnSecondaryButtonClicked);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_LongitudeDialControl.selectedValue = data.sunStudyData.longitude;
            m_LatitudeDialControl.selectedValue = data.sunStudyData.latitude;
        }

        void OnLatitudeDialValueChanged(float value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.latitude = (int)value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            var Message = "Latitude: " + data.latitude + ", Longitude: " + data.longitude;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, Message));
        }

        void OnLongitudeDialValueChanged(float value)
        {
            var data = UIStateManager.current.stateData.sunStudyData;
            data.longitude = (int)value;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSunStudy, data));
            var Message = "Latitude: " + data.latitude + ", Longitude: " + data.longitude;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetStatusMessage, Message));
        }

        void OnRefreshButtonClicked()
        {

        }

        void onToolButtonClicked()
        {
            // TODO: on close, display previous ToolbarType instead of ORBIT Sidebar!
            //var toolbarType = m_DialogWindow.open ? ToolbarType.OrbitSidebar : ToolbarType.SunStudyDial;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.AltitudeAzimuthDial));
        }

        void OnMainButtonClicked()
        {
            // TODO: on close, display previous ToolbarType instead of ORBIT Sidebar!
            //var toolbarType = m_DialogWindow.open ? ToolbarType.OrbitSidebar : ToolbarType.SunStudyDial;
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.OrbitSidebar));
            //Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveTool, ToolType.None));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
        }

        void OnSecondaryButtonClicked()
        {
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetActiveToolbar, ToolbarType.AltitudeAzimuthDial));
            Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.ClearStatus, null));
        }
    }
}
