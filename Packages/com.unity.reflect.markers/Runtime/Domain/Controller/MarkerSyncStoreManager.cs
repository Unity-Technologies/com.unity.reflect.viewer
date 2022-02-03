using System;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Source.Utils.Errors;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class MarkerSyncStoreManager : MonoBehaviour
    {
        [SerializeField]
        MarkerController m_MarkerController;
        SyncMarkerStore m_SyncMarkerStore;

        public SyncMarkerStore SyncMarkerStore => m_SyncMarkerStore;

        void Start()
        {
            if (m_MarkerController == null)
                Debug.LogError("Missing Marker Controller");

            if (m_MarkerController.MarkerStorage == null)
            {

                m_SyncMarkerStore = new SyncMarkerStore();
                m_SyncMarkerStore.Initialize();
                m_MarkerController.MarkerStorage = m_SyncMarkerStore;
            } else if (m_MarkerController.MarkerStorage is MirroredMarkerStore mirrorStore)
            {
                m_SyncMarkerStore = new SyncMarkerStore();
                m_SyncMarkerStore.Initialize();
                mirrorStore.AddStore(m_SyncMarkerStore);
            } else
                Debug.LogError("Storage is missing or not SyncMarkerStore");
        }

        public void UpdateProject(UnityProject currentProject, UnityUser user)
        {
            m_MarkerController.LoadingComplete = false;
            try
            {
                m_SyncMarkerStore?.UpdateProject(currentProject, user);
            }
            catch (SyncModelNotSupportedException ex)
            {
                m_MarkerController.UnsupportedMessage = "QR Markers unsupported by Host.";
                Debug.LogError($"SyncMarkers not supported by the server: {ex}");
            }
            catch (Exception ex)
            {
                m_MarkerController.UnsupportedMessage = $"QR Markers failed to connect to Host: {ex.Message}";
                Debug.LogError($"Exception thrown creating publisher client: {ex}");
            }

        }

        void OnDestroy()
        {
            m_SyncMarkerStore?.Dispose();
        }
    }
}
