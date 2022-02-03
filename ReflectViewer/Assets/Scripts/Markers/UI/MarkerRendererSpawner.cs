using System;
using System.Collections.Generic;
using SharpFlux.Dispatching;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class MarkerRendererSpawner : MonoBehaviour, IRenderedMarkerManager
    {
        [SerializeField]
        MarkerRenderer m_MarkerRendererPrefab;
        [SerializeField]
        MarkerController m_MarkerController;
        [SerializeField]
        MarkerUIPresenter m_MarkerUIPresenter;

        [SerializeField, Header("Debug")]
        bool m_Debug = false;
        [SerializeField]
        GameObject m_DebugGameObject;

        Dictionary<SyncId, MarkerRenderer> m_SpawnedMarkers = new Dictionary<SyncId, MarkerRenderer>();
        IUISelector<Transform> m_RootGetter;
        IUISelector<OpenDialogAction.DialogType> m_ActiveDialogSelector;
        bool m_Visible = false;

        public void SelectMarker(IMarker selectedMarker)
        {
            m_MarkerUIPresenter.SelectMarker((Marker)selectedMarker);
        }

        void Awake()
        {
            if (m_Debug)
                m_Debug = Debug.isDebugBuild;
        }

        void Start()
        {
            m_MarkerController.OnMarkerListUpdated += HandleMarkersUpdated;
            m_MarkerController.OnAlignedObjectUpdated += HandleMarkersUpdated;
            m_MarkerController.OnMarkerUpdated += HandleMarkerUpdated;
            m_MarkerController.RenderedMarkerManager = this;
            m_RootGetter = UISelectorFactory.createSelector<Transform>(PipelineContext.current, "rootNode");
            if (m_MarkerUIPresenter == null)
                m_MarkerUIPresenter = FindObjectOfType<MarkerUIPresenter>();

            m_ActiveDialogSelector = UISelectorFactory.createSelector<OpenDialogAction.DialogType>(UIStateContext.current, nameof(IDialogDataProvider.activeDialog), OnActiveDialogChanged);
        }

        void OnActiveDialogChanged(OpenDialogAction.DialogType data)
        {
            var open = data == OpenDialogAction.DialogType.Marker;
            if (open != m_Visible)
            {
                m_Visible = open;
                HandleMarkersUpdated();
            }
        }

        void OnDestroy()
        {
            m_MarkerController.OnMarkerListUpdated -= HandleMarkersUpdated;
            m_MarkerController.OnAlignedObjectUpdated -= HandleMarkersUpdated;
            m_MarkerController.OnMarkerUpdated -= HandleMarkerUpdated;

            ClearSpawnedMarkers();
            m_RootGetter?.Dispose();
            m_ActiveDialogSelector?.Dispose();
        }

        void HandleMarkersUpdated()
        {
            ClearSpawnedMarkers();
            SpawnMarkers();
        }

        void HandleMarkerUpdated(IMarker marker)
        {
            HandleMarkersUpdated();
        }

        void ClearSpawnedMarkers()
        {
            if (m_SpawnedMarkers == null || m_SpawnedMarkers.Count == 0)
                return;
            foreach (var item in m_SpawnedMarkers)
            {
                if (item.Value && item.Value.gameObject)
                    Destroy(item.Value.gameObject);
            }
            m_SpawnedMarkers.Clear();
        }

        void SpawnMarkers()
        {
            if (!m_Visible)
                return;
            foreach (var marker in m_MarkerController.MarkerStorage.Markers)
            {
                
                SpawnMarker(marker);
            }
        }

        void SpawnMarker(IMarker marker)
        {
            MarkerRenderer newMarker = Instantiate(m_MarkerRendererPrefab, m_RootGetter.GetValue().transform);
            if (m_Debug && m_DebugGameObject)
                Instantiate(m_DebugGameObject, newMarker.transform);
            newMarker.Setup(this, marker, m_RootGetter.GetValue().transform);
            m_SpawnedMarkers.Add(marker.Id, newMarker);
        }

        /// <summary>
        /// Update a single rendered marker
        /// </summary>
        /// <param name="marker">Marker to update</param>
        public void Visualize(IMarker marker)
        {
            if (!m_SpawnedMarkers.ContainsKey(marker.Id))
            {
                SpawnMarker(marker);
                return;
            }

            var item = m_SpawnedMarkers[marker.Id];
            item.Setup(this, marker, m_RootGetter.GetValue().transform);
        }
    }
}
