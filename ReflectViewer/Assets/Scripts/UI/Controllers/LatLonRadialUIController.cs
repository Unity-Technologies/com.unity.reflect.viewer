using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;
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

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.latitude), (lat) =>
                {
                    m_LatitudeDialControl.selectedValue = lat;
                }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.longitude), (lon) =>
            {
                m_LongitudeDialControl.selectedValue = lon;
            }));
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

        // See old commit for previous implementation
        void OnLatitudeDialValueChanged(float value)
        {

        }

        void OnLongitudeDialValueChanged(float value)
        {

        }

        void OnRefreshButtonClicked()
        {

        }

        void onToolButtonClicked()
        {
            // TODO: on close, display previous ToolbarType instead of ORBIT Sidebar!
            //var toolbarType = m_DialogWindow.open ? ToolbarType.OrbitSidebar : ToolbarType.SunStudyDial;
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.AltitudeAzimuthDial));
        }

        void OnMainButtonClicked()
        {
            // TODO: on close, display previous ToolbarType instead of ORBIT Sidebar!
            //var toolbarType = m_DialogWindow.open ? ToolbarType.OrbitSidebar : ToolbarType.SunStudyDial;
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.OrbitSidebar));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
        }

        void OnSecondaryButtonClicked()
        {
            Dispatcher.Dispatch(SetActiveToolBarAction.From(SetActiveToolBarAction.ToolbarType.AltitudeAzimuthDial));
            Dispatcher.Dispatch(ClearStatusAction.From(true));
            Dispatcher.Dispatch(ClearStatusAction.From(false));
        }
    }
}
