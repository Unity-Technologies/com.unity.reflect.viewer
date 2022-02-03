using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.UI
{

    public class MarkerGraphicProperty
    {
        public SyncId Id;

        public MarkerGraphicProperty(IMarker marker)
        {
            Id = marker.Id;
        }

        public override string ToString()
        {
            return "Marker ID: " + Id;
        }
    }

    public class MarkerGraphicPropertyValue : MonoBehaviour, IPropertyValue<MarkerGraphicProperty>
    {
        [SerializeField]
        RawImage m_MarkerImage;
        [SerializeField]
        ButtonControl m_PrintButton;
        MarkerGraphicProperty m_Value;
        MarkerGraphicManager m_GraphicManager;
        public Type type => typeof(MarkerGraphicProperty);

        public object objectValue
        {
            get => m_Value;
            set
            {
                m_Value = (MarkerGraphicProperty)value;
                GenerateGraphic();
            }
        }

        UnityEvent<MarkerGraphicProperty> m_OnValueChanged = new UnityEvent<MarkerGraphicProperty>();
        Dictionary<Action, UnityAction<MarkerGraphicProperty>> m_Handlers = new Dictionary<Action, UnityAction<MarkerGraphicProperty>>();



        public MarkerGraphicProperty value
        {
            get => m_Value;
            private set
            {
                m_Value = value;
                GenerateGraphic();
                m_OnValueChanged?.Invoke(m_Value);
            }
        }

        void Start()
        {
            if (m_PrintButton)
                m_PrintButton.onControlUp.AddListener(HandlePrintButton);
        }

        void OnDestroy()
        {
            if (m_PrintButton)
                m_PrintButton.onControlUp.RemoveListener(HandlePrintButton);
        }

        void GenerateGraphic()
        {
            if (m_Value == null)
                return;
            if (!m_GraphicManager)
                m_GraphicManager = FindObjectOfType<MarkerGraphicManager>();
            StartCoroutine(DisplayGraphicWhenAvailable());
        }

        IEnumerator DisplayGraphicWhenAvailable()
        {
            yield return new WaitUntil(()=>m_GraphicManager.GraphicsAvailable);
            var graphic = m_GraphicManager.GeneratedGraphics[m_Value.Id];
            Texture2D newTexture = new Texture2D(graphic.Item2, graphic.Item3, graphic.Item4, false, true);
            newTexture.SetPixels32(graphic.Item1);
            newTexture.Apply();
            yield return null;
            m_MarkerImage.texture = newTexture;
        }

        void HandlePrintButton(BaseEventData evt)
        {
            Print();
        }

        void Print()
        {
            m_GraphicManager.PrintMarker(m_Value.Id);
        }

        /// <summary>
        /// Trigger event when value changes
        /// </summary>
        /// <param name="eventFunc"></param>
        public void AddListener(Action eventFunc)
        {
            m_Handlers[eventFunc] = (newValue) =>
            {
                eventFunc();
            };
            m_OnValueChanged.AddListener(m_Handlers[eventFunc]);
        }

        /// <summary>
        /// Remove value change event
        /// </summary>
        /// <param name="eventFunc"></param>
        public void RemoveListener(Action eventFunc)
        {
            m_OnValueChanged.RemoveListener(m_Handlers[eventFunc]);
            m_Handlers.Remove(eventFunc);
        }

    }
}
