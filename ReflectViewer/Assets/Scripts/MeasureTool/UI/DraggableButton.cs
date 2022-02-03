using System;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UnityEngine.Reflect.MeasureTool
{
    public class DraggableButton : MonoBehaviour
    {
#pragma warning disable CS0649
        [SerializeField]
        Button m_Button;
#pragma warning restore CS0649

        bool m_Selected;

        public Button button => m_Button;

        public class DragEvent : UnityEvent<Vector3>{}

        [SerializeField]
        DragEvent m_OnBeginDrag = new DragEvent();
        [SerializeField]
        DragEvent m_OnDrag = new DragEvent();
        [SerializeField]
        DragEvent m_OnEndDrag = new DragEvent();

        public DragEvent onBeginDrag => m_OnBeginDrag;
        public DragEvent onDrag => m_OnDrag;
        public DragEvent onEndDrag => m_OnEndDrag;

        public void Start()
        {
            EventTriggerUtility.CreateEventTrigger(m_Button.gameObject, OnBeginDrag, EventTriggerType.BeginDrag);
            EventTriggerUtility.CreateEventTrigger(m_Button.gameObject, OnDrag, EventTriggerType.Drag);
            EventTriggerUtility.CreateEventTrigger(m_Button.gameObject, OnEndDrag, EventTriggerType.EndDrag);
        }

        void OnBeginDrag(BaseEventData eventData)
        {
            m_OnBeginDrag?.Invoke(((PointerEventData)eventData).position);
        }

        void OnDrag(BaseEventData eventData)
        {
            m_OnDrag?.Invoke(((PointerEventData)eventData).position);
        }

        void OnEndDrag(BaseEventData eventData)
        {
            m_OnEndDrag?.Invoke(((PointerEventData)eventData).position);
        }
    }
}
