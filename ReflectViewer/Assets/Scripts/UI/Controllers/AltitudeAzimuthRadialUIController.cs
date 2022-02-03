using SharpFlux;
using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
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

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.altitude), (alt) =>
            {
                m_AltitudeDialControl.selectedValue = alt;
            }));
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<int>(SunStudyContext.current, nameof(ISunstudyDataProvider.azimuth), (az) =>
            {
                m_AzimuthDialControl.selectedValue = az;
            }));
        }

        void Start()
        {
            m_AzimuthDialControl.onSelectedValueChanged.AddListener(OnAzimuthDialValueChanged);
            m_AltitudeDialControl.onSelectedValueChanged.AddListener(OnAltitudeDialValueChanged);
            m_ResetButton.onClick.AddListener(OnResetButtonClicked);

            m_MainButton.onClick.AddListener(OnMainButtonClicked);
            m_SecondaryButton.onClick.AddListener(OnSecondaryButtonClicked);
        }

        public static string GetAltAzStatusMessage(SunStudyData sunStudyData)
        {

            return "Altitude: " + Math.Round(sunStudyData.altitude, 2, MidpointRounding.AwayFromZero) +
                ", Azimuth: " + Math.Round(sunStudyData.azimuth, 2);
        }

        // See old commit for previous implementation
        void OnAltitudeDialValueChanged(float value)
        {

        }

        void OnAzimuthDialValueChanged(float value)
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
    }
}
