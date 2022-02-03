using System;
using TMPro;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UI;

namespace Unity.Reflect.Viewer.UI
{
    [RequireComponent(typeof(DialogWindow))]
    public class MarkerSyncServiceDebugger : MonoBehaviour
    {
        [SerializeField]
        Button m_ToggleTools;
        [SerializeField][Tooltip("Generate a new marker which will sync to the service.")]
        Button m_SyncNewMarkerButton;
        [SerializeField][Tooltip("Add Vector3.one to the current active marker, and save.")]
        Button m_UpdateActiveMarkerButton;
        [SerializeField]
        TextMeshProUGUI m_ActiveMarkerInfoOutput;

        MarkerSyncStoreManager m_SyncStoreManager;
        DialogWindow m_DialogWindow;

        [SerializeField]
        MarkerController m_MarkerController;

        int m_NextMarker = 0;

        void Awake()
        {
#if !DEBUG
            // Disable development tool on production
            if (m_ToggleTools)
                m_ToggleTools.gameObject.SetActive(false);
            gameObject.SetActive(false);
            return;
#endif
            if (m_SyncStoreManager == null)
                m_SyncStoreManager = FindObjectOfType<MarkerSyncStoreManager>();

            // There needs to be a syncstoremanager active and enabled for this script to be useful.
            Debug.Assert(m_SyncStoreManager, "MarkerSyncStoreManager needs to be available to test it's functionality.");
            Debug.Assert(m_SyncStoreManager.enabled, "MarkerSyncStoreManager needs to be enabled to test it's functionality.");
            m_ActiveMarkerInfoOutput.text = m_MarkerController.ActiveMarker.ToString();

            m_MarkerController.OnMarkerUpdated += HandleOnMarkerUpdated;
            m_SyncNewMarkerButton.onClick.AddListener(SyncNewMarker);
            m_UpdateActiveMarkerButton.onClick.AddListener(UpdateActiveMarker);
            m_ToggleTools.onClick.AddListener(TogglePanel);
            m_DialogWindow = GetComponent<DialogWindow>();
            m_DialogWindow.Close();
        }

        void OnDestroy()
        {
            m_MarkerController.OnMarkerUpdated -= HandleOnMarkerUpdated;
            m_SyncNewMarkerButton.onClick.RemoveListener(SyncNewMarker);
            m_UpdateActiveMarkerButton.onClick.RemoveListener(UpdateActiveMarker);
            m_ToggleTools.onClick.RemoveListener(TogglePanel);
        }

        void TogglePanel()
        {
            if (m_DialogWindow.open)
                m_DialogWindow.Close();
            else
                m_DialogWindow.Open();
        }

        void SyncNewMarker()
        {
            Marker newMarker = new Marker($"debug marker - {m_NextMarker}");
            m_MarkerController.CreateMarker(newMarker);
            m_MarkerController.ActiveMarker = newMarker;
            m_NextMarker++;
        }

        void UpdateActiveMarker()
        {
            Marker marker = (Marker)m_MarkerController.ActiveMarker;
            marker.RelativePosition = marker.RelativePosition + Vector3.one;
            m_MarkerController.EditMarker(marker);
        }

        void HandleOnMarkerUpdated(IMarker value)
        {
            m_ActiveMarkerInfoOutput.text = m_MarkerController.ActiveMarker.ToString();
        }
    }
}
