using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.UI
{
    public class MarkerListButtonControl : MonoBehaviour
    {
        [SerializeField]
        ButtonControl m_ButtonControl;
        [SerializeField]
        GameObject m_SelectIcon;
        [SerializeField]
        GameObject m_ActiveMarkerIcon;
        [SerializeField]
        RawImage m_MarkerThumbnail;

        event Action<SyncId> m_OnClick;
        SyncId m_Id;
        void Start()
        {
            m_ButtonControl.onControlUp.AddListener(HandleControlUp);
        }

        void OnDestroy()
        {
            m_ButtonControl.onControlUp.RemoveListener(HandleControlUp);
        }

        void HandleControlUp(BaseEventData evt)
        {
            m_OnClick?.Invoke(m_Id);
        }

        public void Select(bool state)
        {
            m_SelectIcon.SetActive(state);
        }

        public void ActiveMarker(bool state)
        {
            m_ActiveMarkerIcon.SetActive(state);
        }

        public void SetThumbnail(Texture2D texture)
        {
            if (texture)
            {
                m_MarkerThumbnail.gameObject.SetActive(true);
                m_MarkerThumbnail.texture = texture;
            }
            else
            {
                m_MarkerThumbnail.gameObject.SetActive(false);
            }
        }

        public void SetLabel(string text)
        {
            m_ButtonControl.text = text;
        }

        public void Configure(Action<SyncId> action, SyncId id)
        {
            m_Id = id;
            m_OnClick += action;
        }

        public void Dispose()
        {
            m_OnClick = null;
        }
    }
}
