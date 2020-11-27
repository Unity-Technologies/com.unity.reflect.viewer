using System;
using SharpFlux;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    class OrphanUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        RectTransform m_TapDetectorRect;
        [SerializeField]
        Button m_SyncButton;
        [SerializeField]
        Sprite m_SyncEnabledSprite;
        [SerializeField]
        Sprite m_SyncDisabledSprite;
#pragma warning restore CS0649

        public delegate void BaseEventDataHandler(BaseEventData evt);
        public static event BaseEventDataHandler onPointerClick;
        public static event BaseEventDataHandler onPointerDown;
        public static event BaseEventDataHandler onPointerUp;
        public static event BaseEventDataHandler onDrag;
        public static event BaseEventDataHandler onBeginDrag;
        public static event BaseEventDataHandler onEndDrag;

        static bool s_IsPressed;
        static bool s_IsPointed;

        public static bool isPointBlockedByUI => !s_IsPointed;
        public static bool isTouchBlockedByUI => !s_IsPressed;

        bool? m_CachedSyndEnabled;

        void Start()
        {
            // SetupInterceptorsIfNeeded();
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnPointerEnter, EventTriggerType.PointerEnter);
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnPointerExit, EventTriggerType.PointerExit);

            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnPointerClick, EventTriggerType.PointerClick);
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnPointerDown, EventTriggerType.PointerDown);
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnPointerUp, EventTriggerType.PointerUp);
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnDrag, EventTriggerType.Drag);
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnBeginDrag, EventTriggerType.BeginDrag);
            EventTriggerUtility.CreateEventTrigger(m_TapDetectorRect.gameObject, OnEndDrag, EventTriggerType.EndDrag);


            UIStateManager.stateChanged += OnStateDataChanged;
            m_SyncButton.onClick.AddListener(OnSyncButtonClick);
        }


        void OnPointerEnter(BaseEventData eventData)
        {
            s_IsPointed = true;
        }


        void OnPointerExit(BaseEventData eventData)
        {
            s_IsPointed = false;
        }

        void OnPointerClick(BaseEventData eventData)
        {
            onPointerClick?.Invoke(eventData);
        }

        void OnPointerDown(BaseEventData eventData)
        {
            s_IsPressed = true;
            onPointerDown?.Invoke(eventData);
        }

        void OnPointerUp(BaseEventData eventData)
        {
            s_IsPressed = false;
            onPointerUp?.Invoke(eventData);
        }

        void OnDrag(BaseEventData eventData)
        {
            onDrag?.Invoke(eventData);
        }

        void OnBeginDrag(BaseEventData eventData)
        {
            onBeginDrag?.Invoke(eventData);

        }

        void OnEndDrag(BaseEventData eventData)
        {
            onEndDrag?.Invoke(eventData);
        }

        void OnStateDataChanged(UIStateData data)
        {
            m_SyncButton.interactable = data.toolbarsEnabled;

            if (m_CachedSyndEnabled != data.syncEnabled)
            {
                m_SyncButton.image.sprite = data.syncEnabled ? m_SyncEnabledSprite : m_SyncDisabledSprite;
                m_CachedSyndEnabled = data.syncEnabled;
            }
        }

        void OnSyncButtonClick()
        {
            // Helpmode
            if (HelpDialogController.SetHelpID(HelpModeEntryID.Sync)) return;

            var enabled = !UIStateManager.current.stateData.syncEnabled;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSync, enabled));
        }
    }
}
