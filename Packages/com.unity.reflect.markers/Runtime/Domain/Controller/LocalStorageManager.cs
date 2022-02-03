using System;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    /// <summary>
    /// Initializes LocalStore with a serialized configuration
    /// </summary>
    public class LocalStorageManager : MonoBehaviour
    {
        [SerializeField]
        MarkerController m_MarkerStore;

        [SerializeField]
        LocalMarkerStoreConfig m_Config;

        void Start()
        {
            if (m_MarkerStore == null)
                Debug.LogError("Missing Marker Store");

            if (m_MarkerStore.MarkerStorage == null)
            {
                var newLocalStore = new LocalMarkerStore();
                newLocalStore.config = m_Config;
                m_MarkerStore.MarkerStorage = newLocalStore;
            } else if (m_MarkerStore.MarkerStorage is MirroredMarkerStore mirrorStore)
            {
                var newLocalStore = new LocalMarkerStore();
                newLocalStore.config = m_Config;
                mirrorStore.AddStore(newLocalStore);
            } else
                Debug.LogError($"MarkerStore not of LocalMarkerStorage");
        }
    }
}
