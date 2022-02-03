using System;
using System.Collections.Generic;
using Unity.Reflect.Actors;
using Unity.Reflect.Model;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Pipeline;

namespace Unity.Reflect.Markers.Storage
{

    [Serializable]
    public class SyncMarkerStore : BaseSessionId, IMarkerStorage, IDisposable
    {
        public List<Marker> Markers => new List<Marker>(m_Markers.Values);
        MarkerPublisher m_Publisher = null;
        public Action<Delta<Marker>> OnUpdated { get; set; }

        string m_CurrentProject = null;
        string m_CurrentServer = null;

        public void Initialize()
        {
            DataSourceProvider<SyncMarker>.AddListener(HandleDataSourceProvider);

            m_Publisher = new MarkerPublisher();
        }

        public void Dispose()
        {
            DataSourceProvider<SyncMarker>.RemoveListener(HandleDataSourceProvider);
            m_Publisher?.Dispose();
        }

        public void UpdateProject(UnityProject currentProject, UnityUser user)
        {
            if (currentProject.ProjectId == m_CurrentProject && currentProject.Host.ServerId == m_CurrentServer)
                return;

            m_Markers.Clear();
            OnUpdated?.Invoke(new Delta<Marker>());
            if (currentProject == (UnityProject)Project.Empty)
            {
                m_CurrentProject = m_CurrentServer = null;
                m_Publisher.Disconnect();
                return;
            }
            m_CurrentProject = currentProject.ProjectId;
            m_CurrentServer = currentProject.Host.ServerId;
            m_Publisher.UpdateProject(currentProject, user);
        }



        Dictionary<SyncId, Marker> m_Markers = new Dictionary<SyncId, Marker>();
        Dictionary<SyncId, List<StreamKey>> m_StreamKeys = new Dictionary<SyncId, List<StreamKey>>();

        public Marker? Get(string id)
        {
            SyncId key = new SyncId(id);
            return Get(key);
        }

        public bool Contains(SyncId id)
        {
            return m_Markers.ContainsKey(id);
        }

        public Marker? Get(SyncId id)
        {
            if (m_Markers.ContainsKey(id))
                return m_Markers[id];
            return null;
        }

        public bool Create(Marker marker)
        {
            if (IsMarkerStored(marker))
                return false;
            if (m_Publisher == null)
            {
                Debug.LogWarning("[SyncMarkerStore] Publisher not initialized, this is probably because a project isn't open.");
                return false;
            }
            try
            {
                var key = new StreamKey();
                m_Markers.Add(marker.Id, marker);

                Publish();
                m_StreamKeys.Add(marker.Id, new List<StreamKey>(){key});
                OnUpdated?.Invoke(new Delta<Marker>{Added = new List<Marker>{marker}});
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SyncMarkerStore] Create Marker Exception: {ex}");
            }
            return false;
        }

