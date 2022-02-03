using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Unity.Reflect.Actors;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    [Serializable]
    public struct LocalMarkerStoreConfig
    {
        public string dataPath;
        public string fileName;
    }

    [Serializable]
    public class LocalMarkerStore : BaseSessionId, IMarkerStorage
    {
        public Action<Delta<Marker>> OnUpdated { get; set; }
        public LocalMarkerStoreConfig config = new LocalMarkerStoreConfig()
        {
            dataPath = "/markers/",
            fileName = "localmarkers.json"
        };

        public string Folder
        {
            get => Application.persistentDataPath + config.dataPath;
        }
        public string Path
        {
            get => Folder + config.fileName;
        }

        private Dictionary<SyncId, Marker> m_Markers = new Dictionary<SyncId, Marker>();

        private const string k_FileTimeFormat = "yyyyMMddTHH:mm:ssZ";
        [Serializable]
        private struct MarkerStore
        {
            public List<MarkerStorageData> markers;

            public MarkerStore(IEnumerable<Marker> data)
            {
                markers = new List<MarkerStorageData>();
                foreach (var item in data)
                {
                    markers.Add(new MarkerStorageData(item));
                }
            }
        }

        [Serializable]
        private struct MarkerStorageData
        {
            public string Id;
            public string CreatedTime;
            public string LastUpdatedTime;
            public string LastUsedTime;
            public string Name;
            public Vector3 RelativePosition;
            public Vector3 RelativeRotationEuler;
            public Vector3 ObjectScale;

            public MarkerStorageData(IMarker data)
            {
                Id = data.Id.Value;
                CreatedTime = data.CreatedTime.ToString(k_FileTimeFormat);
                LastUpdatedTime = data.LastUpdatedTime.ToString(k_FileTimeFormat);
                LastUsedTime = data.LastUsedTime.ToString(k_FileTimeFormat);
                Name = data.Name;
                RelativePosition = data.RelativePosition;
                RelativeRotationEuler = data.RelativeRotationEuler;
                ObjectScale = data.ObjectScale;
            }

            public static implicit operator Marker(MarkerStorageData data)
            {
                var resp = new Marker();
                resp.Id = new SyncId(data.Id);
                resp.CreatedTime = DateTime.ParseExact(data.CreatedTime, k_FileTimeFormat, CultureInfo.InvariantCulture);
                resp.LastUpdatedTime = DateTime.ParseExact(data.LastUpdatedTime, k_FileTimeFormat, CultureInfo.InvariantCulture);
                resp.LastUsedTime = DateTime.ParseExact(data.LastUsedTime, k_FileTimeFormat, CultureInfo.InvariantCulture);
                resp.Name = data.Name;
                resp.RelativePosition = data.RelativePosition;
                resp.RelativeRotationEuler = data.RelativeRotationEuler;
                resp.ObjectScale = data.ObjectScale;
                return resp;
            }
        }

        private long m_LastLoadTime = 0;

        bool LoadFile()
        {
            if (!File.Exists(Path))
            {
                return false;
            }

            string contents = File.ReadAllText(Path);

            MarkerStore markerStoreData = JsonUtility.FromJson<MarkerStore>(contents);

            Debug.Log($"Loaded {markerStoreData.markers == null}?");
            if (markerStoreData.markers == null)
            {
                EraseFile();
                return false;
            }

            m_Markers = new Dictionary<SyncId, Marker>();
            foreach (var item in markerStoreData.markers)
            {
                if (item.Id != null)
                    m_Markers.Add(item.Id, (Marker)item);
            }
            m_LastLoadTime = DateTime.Now.ToFileTimeUtc();
            return true;
        }

        void WriteFile()
        {
            Directory.CreateDirectory(Folder);
            MarkerStore newStore = new MarkerStore(m_Markers.Values);
            string data = JsonUtility.ToJson(newStore);
            File.WriteAllText(Path, data);
            m_LastLoadTime = DateTime.Now.ToFileTimeUtc();
        }

        bool CheckFileForUpdate()
        {
            if (!File.Exists(Path))
            {
                return false;
            }
            DateTime lastWrite = File.GetLastWriteTime(Path);
            return lastWrite.ToFileTimeUtc() > m_LastLoadTime;
        }

        void EraseFile()
        {
            if (File.Exists(Path))
                File.Delete(Path);
            m_LastLoadTime = 0;
        }

        public List<Marker> Markers
        {
            get
            {
                if (CheckFileForUpdate())
                    LoadFile();
                return new List<Marker>(m_Markers.Values);
            }
        }

        public bool Contains(SyncId id)
        {
            if (CheckFileForUpdate())
                LoadFile();
            return m_Markers.ContainsKey(id);
        }

        public Marker? Get(string id)
        {
            if (CheckFileForUpdate())
                LoadFile();
            if (m_Markers.ContainsKey(id))
                return m_Markers[id];
            return null;
        }

        public bool Create(Marker marker)
        {
            if (CheckFileForUpdate())
                LoadFile();
            // If we have the marker, don't create again.
            if (IsMarkerStored(marker))
                return false;
            m_Markers.Add(marker.Id, marker);
            WriteFile();
            return true;
        }

        public bool Update(Marker marker)
        {
            if (CheckFileForUpdate())
                LoadFile();
            // Check if the updated marker is actually an update.
            if (IsMarkerUpdated(marker))
            {
                m_Markers[marker.Id] = marker;
                WriteFile();
                return true;
            }
            return false;
        }

        public bool Delete(Marker marker)
        {
            if (CheckFileForUpdate())
                LoadFile();
            if (m_Markers.ContainsKey(marker.Id))
            {
                m_Markers.Remove(marker.Id);
                WriteFile();
                return true;
            }
            return false;
        }

        public bool Delete(List<SyncId> markerIds)
        {
            if (CheckFileForUpdate())
                LoadFile();
            foreach (var id in markerIds)
            {
                if (m_Markers.ContainsKey(id))
                {
                    m_Markers.Remove(id);
                }
            }
            WriteFile();
            return true;
        }

        public void Dispose()
        {

        }

        bool IsMarkerStored(Marker marker)
        {
            return m_Markers.ContainsKey(marker.Id);
        }

        bool IsMarkerUpdated(Marker marker)
        {
            foreach (var item in m_Markers)
            {
                if (item.Value.Id == marker.Id && item.Value.LastUpdatedTime < marker.LastUpdatedTime)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
