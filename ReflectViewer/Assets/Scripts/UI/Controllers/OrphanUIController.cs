using System;
using SharpFlux;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [ExecuteAlways]
    class OrphanUIController : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_SyncButton;
        [SerializeField]
        Sprite m_SyncEnabledSprite;
        [SerializeField]
        Sprite m_SyncDisabledSprite;
#pragma warning restore CS0649

        void Awake()
        {
            UIStateManager.stateChanged += UIStateManagerOnStateChanged;
            m_SyncButton.onClick.AddListener(OnSyncButtonClick);
        }

        bool m_Pressed;
        DateTime m_Time;
        void Update()
        {
            if (!m_Pressed && IsTouchStart())
            {
                m_Pressed = true;
                m_Time = DateTime.Now;
            }

            if (m_Pressed)
            {
                if ((DateTime.Now - m_Time).TotalSeconds > 0.2f)
                {
                    m_Pressed = false;
                }
                else if (IsTouchEnd())
                {
                    m_Pressed = false;

                    // we don't close any dialog with tapping the screen anymore.
                    // UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.CloseAllDialogs, null));
                }
            }
        }

        void UIStateManagerOnStateChanged(UIStateData data)
        {
            m_SyncButton.interactable = data.toolbarsEnabled;

            var syncButtonSprite = data.syncEnabled ? m_SyncEnabledSprite : m_SyncDisabledSprite;
            m_SyncButton.image.sprite = syncButtonSprite;
        }

        void OnSyncButtonClick()
        {
            var enabled = !UIStateManager.current.stateData.syncEnabled;
            UIStateManager.current.Dispatcher.Dispatch(Payload<ActionTypes>.From(ActionTypes.SetSync, enabled));
        }

        bool IsTouchStart()
        {
            var id = -1;
            var pressed = false;


            if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                id = Input.GetTouch(0).fingerId;
                pressed = true;
            }

            if (!pressed)
            {
                pressed = Input.GetMouseButtonDown(0);
            }

            if (pressed)
            {
                pressed = !EventSystem.current.IsPointerOverGameObject(id);
            }

            return pressed;
        }

        bool IsTouchEnd()
        {
            var touchEnd = Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Ended;

            if (!touchEnd)
            {
                touchEnd = Input.GetMouseButtonUp(0);
            }
            return touchEnd;
        }

    }
}