        public bool Update(Marker marker)
        {
            if (!IsMarkerUpdated(marker))
                return false;
            try
            {
                var prev = m_Markers[marker.Id];
                m_Markers[marker.Id] = marker;
                Publish();
                OnUpdated?.Invoke(new Delta<Marker>{Changed = new List<(Marker, Marker)>{(prev, marker)}});
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SyncMarkerStore] Update Marker Exception: {ex}");
            }
            return false;
        }

        public bool Delete(Marker marker)
        {
            if (!IsMarkerStored(marker))
            {
                return false;
            }
            try
            {
                m_Markers.Remove(marker.Id);
                OnUpdated?.Invoke(new Delta<Marker>{Removed = new List<Marker>{marker}});
                PublishDeletion(new List<SyncId>{marker.Id});

                if (m_StreamKeys.ContainsKey(marker.Id))
                {
                    foreach (var item in m_StreamKeys[marker.Id])
                    {
                        DataSourceProvider<SyncMarker>.Remove(item);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SyncMarkerStore] Delete Marker Exception: {ex}");
            }
            return false;
        }

        public bool Delete(List<SyncId> markers)
        {
            List<Marker> removed = new List<Marker>();
            foreach (var marker in markers)
            {
                if (!m_Markers.ContainsKey(marker))
                    continue;
                removed.Add(m_Markers[marker]);
                m_Markers.Remove(marker);
                if (m_StreamKeys.ContainsKey(marker))
                {
                    foreach (var item in m_StreamKeys[marker])
                    {
                        DataSourceProvider<SyncMarker>.Remove(item);
                    }
                }
            }
            OnUpdated?.Invoke(new Delta<Marker>{Removed = removed});
            try
            {
                PublishDeletion(markers);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SyncMarkerStore] Delete Marker Exception: {ex}");
            }
            return false;
        }

        void Publish()
        {
            List<SyncMarker> syncMarkers = new List<SyncMarker>();
            foreach (var markerKVP in m_Markers)
            {
                syncMarkers.Add(Marker.ToSyncMarker(markerKVP.Value));
            }

            _ = m_Publisher.PerformUpdateAsync(syncMarkers);
        }

        void PublishDeletion(List<SyncId> deletedMarkers)
        {
            List<SyncMarker> syncMarkers = new List<SyncMarker>();
            foreach (var markerKVP in m_Markers)
            {
                if (deletedMarkers.Contains(markerKVP.Key))
                    continue;
                syncMarkers.Add(Marker.ToSyncMarker(markerKVP.Value));
            }
            _ = m_Publisher.DeleteAsync(deletedMarkers, syncMarkers);
        }

        void HandleDataSourceProvider(Dictionary<StreamKey, IDataInstance> dataMessages)
        {
            Delta<Marker> results = new Delta<Marker>();
            foreach (var message in dataMessages)
            {
                Debug.Log($"{message.Key} {message.Value.Id}");
                if (message.Value is ProjectMarker markerInstance)
                {
                    bool isNew = !m_Markers.ContainsKey(markerInstance.Id);

                    var newMarker = Marker.FromProjectMarker(markerInstance);
                    if (isNew)
                    {
                        m_StreamKeys.Add(newMarker.Id, new List<StreamKey>());
                        results.Added.Add(newMarker);
                    }
                    else
                        results.Changed.Add((m_Markers[newMarker.Id], newMarker));
                    m_Markers[newMarker.Id] = newMarker;
                    m_StreamKeys[newMarker.Id].Add(message.Key);
                }
            }
            OnUpdated?.Invoke(results);
        }

        public void UpdateCollection(Delta<Marker> collectionUpdate)
        {
            Delta<Marker> results = new Delta<Marker>();
            foreach (var item in collectionUpdate.Added)
            {
                if (m_Markers.ContainsKey(item.Id))
                {
                    if (m_Markers[item.Id].LastUpdatedTime < item.LastUpdatedTime)
                    {
                        results.Changed.Add((m_Markers[item.Id], item));
                        m_Markers[item.Id] = item;
                    }
                }
                else
                {
                    m_Markers.Add(item.Id, item);
                    results.Added.Add(item);
                }
            }

            foreach (var item in collectionUpdate.Changed)
            {
                if (m_Markers.ContainsKey(item.Next.Id))
                {
                    if (m_Markers[item.Next.Id].LastUpdatedTime < item.Next.LastUpdatedTime)
                    {
                        results.Changed.Add((m_Markers[item.Next.Id], item.Next));
                        m_Markers[item.Next.Id] = item.Next;
                    }
                }
                else
                {
                    m_Markers.Add(item.Next.Id, item.Next);
                    results.Added.Add(item.Next);
                }
            }

            foreach (var item in collectionUpdate.Removed)
            {
                if (m_Markers.ContainsKey(item.Id))
                {
                    m_Markers.Remove(item.Id);
                    results.Removed.Add(item);
                }
            }
            OnUpdated?.Invoke(results);
        }

        bool IsMarkerStored(IMarker marker)
        {
            foreach (var item in m_Markers)
            {
                if (item.Value.Id == marker.Id)
                    return true;
            }

            return false;
        }

        bool IsMarkerUpdated(IMarker marker)
        {
            foreach (var item in m_Markers)
                if (item.Value.Id == marker.Id && item.Value.LastUpdatedTime < marker.LastUpdatedTime)
                    return true;
            return false;
        }
    }
}
