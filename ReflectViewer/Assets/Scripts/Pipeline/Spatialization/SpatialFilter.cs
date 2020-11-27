using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Unity.Reflect;
using Unity.Reflect.Data;
using Unity.Reflect.Model;
using UnityEngine.Reflect.Pipeline;

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    [Serializable]
    public class SpatialFilterNode : ReflectNode<SpatialFilter>
    {
        public StreamAssetInput assetInput = new StreamAssetInput();
        public StreamAssetOutput assetOutput = new StreamAssetOutput();

        public GameObjectInput gameObjectInput = new GameObjectInput();

        public SpatialFilterSettings settings;

        public ISpatialPicker<ISpatialObject> SpatialPicker => processor;

        protected override SpatialFilter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var root = settings.boundingBoxRoot.Resolve(resolver);
            var p = new SpatialFilter(hook.helpers.clock, hook.helpers.memoryStats, hook.services.eventHub, hook.systems.memoryCleaner.memoryLevel, root, settings, assetOutput);

            assetInput.streamBegin = assetOutput.SendBegin;
            assetInput.streamEvent = p.OnStreamAssetEvent;
            assetInput.streamEnd = p.OnStreamInstanceEnd;

            gameObjectInput.streamEvent = p.OnStreamGameObjectEvent;

            return p;
        }
    }

    public class MaxNbDisplayableObjectsChanged
    {
        public int MaxNbObjects;
    }

    public sealed class SpatialFilter : ReflectTaskNodeProcessor,
        IOnDrawGizmosSelected,
        ISpatialPicker<ISpatialObject>
    {
        class SpatialObject : ISpatialObject
        {
            public static Action<SpatialObject> Load;
            public static Action<SpatialObject> Unload;

            public Vector3 min { get; }
            public Vector3 max { get; }
            public Vector3 center { get; }
            public float priority { get; set; }
            public bool isVisible { get; set; }
            public GameObject loadedObject { get; set; }

            // the following fields break the Unity code conventions, which suggest using properties instead
            // however, in the worst cases these fields are accessed multiple times per object per frame
            // which causes a massive performance hit when deep profiling in the Unity Editor
            // this is worth the trade-off since this class is private
            public bool IsLoaded;
            public MeshRenderer[] MeshRenderers;
            public bool HasMeshRendererChanged;
            public readonly SyncedData<StreamAsset> StreamAsset;

            bool m_ReceivedVisibilityUpdate, m_WasVisible;

            public bool HasMeshRenderers => MeshRenderers != null && MeshRenderers.Length > 0;

            public SpatialObject(SyncedData<StreamAsset> streamAsset)
            {
                StreamAsset = streamAsset;

                var syncBb = streamAsset.data.boundingBox;
                min = new Vector3(syncBb.Min.X, syncBb.Min.Y, syncBb.Min.Z);
                max = new Vector3(syncBb.Max.X, syncBb.Max.Y, syncBb.Max.Z);
                center = min + (max - min) / 2f;
            }

            public void Dispose()
            {
                // nothing to do
            }

            public bool TryRefreshVisibility()
            {
                if (isVisible && !IsLoaded)
                {
                    Load?.Invoke(this);
                }
                else if (!isVisible && IsLoaded)
                {
                    Unload?.Invoke(this);
                }

                if (isVisible == m_WasVisible)
                    return false;

                m_WasVisible = isVisible;

                return true;
            }

            public bool TryRefreshRenderers(bool forceHidden = false)
            {
                if (!HasMeshRenderers)
                    return false;

                var visible = isVisible && !forceHidden;
                foreach (var meshRenderer in MeshRenderers)
                {
                    if (meshRenderer.enabled != visible)
                        meshRenderer.enabled = visible;
                }

                return true;
            }
        }

        struct BoundingBoxReference
        {
            public Transform transform;
            public MeshRenderer meshRenderer;
        }

        static readonly int k_MinNbObjects = 100;

        readonly Clock.Proxy m_Clock;
        readonly MemoryStats.Proxy m_Stats;
        readonly EventHub m_Hub;
        EventHub.Group m_Group;

        MemoryLevel m_MemoryLevel;
        bool m_IsMemoryLevelFresh;
        TimeSpan m_LastSpeculationUpdate;
        int m_NbLoadedGameObjects;
        volatile int m_CurrentMaxVisibleObjects;

        readonly StreamAssetOutput m_StreamOutput;
        public SpatialFilterSettings settings { get; }

        readonly List<ISpatialObject> m_PrevVisibilityResults = new List<ISpatialObject>();
        readonly List<ISpatialObject> m_VisibilityResults = new List<ISpatialObject>();

        Transform m_Root;
        Bounds m_Bounds = new Bounds();
        Camera m_Camera;
        Transform m_CameraTransform;
        float m_VisibilitySqrDist;
        Vector3 m_CamPos, m_CamForward, m_BoundsSize, m_ObjDirection;

        readonly Dictionary<StreamKey, SpatialObject> m_ObjectsByStreamKey = new Dictionary<StreamKey, SpatialObject>();
        readonly PriorityHeap<SpatialObject> m_ObjectsToLoad;

        Transform m_BoundingBoxParent;
        BoundingBoxReference[] m_BoundingBoxes;
        int m_BoundingBoxIndex, m_PrevBoundingBoxIndex;

        readonly Func<ISpatialObject, bool> m_VisibilityPredicate;
        readonly Func<ISpatialObject, float> m_VisibilityPrioritizer;

        bool m_ProjectLifecycleStarted;
        bool m_DisplayOnlyBoundingBoxes;
        volatile bool m_IsPendingVisibilityUpdate;

        Ray m_SelectionRay;
        Ray[] m_SelectionRaySamplePoints;

        public ISpatialCollection<ISpatialObject> spatialCollection { get; }

        bool useDynamicNbVisibleObjects =>
        #if UNITY_IOS || UNITY_ANDROID
            settings.useDynamicNbVisibleObjectsMobile;
        #else
            settings.useDynamicNbVisibleObjectsDesktop;
        #endif

        public SpatialFilter(Clock.Proxy clock, MemoryStats.Proxy stats, EventHub hub, MemoryLevel memoryLevel, Transform root, SpatialFilterSettings settings,
            StreamAssetOutput streamOutput,
            ISpatialCollection<ISpatialObject> spatialCollection = null,
            Func<ISpatialObject, bool> visibilityPredicate = null,
            Func<ISpatialObject, float> visibilityPrioritizer = null)
        {
            m_Clock = clock;
            m_Stats = stats;
            m_Hub = hub;

            m_MemoryLevel = memoryLevel;

            m_Root = root;
            this.settings = settings;
            m_StreamOutput = streamOutput;
            this.spatialCollection = spatialCollection ?? CreateDefaultSpatialCollection();

            m_VisibilityPredicate = visibilityPredicate ?? DefaultVisibilityPredicate;
            m_VisibilityPrioritizer = visibilityPrioritizer ?? DefaultVisibilityPrioritizer;

            m_CurrentMaxVisibleObjects = settings.visibleObjectsMax;

            m_Group = m_Hub.CreateGroup();
            m_Hub.Subscribe<MemoryLevelChanged>(m_Group, OnMemoryEvent);

            InitCamera();

            if (settings.displayUnloadedObjectBoundingBoxes)
                InitBoundingBoxSceneObjects();

            m_ObjectsToLoad = new PriorityHeap<SpatialObject>(settings.visibleObjectsMax, Comparer<SpatialObject>.Create((a, b) => a.priority.CompareTo(b.priority)));

            if (useDynamicNbVisibleObjects)
                m_CurrentMaxVisibleObjects = Math.Min(k_MinNbObjects, settings.visibleObjectsMax);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (m_BoundingBoxParent != null && m_BoundingBoxParent.gameObject != null)
            {
                // only destroy the bounding box root if we created it ourselves
                if (m_Root != null)
                {
                    for (var i = m_BoundingBoxParent.childCount - 1; i >= 0; --i)
                        Object.Destroy(m_BoundingBoxParent.GetChild(i).gameObject);
                }
                else
                    Object.Destroy(m_BoundingBoxParent.gameObject);
            }

            if (m_CameraTransform != null && m_CameraTransform.gameObject)
                Object.Destroy(m_CameraTransform.gameObject);

            m_Camera = null;
            m_CameraTransform = null;
            m_BoundingBoxes = null;
            m_BoundingBoxParent = null;

            m_Hub.DestroyGroup(m_Group);
        }

        public override void OnPipelineInitialized()
        {
            if (m_ProjectLifecycleStarted)
                return;

            m_ProjectLifecycleStarted = true;

            SpatialObject.Load += OnLoad;
            SpatialObject.Unload += OnUnload;

            CacheCameraData();
            Run();

            m_LastSpeculationUpdate = m_Clock.deltaTime;
            UpdateCurrentMaxVisibleObjects(m_CurrentMaxVisibleObjects);
        }

        public override void OnPipelineShutdown()
        {
            if (!m_ProjectLifecycleStarted)
                return;

            m_ProjectLifecycleStarted = false;

            if (m_BoundingBoxes != null)
            {
                for (var i = 0; i < m_BoundingBoxes.Length; ++i)
                {
                    if (m_BoundingBoxes[i].meshRenderer != null)
                        m_BoundingBoxes[i].meshRenderer.enabled = false;
                }
            }

            SpatialObject.Load -= OnLoad;
            SpatialObject.Unload -= OnUnload;

            spatialCollection.Dispose();

            base.OnPipelineShutdown();
        }

        protected override Task RunInternal(CancellationToken token)
        {
            VisibilityTask(token);
            return Task.CompletedTask;
        }

        protected override void UpdateInternal(float unscaledDeltaTime)
        {
            if (!settings.isActive)
                return;

            CacheCameraData();

            if (spatialCollection.objectCount <= 0)
                return;

            m_LastSpeculationUpdate += m_Clock.deltaTime;

            if (m_IsMemoryLevelFresh)
            {
                UpdateSpeculation();
                m_IsMemoryLevelFresh = false;
            }
            else if (m_LastSpeculationUpdate > TimeSpan.FromSeconds(3))
            {
                m_LastSpeculationUpdate = TimeSpan.Zero;
                UpdateSpeculation();
            }

            if (!m_IsPendingVisibilityUpdate)
                return;

            m_IsPendingVisibilityUpdate = false;

            m_BoundingBoxIndex = 0;
            foreach (var obj in m_ObjectsByStreamKey.Values)
            {
                // early exit if neither the visibility nor the renderers have changed
                // and the object doesn't require displaying its bounding box (not visible or renderers are loaded)
                // make sure not to exit if we're displaying only bounding boxes
                // or if the bounding box display toggle has changed, to correctly refresh MeshRenderers
                if (!obj.TryRefreshVisibility()
                    && !obj.HasMeshRendererChanged
                    && (!obj.isVisible || obj.HasMeshRenderers)
                    && !m_DisplayOnlyBoundingBoxes
                    && m_DisplayOnlyBoundingBoxes == settings.displayOnlyBoundingBoxes)
                    continue;

                obj.HasMeshRendererChanged = false;

                // if the object is visible but there is no renderer loaded, display the bounding box instead
                // due to async visibility changes there might be more visible objects than bounding boxes
                // so check to make sure we haven't reached the limit yet
                if ((obj.TryRefreshRenderers(settings.displayOnlyBoundingBoxes) && !settings.displayOnlyBoundingBoxes)
                    || !obj.isVisible
                    || !settings.displayUnloadedObjectBoundingBoxes
                    || m_BoundingBoxIndex >= m_BoundingBoxes.Length)
                    continue;

                var box = m_BoundingBoxes[m_BoundingBoxIndex];
                box.transform.localPosition = obj.center;
                box.transform.localScale = obj.max - obj.min;
                var meshRenderer = box.meshRenderer;
                if (!meshRenderer.enabled)
                    meshRenderer.enabled = true;
                ++m_BoundingBoxIndex;
            }
            m_DisplayOnlyBoundingBoxes = settings.displayOnlyBoundingBoxes;

            if (m_ObjectsToLoad.count > 0)
            {
                const int count = 5; // TODO improve how we determine the count
                for (var i = 0; i < count; ++i)
                {
                    if (!m_ObjectsToLoad.TryPop(out var obj))
                        break;

                    obj.IsLoaded = true;
                    m_StreamOutput.SendStreamAdded(obj.StreamAsset);
                }

                // clear any remaining objects, they will re-add themselves during the next visibility update
                m_ObjectsToLoad.Clear();
            }

            if (!settings.displayUnloadedObjectBoundingBoxes)
                return;

            // hide unused visible boxes
            for (var i = m_BoundingBoxIndex; i < m_PrevBoundingBoxIndex; ++i)
            {
                var meshRenderer = m_BoundingBoxes[i].meshRenderer;
                if (meshRenderer.enabled)
                    meshRenderer.enabled = false;
            }

            m_PrevBoundingBoxIndex = m_BoundingBoxIndex;
        }

        public void OnStreamAssetEvent(SyncedData<StreamAsset> streamAsset, StreamEvent eventType)
        {
            if (!settings.isActive ||
                !PersistentKey.IsKeyFor<SyncObjectInstance>(streamAsset.key.key) ||
                streamAsset.data.boundingBox.Max.Equals(streamAsset.data.boundingBox.Min))
            {
                m_StreamOutput.SendStreamEvent(streamAsset, eventType);
                return;
            }

            SpatialObject obj;
            if (eventType == StreamEvent.Added)
                obj = new SpatialObject(streamAsset);
            else if (!m_ObjectsByStreamKey.TryGetValue(streamAsset.key, out obj))
                return;

            switch (eventType)
            {
                case StreamEvent.Added:
                    m_ObjectsByStreamKey.Add(streamAsset.key, obj);
                    Add(obj);
                    break;
                case StreamEvent.Changed:
                    Remove(obj);
                    Add(obj);
                    m_StreamOutput.SendStreamChanged(streamAsset);
                    break;
                case StreamEvent.Removed:
                    m_ObjectsByStreamKey.Remove(streamAsset.key);
                    Remove(obj);
                    obj.Dispose();
                    m_StreamOutput.SendStreamRemoved(streamAsset);
                    break;
            }
        }

        public void OnStreamInstanceEnd()
        {
            var bounds = spatialCollection.bounds;
            Debug.Log($"[Spatial Filter] depth: {spatialCollection.depth}, " +
                      $"# objects: {spatialCollection.objectCount}, " +
                      $"bounds: [{bounds.min}, {bounds.max}]");

            if (spatialCollection.objectCount != 0)
            {
                settings.onGlobalBoundsCalculated?.Invoke(bounds);
            }

            m_StreamOutput.SendEnd();
        }

        public void OnStreamGameObjectEvent(SyncedData<GameObject> gameObject, StreamEvent eventType)
        {
            if (!settings.isActive)
                return;

            if (!m_ObjectsByStreamKey.TryGetValue(gameObject.key, out var obj))
                return;

            switch (eventType)
            {
                case StreamEvent.Added:
                    ++m_NbLoadedGameObjects;
                    if (m_NbLoadedGameObjects >= m_CurrentMaxVisibleObjects * 0.98f)
                        UpdateSpeculation();
                    UpdateObject(obj, gameObject);
                    break;
                case StreamEvent.Changed:
                    UpdateObject(obj, gameObject);
                    break;
                case StreamEvent.Removed:
                    --m_NbLoadedGameObjects;
                    obj.loadedObject = null;
                    obj.MeshRenderers = null;
                    break;
            }
            obj.HasMeshRendererChanged = true;
        }

        static void UpdateObject(SpatialObject obj, SyncedData<GameObject> gameObject)
        {
            obj.loadedObject = gameObject.data;
            obj.MeshRenderers = gameObject.data.GetComponentsInChildren<MeshRenderer>();
            // disable mesh renderers to avoid popping when displaying only bounding boxes
            foreach (var meshRenderer in obj.MeshRenderers)
                meshRenderer.enabled = false;
        }

        void UpdateSpeculation()
        {
            if (!useDynamicNbVisibleObjects)
            {
                if (m_CurrentMaxVisibleObjects != settings.visibleObjectsMax)
                    UpdateCurrentMaxVisibleObjects(settings.visibleObjectsMax);
                return;
            }

            if (m_MemoryLevel == MemoryLevel.Critical)
            {
                // Really aggressive policy to try to not crash, reduce by 75%
                UpdateCurrentMaxVisibleObjects((int)(m_NbLoadedGameObjects * 0.25f));
            }
            else if (m_MemoryLevel == MemoryLevel.High)
            {
                var ratio = m_NbLoadedGameObjects / (float)m_CurrentMaxVisibleObjects;
                if (m_NbLoadedGameObjects > m_CurrentMaxVisibleObjects)
                {
                    // some gameObjects are expected to be freed, reduce it just a little bit
                    UpdateCurrentMaxVisibleObjects((int)(m_CurrentMaxVisibleObjects * 0.99f));
                }
                else if (ratio >= 0.9f)
                {
                    // nb loaded is close to currently displayed, reduce it just a little more
                    UpdateCurrentMaxVisibleObjects((int)(m_CurrentMaxVisibleObjects * 0.95f));
                }
                else
                {
                    // Drastically drop the max nb objects to current nb loaded as the difference between them is big (in %)
                    UpdateCurrentMaxVisibleObjects(m_NbLoadedGameObjects);
                }
            }
            else if (m_MemoryLevel == MemoryLevel.Medium || m_MemoryLevel == MemoryLevel.Low)
            {
                var isCloseEnough = m_NbLoadedGameObjects / (float)m_CurrentMaxVisibleObjects > 0.95f;
                if (isCloseEnough)
                {
                    if (m_MemoryLevel == MemoryLevel.Low || m_Stats.frameUsedMemory / (float)m_Stats.frameTotalMemory < 0.75f)
                    {
                        // If low, just add +50 objects, if medium, check that the pools still have some remaining space that may be able to accomodate
                        // more objects.
                        UpdateCurrentMaxVisibleObjects(Mathf.Clamp(m_CurrentMaxVisibleObjects + 50, k_MinNbObjects, settings.visibleObjectsMax));
                    }
                }
            }
        }

        void UpdateCurrentMaxVisibleObjects(int newValue)
        {
            m_CurrentMaxVisibleObjects = Mathf.Clamp(newValue, k_MinNbObjects, settings.visibleObjectsMax);
            m_Hub.Broadcast(new MaxNbDisplayableObjectsChanged{ MaxNbObjects = m_CurrentMaxVisibleObjects });
        }

        void OnLoad(SpatialObject obj)
        {
            m_ObjectsToLoad.Push(obj);
        }

        void OnUnload(SpatialObject obj)
        {
            obj.IsLoaded = false;
            m_StreamOutput.SendStreamRemoved(obj.StreamAsset);
        }

        void OnMemoryEvent(MemoryLevelChanged e)
        {
            m_MemoryLevel = e.Level;
            m_IsMemoryLevelFresh = true;
        }

        void Add(ISpatialObject obj)
        {
            spatialCollection.Add(obj);
        }

        void Remove(ISpatialObject obj)
        {
            spatialCollection.Remove(obj);
        }

        public void OnDrawGizmosSelected()
        {
            spatialCollection.DrawDebug(settings.drawNodes ? settings.drawNodesGradient : null,
                settings.drawObjects ? settings.drawObjectsGradient : null,
                settings.drawMaxDepth);
        }

        public ISpatialCollection<ISpatialObject> CreateDefaultSpatialCollection()
        {
            return new RTree(settings.minPerNode, settings.maxPerNode);
        }

        void VisibilityTask(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                // this is where the magic happens
                spatialCollection.Search(m_VisibilityPredicate,
                    m_VisibilityPrioritizer,
                    m_CurrentMaxVisibleObjects,
                    m_VisibilityResults);

                foreach (var obj in m_VisibilityResults.Except(m_PrevVisibilityResults))
                    obj.isVisible = true;

                foreach (var obj in m_PrevVisibilityResults.Except(m_VisibilityResults))
                    obj.isVisible = false;

                // save the results
                m_PrevVisibilityResults.Clear();
                m_PrevVisibilityResults.AddRange(m_VisibilityResults);

                m_IsPendingVisibilityUpdate = true;
            }
        }

        bool DefaultVisibilityPredicate(ISpatialObject obj)
        {
            m_Bounds.SetMinMax(obj.min, obj.max);
            return m_Bounds.SqrDistance(m_CamPos) < m_VisibilitySqrDist;
        }

        float DefaultVisibilityPrioritizer(ISpatialObject obj)
        {
            m_Bounds.SetMinMax(obj.min, obj.max);
            m_BoundsSize = m_Bounds.size;
            m_ObjDirection = m_Bounds.center - m_CamPos;
            // map dot [-1, 1] to [1, 0] so objects closer to m_CamForward have better (lower) score
            return (Vector3.Dot(m_CamForward, m_ObjDirection.normalized) * -0.5f + 0.5f)
                   * m_ObjDirection.sqrMagnitude
                   / (m_BoundsSize.x + m_BoundsSize.y + m_BoundsSize.z);
        }

        void InitCamera()
        {
            m_Camera = Camera.main;
            if (m_Camera == null)
            {
                Debug.LogError($"[{nameof(SpatialFilter)}] active main camera not found!");
                return;
            }

            if (m_CameraTransform == null)
                m_CameraTransform = new GameObject("SpatialFilterCameraTracker").transform;

            m_CameraTransform.SetParent(m_Camera.transform, false);
        }

        void CacheCameraData()
        {
            if (m_Camera == null || !m_Camera.gameObject.activeInHierarchy)
                InitCamera();

            m_CamPos = m_CameraTransform.position;
            m_CamForward = m_CameraTransform.forward;
            m_VisibilitySqrDist = m_Camera.farClipPlane;
            m_VisibilitySqrDist *= m_VisibilitySqrDist;
        }

        void InitBoundingBoxSceneObjects()
        {
            // init bounding boxes
            var amount = m_CurrentMaxVisibleObjects;
            m_BoundingBoxParent = m_Root != null
                ? m_Root
                : new GameObject("BoundingBoxRoot").transform;
            m_BoundingBoxes = new BoundingBoxReference[amount];
            for (var i = 0; i < amount; ++i)
            {
                var obj = Object.Instantiate(settings.boundingBoxPrefab, m_BoundingBoxParent);
                var meshRenderer = obj.GetComponent<MeshRenderer>();
                meshRenderer.enabled = false;
                m_BoundingBoxes[i] = new BoundingBoxReference { transform = obj.transform, meshRenderer = meshRenderer };
            }
        }

        public void Pick(Ray ray, List<ISpatialObject> results)
        {
            m_SelectionRay = ray;

            spatialCollection.Search(CheckIntersectRay,
                GetRayCastDistance,
                settings.selectedObjectsMax,
                results);
        }

        public void Pick(Vector3[] samplePoints, int samplePointCount, List<ISpatialObject> results)
        {
            if (m_SelectionRaySamplePoints == null || m_SelectionRaySamplePoints.Length != samplePointCount - 1)
                m_SelectionRaySamplePoints = new Ray[samplePointCount - 1];

            for (var i = 0; i < m_SelectionRaySamplePoints.Length; ++i)
            {
                m_SelectionRaySamplePoints[i].origin = samplePoints[i];
                m_SelectionRaySamplePoints[i].direction = samplePoints[i + 1] - samplePoints[i];
            }

            spatialCollection.Search(CheckIntersectRaySamplePoints,
                GetRayCastDistance,
                settings.selectedObjectsMax,
                results);
        }

        bool CheckIntersectRaySamplePoints(ISpatialObject obj)
        {
            m_Bounds.SetMinMax(obj.min, obj.max);
            for (var i = 0; i < m_SelectionRaySamplePoints.Length; ++i)
            {
                var ray = m_SelectionRaySamplePoints[i];
                if (!m_Bounds.IntersectRay(ray, out var distance))
                    continue;

                obj.priority = distance;
                if (i > 0)
                    obj.priority += Vector3.Distance(m_SelectionRaySamplePoints[0].origin, ray.origin);

                return true;
            }

            return false;
        }

        bool CheckIntersectRay(ISpatialObject obj)
        {
            m_Bounds.SetMinMax(obj.min, obj.max);
            if (!m_Bounds.IntersectRay(m_SelectionRay, out var distance))
                return false;

            obj.priority = distance;
            return true;
        }

        static float GetRayCastDistance(ISpatialObject obj)
        {
            return obj.priority;
        }
    }
}
