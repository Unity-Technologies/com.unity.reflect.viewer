using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class DebugOptionsUIController: MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        SlideToggle m_GesturesTrackingToggle;
        [SerializeField]
        SlideToggle m_ARAxisTrackingToggle;
        [SerializeField]
        TextMeshProUGUI m_QualitySettingValue;

        [SerializeField]
        SlideToggle m_ExampleFlagToggle;
        [SerializeField]
        TextMeshProUGUI m_ExampleTextSettingValue;
#pragma warning restore 649

        List<IDisposable> m_DisposeOnDestroy = new List<IDisposable>();

        void Awake()
        {
            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<string>(ApplicationContext.current, nameof(IQualitySettingsDataProvider.qualityLevel), (qualityLevel) =>
             {
                 m_QualitySettingValue.text = qualityLevel;
             }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.gesturesTrackingEnabled), (gesturesTrackingEnabled) =>
             {
                 m_GesturesTrackingToggle.on = gesturesTrackingEnabled;
             }));

            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, nameof(IDebugOptionDataProvider.ARAxisTrackingEnabled), (ARAxisTrackingEnabled) =>
             {
                 m_ARAxisTrackingToggle.on = ARAxisTrackingEnabled;
             }));


            m_DisposeOnDestroy.Add(UISelectorFactory.createSelector<bool>(DebugOptionContext.current, "ARAxisTrackingEnabled", (ARAxisTrackingEnabled) =>
            {
                m_ARAxisTrackingToggle.on = ARAxisTrackingEnabled;
            }));
        }

        void OnDestroy()
        {
            m_DisposeOnDestroy.ForEach(x => x.Dispose());
        }

        void Start()
        {
            m_GesturesTrackingToggle.onValueChanged.AddListener(OnGesturesTrackingToggleChanged);
            m_ARAxisTrackingToggle.onValueChanged.AddListener(OnARAxisTrackingToggleChanged);
        }

        static void OnGesturesTrackingToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new { gesturesTrackingEnabled = on }));
        }

        static void OnARAxisTrackingToggleChanged(bool on)
        {
            Dispatcher.Dispatch(SetDebugOptionsAction.From(new { ARAxisTrackingEnabled = on }));
        }
    }
}
