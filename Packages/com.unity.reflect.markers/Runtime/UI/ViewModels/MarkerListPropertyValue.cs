using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.Events;

namespace Unity.Reflect.Markers.UI
{
    public class MarkerListPropertyValue : MonoBehaviour, IPropertyValue<MarkerListProperty>
    {
        [SerializeField]
        Transform m_Container;

        [SerializeField]
        MarkerListButtonControl m_OptionPrefab;

        MarkerListProperty m_Value;
        Dictionary<SyncId, MarkerListButtonControl> m_SpawnedButtons = new Dictionary<SyncId, MarkerListButtonControl>();
        public Type type => typeof(MarkerListProperty);

        public object objectValue
        {
            get => value;
            set
            {
                this.value = (MarkerListProperty)value;
                UpdateList();
            }
        }
        UnityEvent<MarkerListProperty> m_OnValueChanged = new UnityEvent<MarkerListProperty>();
        Dictionary<Action, UnityAction<MarkerListProperty>> m_Handlers = new Dictionary<Action, UnityAction<MarkerListProperty>>();

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

        public MarkerListProperty value
        {
            get => m_Value;
            private set
            {
                if (m_Value.Equals(value))
                    return;
                m_Value = value;
                UpdateList();
                m_OnValueChanged?.Invoke(m_Value);
            }
        }

        void Start()
        {
            // Populate the UI
            UpdateList();
        }

        /// <summary>
        /// Disconnect listeners
        /// </summary>
        void OnDestroy()
        {
            ClearList();
        }


        /// <summary>
        /// Spawn buttons for UI
        /// </summary>
        void UpdateList()
        {
            ClearStaleButtons();
            if (value.Markers == null)
                return;
            foreach (var marker in value.Markers)
            {
                UpdateButton(marker);
            }
        }

        void SpawnButton(Marker marker)
        {
            MarkerListButtonControl buttonControl = Instantiate(m_OptionPrefab, m_Container);
            buttonControl.Configure(HandleMarkerClick, marker.Id);
            m_SpawnedButtons.Add(marker.Id, buttonControl);
        }

        void UpdateButton(Marker marker)
        {
            if (!m_SpawnedButtons.ContainsKey(marker.Id))
            {
                SpawnButton(marker);
            }

            var buttonControl = m_SpawnedButtons[marker.Id];
            buttonControl.SetLabel(marker.Name);
            // @@TODO Set thumbnail
            buttonControl.SetThumbnail(null);
            buttonControl.Select(value.Selected.Contains(marker.Id));
            buttonControl.ActiveMarker(marker.Id.Equals(value.Active));
        }

        void ClearStaleButtons()
        {
            List<SyncId> staleIds = new List<SyncId>();
            List<SyncId> freshIds = new List<SyncId>();

            foreach (var item in value.Markers)
            {
                freshIds.Add(item.Id);
            }

            foreach (var item in m_SpawnedButtons)
            {
                if (!freshIds.Contains(item.Key))
                {
                    staleIds.Add(item.Key);
                    item.Value.Dispose();
                    Destroy(item.Value.gameObject);
                }
            }

            foreach (var item in staleIds)
            {
                m_SpawnedButtons.Remove(item);
            }
        }

        void HandleMarkerClick(SyncId markerId)
        {
            switch (value.Mode)
            {
                case MarkerListProperty.SelectionMode.Single:
                    SoloSelect(markerId);
                    break;
                case MarkerListProperty.SelectionMode.Multiple:
                    AddToSelection(markerId);
                    break;
            }
        }

        void AddToSelection(SyncId markerId)
        {
            if (value.Selected.Contains(markerId))
            {
                // Unselect
                value.Selected.Remove(markerId);
                m_SpawnedButtons[markerId].Select(false);
            }
            else
            {
                // select
                value.Selected.Add(markerId);
                m_SpawnedButtons[markerId].Select(true);
            }

            UpdateList();
            value.OnSelectionUpdated?.Invoke();
            m_OnValueChanged?.Invoke(value);
        }

        void SoloSelect(SyncId markerId)
        {
            if (value.Selected.Contains(markerId))
            {
                // Unselect
                value.Selected.Remove(markerId);
                m_SpawnedButtons[markerId].Select(false);
            }
            else
            {
                // select
                if (value.Selected.Count > 0)
                    ClearSelection();
                value.Selected.Add(markerId);
                m_SpawnedButtons[markerId].Select(true);
            }

            UpdateList();
            value.OnSelectionUpdated?.Invoke();
            m_OnValueChanged?.Invoke(value);
        }

        void ClearSelection()
        {
            foreach (var item in value.Selected)
            {
                if (m_SpawnedButtons.ContainsKey(item))
                {
                    m_SpawnedButtons[item].Select(false);
                }
            }
            value.Selected.Clear();
        }

        /// <summary>
        /// Disconnect listeners, and remove spawned objects.
        /// </summary>
        void ClearList()
        {
            foreach (var button in m_SpawnedButtons)
            {
                button.Value.Dispose();
                Destroy(button.Value.gameObject);
            }
            m_SpawnedButtons.Clear();
        }
    }
}
