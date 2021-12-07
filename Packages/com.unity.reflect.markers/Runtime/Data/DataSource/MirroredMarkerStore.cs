using System;
using System.Collections.Generic;
using Unity.Reflect.Actors;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{

    [Serializable]
    public class MirroredMarkerStore : IMarkerStorage, IDisposable
    {
        public Action<Delta<Marker>> OnUpdated { get; set; }

        public int SessionId
        {
            get
            {
                if (m_SessionId == 0)
                    m_SessionId = Guid.NewGuid().GetHashCode();
                return m_SessionId;
            }
        }
        int m_SessionId = 0;

        public List<Marker> Markers => new List<Marker>(m_Markers.Values);

        Dictionary<SyncId, Marker> m_Markers = new Dictionary<SyncId, Marker>();

        List<IMarkerStorage> m_Mirrors = new List<IMarkerStorage>();

        /// <summary>
        /// Add an IMarkerStorage to the mirror, all contents of other stores will be shared with each other.
        /// </summary>
        /// <param name="mirror">IMarkerStorage object to add</param>
        public void AddStore(IMarkerStorage mirror)
        {
            m_Mirrors.Add(mirror);
            mirror.OnUpdated += HandleMirrorUpdates;
            Sync();
        }

        /// <summary>
        /// Remove an IMarkerStorage from the mirror
        /// </summary>
        /// <param name="mirror">IMarkerStorage object to remove</param>
        public void RemoveStore(IMarkerStorage mirror)
        {
            m_Mirrors.Remove(mirror);
            mirror.OnUpdated -= HandleMirrorUpdates;
        }

        /// <summary>
        /// Returns the first T or null if none found.
        /// </summary>
        /// <typeparam name="T">IMarkerStorage class type</typeparam>
        /// <returns>First storage of the passed type</returns>
        public T GetStore<T>() where T : class, IMarkerStorage
        {
            for (int i = 0; i < m_Mirrors.Count; i++)
            {
                if(m_Mirrors[i].GetType() == typeof(T))
                    return (T)m_Mirrors[i];
            }
            return null;
        }

        /// <summary>
        /// Returns a list of all stores of type T
        /// </summary>
        /// <typeparam name="T">IMarkerStorage type</typeparam>
        /// <returns>List of objects of your passed type</returns>
        public List<T> GetStores<T>() where T : IMarkerStorage
        {
            List<T> response = new List<T>();
            for (int i = 0; i < m_Mirrors.Count; i++)
            {
                if(m_Mirrors[i].GetType() == typeof(T))
                    response.Add((T)m_Mirrors[i]);
            }
            return response;
        }

        public bool Contains(SyncId id)
        {
            return m_Markers.ContainsKey(id);
        }

        public Marker? Get(string id)
        {
            if (m_Markers.ContainsKey(id))
                return m_Markers[id];
            return null;
        }

        public bool Create(Marker marker)
        {
            m_Markers.Add(marker.Id, marker);
            foreach (var mirror in m_Mirrors)
            {
                mirror.Create(marker);
            }
            return true;
        }

        public bool Update(Marker marker)
        {
            m_Markers[marker.Id] = marker;
            foreach (var mirror in m_Mirrors)
            {
                mirror.Update(marker);
            }
            return true;
        }

        public bool Delete(Marker marker)
        {
            m_Markers.Remove(marker.Id);
            foreach (var mirror in m_Mirrors)
            {
                mirror.Delete(marker);
            }
            return true;
        }

        public bool Delete(List<SyncId> markerIds)
        {
            foreach (var id in markerIds)
            {
                m_Markers.Remove(id);
            }
            foreach (var mirror in m_Mirrors)
            {
                mirror.Delete(markerIds);
            }
            return true;
        }

        public void Initialize()
        {

        }

        /// <summary>
        ///
        /// </summary>
        public void Sync()
        {
            // Pull all markers from mirrors, compare locally, then add or replace in cache if missing or updated.
            foreach (var mirror in m_Mirrors)
            {
                foreach (var marker in mirror.Markers)
                {
                    if (!m_Markers.ContainsKey(marker.Id) || m_Markers[marker.Id].LastUpdatedTime < marker.LastUpdatedTime)
                        m_Markers[marker.Id] = marker;
                }
            }

            // Updated mirrors with markers in cache
            foreach (var marker in m_Markers)
            {
                foreach (var mirror in m_Mirrors)
                {
                    var mirrorMarker = mirror.Get(marker.Key.Value);
                    if (mirrorMarker == null)
                        mirror.Create(marker.Value);
                    else if (mirrorMarker.Value.LastUpdatedTime < marker.Value.LastUpdatedTime)
                        mirror.Update(marker.Value);
                }
            }
        }

        public void HandleMirrorUpdates(Delta<Marker> delta)
        {
            foreach (var added in delta.Added)
            {
                if (!m_Markers.ContainsKey(added.Id))
                    Create(added);
            }
            foreach (var changed in delta.Changed)
            {
                if (!m_Markers.ContainsKey(changed.Next.Id))
                    Create(changed.Next);
                else if (m_Markers[changed.Next.Id].LastUpdatedTime < changed.Next.LastUpdatedTime)
                    Update(changed.Next);
            }
            OnUpdated?.Invoke(delta);
        }

        public void Dispose()
        {
            foreach (var mirror in m_Mirrors)
            {
                mirror.Dispose();
            }
            m_Mirrors.Clear();
        }

    }
}
