using System.Collections.Generic;
using System.Linq;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.Actors
{
    [Actor("0995663d-e5d2-4338-9b19-ab70dacebf01")]
    public class MarkerActor
    {
        RpcOutput<AcquireResource> m_AcquireResourceOutput;
        public EventOutput<Boxed<Delta<SyncMarker>>> m_OnMarkersLoaded;

        [NetInput]
        public void OnEntryDataChanged(NetContext<EntryDataChanged> ctx)
        {
            GatherMarkers(new Tracker(Filter(ctx.Data.Delta)));
        }

        void GatherMarkers(Tracker tracker)
        {
            Delta<SyncMarker> result = new Delta<SyncMarker>();
            foreach (var entryData in tracker.entriesToProcess)
            {
                var addAcquireRpc = m_AcquireResourceOutput.Call(this, tracker, entryData,  new AcquireResource(new StreamState(), entryData));
                addAcquireRpc.Success<SyncMarker>((self, entryDataList, data, syncObject) =>
                {
                    tracker.entriesToProcess.Remove(data);
                    switch (tracker.modificationTypes[data.Id])
                    {
                        case Tracker.Modification.Added:
                            result.Added.Add(syncObject);
                            break;
                        case Tracker.Modification.Changed:
                            result.Changed.Add((default, syncObject));
                            break;
                        case Tracker.Modification.Removed:
                            result.Removed.Add(syncObject);
                            break;
                    }
                    if (tracker.entriesToProcess.Count == 0)
                    {
                        self.m_OnMarkersLoaded.Broadcast(new Boxed<Delta<SyncMarker>>(result));
                    }
                });
                addAcquireRpc.Failure((self, ctx, userCtx, ex) =>
                {
                    Debug.LogException(ex);
                });
            }
        }

        Delta<EntryData> Filter(Delta<EntryData> unfiltered)
        {
            Delta<EntryData> result = new Delta<EntryData>();
            foreach (var item in unfiltered.Added.Where(e => e.EntryType == typeof(SyncMarker)))
                result.Added.Add(item);
            foreach (var item in unfiltered.Changed.Where(e => e.Next.EntryType == typeof(SyncMarker)))
                result.Changed.Add(item);
            foreach (var item in unfiltered.Removed.Where(e => e.EntryType == typeof(SyncMarker)))
                result.Removed.Add(item);
            return result;
        }

        public class Tracker
        {
            public HashSet<EntryData> entriesToProcess;
            public Dictionary<EntryGuid, Modification> modificationTypes;
            public enum Modification
            {
                Added,
                Changed,
                Removed
            }
            public Tracker(Delta<EntryData> entries)
            {
                entriesToProcess = new HashSet<EntryData>();
                modificationTypes = new Dictionary<EntryGuid, Modification>();
                foreach (var item in entries.Added.Where(e => e.EntryType == typeof(SyncMarker)))
                {
                    modificationTypes.Add(item.Id, Modification.Added);
                    entriesToProcess.Add(item);
                }

                foreach (var item in entries.Changed.Where(e => e.Next.EntryType == typeof(SyncMarker)))
                {
                    modificationTypes.Add(item.Next.Id, Modification.Changed);
                    entriesToProcess.Add(item.Next);
                }

                foreach (var item in entries.Removed.Where(e => e.EntryType == typeof(SyncMarker)))
                {
                    modificationTypes.Add(item.Id, Modification.Removed);
                    entriesToProcess.Add(item);
                }
            }
        }
    }
}
