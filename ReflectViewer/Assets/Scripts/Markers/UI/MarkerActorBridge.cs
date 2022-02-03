using System;
using System.Collections.Generic;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using Unity.Reflect.Markers.Domain.Controller;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer
{
    public class MarkerActorBridge : MonoBehaviour
    {
        BridgeActor.Proxy m_Bridge;
        [SerializeField]
        ViewerReflectBootstrapper m_Reflect;
        [SerializeField]
        MarkerSyncStoreManager m_SyncStoreManager;

        void Start()
        {
            m_Reflect.StreamingStarted += HandleStreamingStarted;
        }

        void OnDestroy()
        {
            m_Reflect.StreamingStarted -= HandleStreamingStarted;
        }

        void HandleStreamingStarted(BridgeActor.Proxy bridge)
        {
            m_Bridge = bridge;
            m_Bridge.Subscribe<Boxed<Delta<SyncMarker>>>(SetMarkerCollection);
        }

        void SetMarkerCollection(EventContext< Boxed< Delta<SyncMarker>>> ctx)
        {
            Delta<Marker> resp = new Delta<Marker>();
            resp.Added = CastList(ctx.Data.Value.Added);
            resp.Removed = CastList(ctx.Data.Value.Removed);
            resp.Changed = CastList(ctx.Data.Value.Changed);
            m_SyncStoreManager.SyncMarkerStore.UpdateCollection(resp);
        }

        List<Marker> CastList(List<SyncMarker> list)
        {
            List<Marker> resp = new List<Marker>();
            foreach (var item in list)
            {
                resp.Add(CastSyncMarker(item));
            }
            return resp;
        }

        List<(Marker, Marker)> CastList(List<(SyncMarker, SyncMarker)> list)
        {
            List<(Marker, Marker)> resp = new List<(Marker, Marker)>();
            foreach (var item in list)
            {
                resp.Add((
                    CastSyncMarker(item.Item1),
                    CastSyncMarker(item.Item2)
                    ));
            }
            return resp;
        }

        Marker CastSyncMarker(SyncMarker syncMarker)
        {
            return Marker.FromProjectMarker(ProjectMarker.FromSyncModel(syncMarker));
        }
    }
}
