using System;
using System.Collections.Generic;
using Unity.Reflect.Actors;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    public interface IMarkerStorage : IDisposable, ISessionId
    {
        public Action<Delta<Marker>> OnUpdated { get; set; }
        List<Marker> Markers { get; }

        Marker? Get(string id);
        bool Contains(SyncId id);
        bool Create(Marker marker);
        bool Update(Marker marker);
        bool Delete(Marker marker);
        bool Delete(List<SyncId> markers);
    }
}
