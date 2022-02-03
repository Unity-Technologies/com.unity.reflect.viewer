using System;
using System.Collections;
using System.Collections.Generic;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Unity.Reflect.Markers.UI
{
    public struct MarkerActions
    {
        public Action Save;
        public Action Export;
        public Action Delete;
    }

    public class MarkerActionsPropertyValue : MonoBehaviour, IPropertyValue<MarkerActions>
    {
        [SerializeField]
        ButtonControl m_SaveButton;

        [SerializeField]
        ButtonControl m_ExportButton;

        [SerializeField]
        ButtonControl m_DeleteButton;

        #region PropertyValue

        MarkerActions m_Value;
        public Type type => typeof(MarkerActions);

        public object objectValue
        {
            get => m_Value;
            set
            {
                m_Value = (MarkerActions)value;
            }
        }

        public MarkerActions value
        {
            get => m_Value;
            private set
            {
                m_Value = value;
                m_OnValueChanged?.Invoke(m_Value);
            }
        }
        #endregion
        #region PropertyActions

        UnityEvent<MarkerActions> m_OnValueChanged = new UnityEvent<MarkerActions>();
        Dictionary<Action, UnityAction<MarkerActions>> m_Handlers = new Dictionary<Action, UnityAction<MarkerActions>>();


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

        #endregion

        void Start()
        {
            if (m_SaveButton)
                m_SaveButton.onControlUp.AddListener(HandleSaveButton);
            if (m_ExportButton)
                m_ExportButton.onControlUp.AddListener(HandleExportButton);
            if (m_DeleteButton)
                m_DeleteButton.onControlUp.AddListener(HandleDeleteButton);
        }

        void HandleSaveButton(BaseEventData evt)
        {
            value.Save?.Invoke();
        }

        void HandleExportButton(BaseEventData evt)
        {
            value.Export?.Invoke();
        }

        void HandleDeleteButton(BaseEventData evt)
        {
            value.Delete?.Invoke();
        }
    }
}
