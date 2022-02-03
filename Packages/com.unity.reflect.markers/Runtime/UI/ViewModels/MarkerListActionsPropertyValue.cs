using System;
using System.Collections.Generic;
using TMPro;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Unity.Reflect.Markers.UI
{
    public struct MarkerListActions: IEquatable<MarkerListActions>
    {
        public Action Delete;
        public Action Export;
        public int MarkersSelected;
        public bool Active;

        public bool Equals(MarkerListActions other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Delete, other.Delete) && Equals(Export, other.Export) && MarkersSelected == other.MarkersSelected && Active == other.Active;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MarkerListActions)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Delete != null ? Delete.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Export != null ? Export.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ MarkersSelected;
                hashCode = (hashCode * 397) ^ Active.GetHashCode();
                return hashCode;
            }
        }
    }

    [RequireComponent(typeof(LayoutElement))]
    public class MarkerListActionsPropertyValue : MonoBehaviour, IPropertyValue<MarkerListActions>
    {
        [SerializeField]
        ButtonControl m_DeleteButton;

        [SerializeField]
        ButtonControl m_ExportButton;

        [SerializeField]
        TextMeshProUGUI m_SelectionCountLabel;

        [SerializeField]
        GameObject m_Panel;

        [SerializeField]
        float m_ExpandedHeight = 100f;

        #region PropertyValue
        Type m_Type;
        MarkerListActions m_Value;
        LayoutElement m_LayoutElement;

        public Type type => typeof(MarkerListActions);

        public object objectValue
        {
            get => m_Value;
            set
            {
                m_Value = (MarkerListActions)value;
                UpdateState();
            }
        }

        public MarkerListActions value
        {
            get => m_Value;
            private set
            {
                m_Value = value;
                UpdateState();
                m_OnValueChanged?.Invoke(m_Value);
            }
        }

        #endregion
        #region PropertyActions

        UnityEvent<MarkerListActions> m_OnValueChanged = new UnityEvent<MarkerListActions>();
        Dictionary<Action, UnityAction<MarkerListActions>> m_Handlers = new Dictionary<Action, UnityAction<MarkerListActions>>();


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
            m_LayoutElement = GetComponent<LayoutElement>();
            m_DeleteButton.onControlUp.AddListener(HandleDelete);
            m_ExportButton.onControlUp.AddListener(HandleExport);
        }

        void HandleDelete(BaseEventData evt)
        {
            value.Delete?.Invoke();
        }

        void HandleExport(BaseEventData evt)
        {
            value.Export?.Invoke();
        }

        void UpdateState()
        {
            if (!m_Value.Active)
            {
                m_Panel.SetActive(false);
                if (m_LayoutElement)
                    m_LayoutElement.preferredHeight = 0f;
            }
            else
            {
                m_Panel.SetActive(true);
                UpdateLabel();
                if (m_LayoutElement)
                    m_LayoutElement.preferredHeight = 100f;
            }
        }

        void UpdateLabel()
        {
            string text;
            if (m_Value.MarkersSelected == 1)
                text = $"{m_Value.MarkersSelected} Marker selected.";
            else
                text = $"{m_Value.MarkersSelected} Markers selected.";
            m_SelectionCountLabel.text = text;
        }
    }
}
