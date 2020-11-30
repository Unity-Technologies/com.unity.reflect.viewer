using SharpFlux;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class DebugOptionsUIController : MonoBehaviour
    {
#pragma warning disable 649
        [SerializeField]
        SlideToggle m_GesturesTrackingToggle;
        [SerializeField]
        SlideToggle m_ARAxisTrackingToggle;
#pragma warning restore 649

        DialogWindow m_DialogWindow;
        DebugOptionsData? m_CurrentsDebugOptionsData;

        void Awake()
        {
            UIStateManager.debugStateChanged += OnDebugStateChanged;
            m_DialogWindow = GetComponent<DialogWindow>();
        }

        void Start()
        {
            m_GesturesTrackingToggle.onValueChanged.AddListener(OnGesturesTrackingToggleChanged);
            m_ARAxisTrackingToggle.onValueChanged.AddListener(OnARAxisTrackingToggleChanged);
        }

        void OnDebugStateChanged(UIDebugStateData data)
        {
            if (m_CurrentsDebugOptionsData == data.debugOptionsData)
                return;

            m_GesturesTrackingToggle.on = data.debugOptionsData.gesturesTrackingEnabled;

            m_ARAxisTrackingToggle.on = data.debugOptionsData.ARAxisTrackingEnabled;

            m_CurrentsDebugOptionsData = data.debugOptionsData;
        }

        void OnGesturesTrackingToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.gesturesTrackingEnabled = on;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
        }

        void OnARAxisTrackingToggleChanged(bool on)
        {
            var data = UIStateManager.current.debugStateData.debugOptionsData;
            data.ARAxisTrackingEnabled = on;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetDebugOptions, data));
        }
    }
}
