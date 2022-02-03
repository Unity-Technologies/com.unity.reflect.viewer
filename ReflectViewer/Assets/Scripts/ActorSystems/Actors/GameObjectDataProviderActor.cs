using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Data;
using Unity.Reflect.Model;
using Unity.Reflect.Actors;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using Object = UnityEngine.Object;

namespace Unity.Reflect.Viewer.Actors
{
    [Actor("c60e3090-d2ed-4e14-a0ff-7ce9c5ac5f48", true)]
    public class GameObjectDataProviderActor
    {
#pragma warning disable 649
        Settings m_Settings;

        RpcOutput<AcquireDynamicEntry> m_AcquireDynamicEntryDataOutput;

        RpcOutput<GetManifests> m_GetManifestsFallback;
        RpcOutput<CreateGameObject> m_CreateGameObjectFallback;
#pragma warning restore 649

        Dictionary<StreamKey, GameObject> m_GameObjectLookup;

        GameObject m_Prefab;
        Transform m_DataRoot;

        bool m_EmbeddedProject;

        void Inject(Project project)
        {
            if (project is EmbeddedProject embeddedProject)
            {
                m_EmbeddedProject = true;
                m_Prefab = embeddedProject.prefab;
            }
            else
            {
                m_EmbeddedProject = false;
            }
        }

        void Shutdown()
        {
            if (m_DataRoot != null)
            {
                Object.Destroy(m_DataRoot.gameObject);
            }
        }

        [RpcInput]
        void OnGetManifests(RpcContext<GetManifests> ctx)
        {
            if (!m_EmbeddedProject)
            {
                ForwardCall<GetManifests, List<SyncManifest>>(ctx, m_GetManifestsFallback);
                return;
            }

            var manifests = new List<SyncManifest>();

            m_DataRoot = Object.Instantiate(m_Prefab, m_Settings.Root).transform;
            m_DataRoot.name = m_Prefab.name;

            m_GameObjectLookup = new Dictionary<StreamKey, GameObject>();

            var sources = GetSourcesRoots(m_DataRoot);

            foreach (var source in sources)
            {
                var manifest = new SyncManifest { SourceId = Guid.NewGuid().ToString() };

                foreach (Transform child in source.transform)
                {
                    child.gameObject.SetActive(false);

                    string id = null;

                    if (child.TryGetComponent<SyncObjectBinding>(out var syncObjectBinding))
                    {
                        id = syncObjectBinding.streamKey.key.Name;
                    }

                    if (string.IsNullOrEmpty(id))
                    {
                        id = Guid.NewGuid().ToString();
                    }

                    var bbox = ComputeBoundingBox(child);

                    var persistentKey = new PersistentKey(typeof(SyncObjectInstance), id);

                    manifest.Append(persistentKey, string.Empty, "dummy", bbox);

                    m_GameObjectLookup.Add(new StreamKey(manifest.SourceId, persistentKey), child.gameObject);
                }

                manifests.Add(manifest);
            }

            ctx.SendSuccess(manifests);
        }

        static SyncBoundingBox ComputeBoundingBox(Transform child)
        {
            var childBounds = GetBounds(child).ToArray();

            var bounds = new Bounds();

            for (var i = 0; i < childBounds.Length; ++i)
            {
                var b = childBounds[i];
                if (i == 0)
                {
                    bounds = b;
                }
                else
                {
                    bounds.Encapsulate(b);
                }
            }

            var min = bounds.min;
            var max = bounds.max;

            return new SyncBoundingBox(new System.Numerics.Vector3(min.x, min.y, min.z), new System.Numerics.Vector3(max.x, max.y, max.z));
        }

        static IEnumerable<Bounds> GetBounds(Transform root)
        {
            var renderers = root.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                yield return renderer.bounds;
            }

            var terrains = root.GetComponentsInChildren<Terrain>();

            foreach (var terrain in terrains)
            {
                yield return new Bounds
                {
                    min = root.TransformPoint(terrain.terrainData.bounds.min),
                    max = root.TransformPoint(terrain.terrainData.bounds.max)
                };
            }
        }

        [RpcInput]
        void OnCreateGameObject(RpcContext<CreateGameObject> ctx)
        {
            if (!m_EmbeddedProject)
            {
                ForwardCall<CreateGameObject, GameObject>(ctx, m_CreateGameObjectFallback);
                return;
            }

            var tracker = new Tracker();

            var instanceEntryRpc = m_AcquireDynamicEntryDataOutput.Call(this, ctx, tracker, new AcquireDynamicEntry(ctx.Data.InstanceId));

            instanceEntryRpc.Success<DynamicEntry>((self, ctx, tracker, entryData) =>
            {
                var streamKey = new StreamKey(entryData.Data.SourceId, entryData.Data.IdInSource);
                var original = m_GameObjectLookup[streamKey];

                original.SetActive(true);

                ctx.SendSuccess(original);
            });
        }

        static IEnumerable<Transform> GetSourcesRoots(Transform root)
        {
            // Avoid embedded SyncPrefabBinding by only going through the first level of children.
            var syncPrefabBinding = root.GetComponent<SyncPrefabBinding>();

            if (syncPrefabBinding != null)
            {
                yield return root;
                yield break;
            }

            foreach (Transform child in root)
            {
                yield return child;
            }
        }

        class Tracker { }

        [Serializable]
        public class Settings : ActorSettings
        {
            [SerializeField]
            [Transient(nameof(Root))]
            ExposedReference<Transform> m_Root;

            [HideInInspector]
            public Transform Root;

            public Settings()
                : base(Guid.NewGuid().ToString()) { }
        }

        static void ForwardCall<T, TResult>(RpcContext<T> rpcCtx, RpcOutput<T> rpcOutput)
            where T : class
            where TResult : class
        {
            var rpc = rpcOutput.Call((object) null, rpcCtx, (object) null, rpcCtx.Data);
            rpc.Success<TResult>((self, ctx, userCtx, res) => ctx.SendSuccess(res));
            rpc.Failure((self, ctx, userCtx, ex) => ctx.SendFailure(ex));
        }
    }
}
