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
        public GameObjectOutput visibilityOutput = new GameObjectOutput();

        public SpatialFilterSettings settings;

        public ISpatialPicker<ISpatialObject> SpatialPicker => processor;

        protected override SpatialFilter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var p = new SpatialFilter(hook.helpers.clock, hook.helpers.memoryStats, hook.services.eventHub, hook.systems.memoryCleaner.memoryLevel,
                settings, assetOutput, visibilityOutput, resolver);

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
            public Vector3 min { get; }
            public Vector3 max { get; }
            public Vector3 center { get; }
            public float priority { get; set; }
            public bool isVisible { get; set; }
            public GameObject loadedObject
            {
                get => SyncedGameObject.data;
                set => SyncedGameObject = new SyncedData<GameObject>(StreamAsset.key, value);
            }

            // the following fields break the Unity code conventions, which suggest using properties instead
            // however, in the worst cases these fields are accessed multiple times per object per frame
            // which causes a massive performance hit when deep profiling in the Unity Editor
            // this is worth the trade-off since this class is private
            public bool IsLoaded;
            public bool WasVisible;
            public readonly SyncedData<StreamAsset> StreamAsset;
            public SyncedData<GameObject> SyncedGameObject;

            public SpatialObject(SyncedData<StreamAsset> streamAsset)
            {
                StreamAsset = streamAsset;
                SyncedGameObject = new SyncedData<GameObject>(streamAsset.key, null);

                var syncBb = streamAsset.data.boundingBox;
                min = new Vector3(syncBb.Min.X, syncBb.Min.Y, syncBb.Min.Z);
                max = new Vector3(syncBb.Max.X, syncBb.Max.Y, syncBb.Max.Z);
                center = min + (max - min) / 2f;
            }

            public void Dispose()
            {
                // nothing to do
            }
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
        readonly GameObjectOutput m_VisibilityOutput;
        public SpatialFilterSettings settings { get; }

        readonly List<ISpatialObject> m_PrevVisibilityResults = new List<ISpatialObject>();
        readonly List<ISpatialObject> m_VisibilityResults = new List<ISpatialObject>();

        Bounds m_Bounds = new Bounds();
        Camera m_Camera;
        Transform m_CameraTransform;
        Transform m_DistanceCheckOrigin;
        float m_VisibilitySqrDist, m_GlobalMaxSqrDistance, m_PickSqrDistance;
        Vector3 m_CamPos, m_CamForward, m_BoundsSize, m_ObjDirection, m_GlobalBoundsSize;

        readonly Dictionary<StreamKey, SpatialObject> m_ObjectsByStreamKey = new Dictionary<StreamKey, SpatialObject>();
        readonly PriorityHeap<SpatialObject> m_ObjectsToLoad;
        readonly PriorityHeap<SpatialObject> m_ObjectsToUnload;

        readonly Func<ISpatialObject, bool> m_VisibilityPredicate;
        readonly Func<ISpatialObject, float> m_VisibilityPrioritizer;

        bool m_ProjectLifecycleStarted;
        volatile bool m_IsPendingVisibilityUpdate;

        Ray m_SelectionRay;
        Ray[] m_SelectionRaySamplePoints;

        readonly SpatialFilterCulling m_Culling;

        public ISpatialCollection<ISpatialObject> spatialCollection { get; }

        bool useDynamicNbVisibleObjects =>
        #if UNITY_IOS || UNITY_ANDROID
            settings.useDynamicNbVisibleObjectsMobile;
        #else
            settings.useDynamicNbVisibleObjectsDesktop;
        #endif

        public SpatialFilter(Clock.Proxy clock, MemoryStats.Proxy stats, EventHub hub, MemoryLevel memoryLevel, SpatialFilterSettings settings,
            StreamAssetOutput streamOutput, GameObjectOutput visibilityOutput, IExposedPropertyTable resolver,
            ISpatialCollection<ISpatialObject> spatialCollection = null,
            Func<ISpatialObject, bool> visibilityPredicate = null,
            Func<ISpatialObject, float> visibilityPrioritizer = null)
        {
            m_Clock = clock;
            m_Stats = stats;
            m_Hub = hub;

            m_MemoryLevel = memoryLevel;
            settings.memoryLevelChanged?.Invoke(m_MemoryLevel);

            this.settings = settings;
            m_Culling = new SpatialFilterCulling(settings.cullingSettings, resolver);
            m_StreamOutput = streamOutput;
            m_VisibilityOutput = visibilityOutput;
            this.spatialCollection = spatialCollection ?? CreateDefaultSpatialCollection();

            m_VisibilityPredicate = visibilityPredicate ?? DefaultVisibilityPredicate;
            m_VisibilityPrioritizer = visibilityPrioritizer ?? DefaultVisibilityPrioritizer;

            m_CurrentMaxVisibleObjects = settings.visibleObjectsMax;

            m_Group = m_Hub.CreateGroup();
            m_Hub.Subscribe<MemoryLevelChanged>(m_Group, OnMemoryEvent);

            InitCamera();

            m_ObjectsToLoad = new PriorityHeap<SpatialObject>(settings.visibleObjectsMax, Comparer<SpatialObject>.Create((a, b) => a.priority.CompareTo(b.priority)));
            m_ObjectsToUnload = new PriorityHeap<SpatialObject>(settings.visibleObjectsMax, Comparer<SpatialObject>.Create((a, b) => b.priority.CompareTo(a.priority)));

            if (useDynamicNbVisibleObjects)
                m_CurrentMaxVisibleObjects = Math.Min(k_MinNbObjects, settings.visibleObjectsMax);
        }

        public override void Dispose()
        {
            base.Dispose();

            if (m_CameraTransform != null && m_CameraTransform.gameObject)
                Object.Destroy(m_CameraTransform.gameObject);

            m_Camera = null;
            m_CameraTransform = null;

            m_Hub.DestroyGroup(m_Group);
        }

        public override void OnPipelineInitialized()
        {
            if (m_ProjectLifecycleStarted)
                return;

            m_ProjectLifecycleStarted = true;

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

            if (settings.useCulling)
                m_Culling.OnUpdate();

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

            var count = Mathf.CeilToInt(settings.streamFactor / Time.deltaTime);

            foreach (var obj in m_ObjectsByStreamKey.Values)
            {
                if (obj.isVisible)
                {
                    if (!obj.IsLoaded)
                        OnLoad(obj);
                    if (!obj.WasVisible)
                        OnShow(obj);
                }
                else
                {
                    if (obj.WasVisible)
                        OnHide(obj);
                    // only unload when past the object limit
                    if (obj.IsLoaded && m_NbLoadedGameObjects > m_CurrentMaxVisibleObjects)
                        OnUnload(obj);
                }
            }

            // all visible objects are already loading, start loading hidden objects
            if (settings.loadHiddenObjects &&
                m_ObjectsToLoad.count < count &&
                m_NbLoadedGameObjects < m_CurrentMaxVisibleObjects &&
                m_NbLoadedGameObjects < spatialCollection.objectCount)
            {
                var nbToLoad = Mathf.Min(count - m_ObjectsToLoad.count, m_CurrentMaxVisibleObjects - m_NbLoadedGameObjects);
                foreach (var obj in m_ObjectsByStreamKey.Values)
                {
                    if (!obj.isVisible && !obj.IsLoaded)
                    {
                        OnLoad(obj);
                        --nbToLoad;
                    }

                    if (nbToLoad <= 0)
                        break;
                }
            }

            if (m_ObjectsToUnload.count > 0)
            {
                for (var i = 0; i < count; ++i)
                {
                    if (!m_ObjectsToUnload.TryPop(out var obj))
                        break;

                    obj.IsLoaded = false;
                    m_StreamOutput.SendStreamRemoved(obj.StreamAsset);
                }

                // clear any remaining objects, they will re-add themselves during the next visibility update
                m_ObjectsToUnload.Clear();
            }

            if (m_ObjectsToLoad.count <= 0)
                return;

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

            m_GlobalBoundsSize = spatialCollection.bounds.size;
            m_GlobalMaxSqrDistance = Mathf.Max(m_GlobalBoundsSize.x, m_GlobalBoundsSize.y, m_GlobalBoundsSize.z);
            m_GlobalMaxSqrDistance *= m_GlobalMaxSqrDistance;
        }

        public void OnStreamInstanceEnd()
        {
            var bounds = spatialCollection.bounds;
            Debug.Log($"[Spatial Filter] depth: {spatialCollection.depth}, " +
                      $"# objects: {spatialCollection.objectCount}, " +
                      $"bounds: [{bounds.min}, {bounds.max}]");

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
                    obj.loadedObject = gameObject.data;
                    ++m_NbLoadedGameObjects;
                    if (m_NbLoadedGameObjects >= m_CurrentMaxVisibleObjects * 0.98f)
                        UpdateSpeculation();
                    break;
                case StreamEvent.Changed:
                    obj.loadedObject = gameObject.data;
                    break;
                case StreamEvent.Removed:
                    obj.loadedObject = null;
                    --m_NbLoadedGameObjects;
                    break;
            }
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
            m_ObjectsToUnload.Push(obj);
        }

        void OnShow(SpatialObject obj)
        {
            m_VisibilityOutput.SendStreamAdded(obj.SyncedGameObject);
            obj.WasVisible = true;
        }

        void OnHide(SpatialObject obj)
        {
            m_VisibilityOutput.SendStreamRemoved(obj.SyncedGameObject);
            obj.WasVisible = false;
        }

        void OnMemoryEvent(MemoryLevelChanged e)
        {
            m_MemoryLevel = e.Level;
            m_IsMemoryLevelFresh = true;
            settings.memoryLevelChanged?.Invoke(e.Level);
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
                settings.priorityWeightAngle + settings.priorityWeightDistance + settings.priorityWeightSize,
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
            if (settings.useCulling)
                return m_Culling.IsVisible(obj);

            m_Bounds.SetMinMax(obj.min, obj.max);
            return m_Bounds.SqrDistance(m_CamPos) < m_VisibilitySqrDist;
        }

        float DefaultVisibilityPrioritizer(ISpatialObject obj)
        {
            m_Bounds.SetMinMax(obj.min, obj.max);
            m_BoundsSize = m_Bounds.size;
            m_ObjDirection = m_Bounds.ClosestPoint(m_CamPos) - m_CamPos;
            // lower priority is better so change sign after adding the weighted values
            return -1f *
                   // backward [0-1] forward
                   (settings.priorityWeightAngle * (1f - Vector3.Angle(m_CamForward, m_ObjDirection) / 180f)
                    // farther [0-1] closer
                    + settings.priorityWeightDistance * (1f - m_ObjDirection.sqrMagnitude / m_GlobalMaxSqrDistance)
                    // smaller [0-1] larger
                    + settings.priorityWeightSize * (m_BoundsSize.x + m_BoundsSize.y + m_BoundsSize.z) / (m_GlobalBoundsSize.x + m_GlobalBoundsSize.y + m_GlobalBoundsSize.z));
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

            m_Culling.SetCamera(m_Camera);
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

        public void Pick(Ray ray, List<ISpatialObject> results)
        {
            m_SelectionRay = ray;

            spatialCollection.Search(CheckIntersectRay,
                GetRayCastDistance,
                settings.selectedObjectsMax,
                results);
        }

        public void VRPick(Ray ray, List<ISpatialObject> results)
        {
            Pick(ray, results);
        }

        public void Pick(float distance, List<ISpatialObject> results, Transform origin)
        {
            m_DistanceCheckOrigin = origin;
            m_PickSqrDistance = distance * distance;

            spatialCollection.Search(CheckDistance,
                GetRayCastDistance,
                k_MinNbObjects,
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

        bool CheckDistance(ISpatialObject obj)
        {
            m_Bounds.SetMinMax(obj.min, obj.max);
            var distance = m_Bounds.SqrDistance(m_DistanceCheckOrigin.position);
            if (distance > m_PickSqrDistance)
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
