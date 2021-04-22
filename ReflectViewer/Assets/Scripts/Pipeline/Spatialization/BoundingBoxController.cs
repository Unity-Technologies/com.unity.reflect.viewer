using System;
using System.Collections.Generic;
using System.IO;
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
    public class BoundingBoxControllerNode : ReflectNode<BoundingBoxController>
    {
        public StreamAssetInput assetInput = new StreamAssetInput();
        public StreamAssetInput filteredInput = new StreamAssetInput();
        public StreamInstanceInput instanceInput = new StreamInstanceInput();
        public StreamInstanceDataInput instanceDataInput = new StreamInstanceDataInput();
        public GameObjectInput gameObjectInput = new GameObjectInput();
        public GameObjectInput visibilityInput = new GameObjectInput();

        public BoundingBoxSettings settings;

        protected override BoundingBoxController Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var p = new BoundingBoxController(hook.services.eventHub, settings.boundingBoxRoot.Resolve(resolver), settings);

            assetInput.streamEvent = p.OnAssetEvent;
            filteredInput.streamEvent = p.OnFilteredAssetEvent;
            instanceInput.streamEvent = p.OnInstanceEvent;
            instanceDataInput.streamEvent = p.OnInstanceDataEvent;
            gameObjectInput.streamEvent = p.OnGameObjectEvent;
            visibilityInput.streamEvent = p.OnVisibilityEvent;

            assetInput.streamEnd = p.OnAssetEnd;

            return p;
        }
    }

    public sealed class BoundingBoxController : ReflectTaskNodeProcessor
    {
        class BoundingBoxReference
        {
            public GameObject gameObject;
            public MeshRenderer meshRenderer;
            public StreamState streamState;
        }

        EventHub m_Hub;
        EventHub.Handle m_ErrorHandle;

        readonly Dictionary<StreamKey, BoundingBoxReference> m_BoundingBoxesByStreamKey = new Dictionary<StreamKey, BoundingBoxReference>();
        readonly Dictionary<StreamKey, GameObject> m_GameObjectsByStreamKey = new Dictionary<StreamKey, GameObject>();
        readonly Dictionary<StreamState, Material> m_DefaultMaterialPresets = new Dictionary<StreamState, Material>();
        readonly Dictionary<StreamState, Material> m_DebugMaterialPresets = new Dictionary<StreamState, Material>();
        readonly Transform m_Root;

        Bounds m_GlobalBounds;
        Transform m_BoundingBoxParent;
        List<BoundingBoxReference> m_BoundingBoxPool;
        bool m_DisplayOnlyBoundingBoxes;
        bool m_UseDebugMaterials;

        public BoundingBoxSettings settings { get; }

        Dictionary<StreamState, Material> CurrentMaterialPresets =>
            m_UseDebugMaterials ? m_DebugMaterialPresets : m_DefaultMaterialPresets;

        public BoundingBoxController(EventHub hub, Transform root, BoundingBoxSettings settings)
        {
            m_Hub = hub;
            this.settings = settings;
            m_Root = root;

            foreach (var boundingBoxMaterial in settings.defaultBoundingBoxMaterials)
                m_DefaultMaterialPresets.Add(boundingBoxMaterial.streamState, boundingBoxMaterial.material);

            foreach (var boundingBoxMaterial in settings.debugBoundingBoxMaterials)
                m_DebugMaterialPresets.Add(boundingBoxMaterial.streamState, boundingBoxMaterial.material);

            InitBoundingBoxPool();
        }

        protected override Task RunInternal(CancellationToken token)
        {
            throw new NotImplementedException();
        }

        protected override void UpdateInternal(float unscaledDeltaTime)
        {
            if (m_UseDebugMaterials != settings.useDebugMaterials)
            {
                m_UseDebugMaterials = settings.useDebugMaterials;
                foreach (var kvp in m_BoundingBoxesByStreamKey)
                    SetStreamState(kvp.Value, kvp.Value.streamState);
            }

            if (m_DisplayOnlyBoundingBoxes == settings.displayOnlyBoundingBoxes)
                return;

            m_DisplayOnlyBoundingBoxes = settings.displayOnlyBoundingBoxes;

            SetDisplayOnlyBoundingBoxes(m_DisplayOnlyBoundingBoxes);
        }

        void OnStreamingError(StreamingErrorEvent e)
        {
            AddOrEditBoundingBox(e.Key, e.BoundingBox, StreamState.Invalid, true);
        }

        void AddOrEditBoundingBox(StreamKey key, SyncBoundingBox boundingBox, StreamState state, bool show)
        {
            if (!m_BoundingBoxesByStreamKey.TryGetValue(key, out var box))
            {
                box = GetFreeBoundingBoxReference();
                SetBoundingBoxValues(box, boundingBox);
                m_BoundingBoxesByStreamKey.Add(key, box);
            }

            box.gameObject.SetActive(show);
            SetStreamState(box, state);
        }

        void InitBoundingBoxPool()
        {
            // init bounding boxes
            m_BoundingBoxParent = m_Root != null
                ? m_Root
                : new GameObject("BoundingBoxRoot").transform;
            m_BoundingBoxPool = new List<BoundingBoxReference>(settings.initialBoundingBoxPoolSize);
            for (var i = 0; i < settings.initialBoundingBoxPoolSize; ++i)
                m_BoundingBoxPool.Add(CreateBoundingBoxReference());
        }

        BoundingBoxReference CreateBoundingBoxReference()
        {
            var obj = Object.Instantiate(settings.boundingBoxPrefab, m_BoundingBoxParent);
            var meshRenderer = obj.GetComponent<MeshRenderer>();
            obj.gameObject.SetActive(false);
            return new BoundingBoxReference { gameObject = obj, meshRenderer = meshRenderer };
        }

        BoundingBoxReference GetFreeBoundingBoxReference()
        {
            if (m_BoundingBoxPool.Count <= 0)
                return CreateBoundingBoxReference();

            var lastIndex = m_BoundingBoxPool.Count - 1;
            var bb = m_BoundingBoxPool[lastIndex];
            m_BoundingBoxPool.RemoveAt(lastIndex);
            return bb;
        }

        void ReleaseBoundingBoxReference(BoundingBoxReference boundingBoxReference)
        {
            if (boundingBoxReference == null || boundingBoxReference.gameObject == null)
                return;
            
            boundingBoxReference.gameObject.SetActive(false);
            boundingBoxReference.streamState = StreamState.Asset;
            m_BoundingBoxPool.Add(boundingBoxReference);
        }

        void SetBoundingBoxValues(BoundingBoxReference boundingBoxReference, SyncBoundingBox syncBoundingBox)
        {
            var min = new Vector3(syncBoundingBox.Min.X, syncBoundingBox.Min.Y, syncBoundingBox.Min.Z);
            var max = new Vector3(syncBoundingBox.Max.X, syncBoundingBox.Max.Y, syncBoundingBox.Max.Z);
            EncapsulateGlobalBounds(min, max);
            var size = max - min;
            boundingBoxReference.gameObject.transform.localPosition = min + (size / 2);
            boundingBoxReference.gameObject.transform.localScale = size;
        }

        void SetStreamState(BoundingBoxReference boundingBoxReference, StreamState streamState)
        {
            if (boundingBoxReference.streamState == StreamState.Invalid)
                return;

            boundingBoxReference.streamState = streamState;

            if (CurrentMaterialPresets.TryGetValue(streamState, out var material))
            {
                boundingBoxReference.meshRenderer.sharedMaterial = material;
            }
        }

        public void OnAssetEvent(SyncedData<StreamAsset> streamAsset, StreamEvent eventType)
        {
            if (!PersistentKey.IsKeyFor<SyncObjectInstance>(streamAsset.key.key) ||
                streamAsset.data.boundingBox.Max.Equals(streamAsset.data.boundingBox.Min))
                return;

            switch (eventType)
            {
                case StreamEvent.Added:
                    AddOrEditBoundingBox(streamAsset.data.key, streamAsset.data.boundingBox, StreamState.Asset, false);
                    break;
                case StreamEvent.Changed:
                    if (!m_BoundingBoxesByStreamKey.TryGetValue(streamAsset.key, out var box))
                        break;
                    SetBoundingBoxValues(box, streamAsset.data.boundingBox);
                    break;
                case StreamEvent.Removed:
                    ReleaseBoundingBoxReference(m_BoundingBoxesByStreamKey[streamAsset.key]);
                    m_BoundingBoxesByStreamKey.Remove(streamAsset.key);
                    break;
            }
        }

        public void OnAssetEnd()
        {
            Debug.Log($"[Bounding Box Controller]: [{m_GlobalBounds.min}, {m_GlobalBounds.max}]");

            settings.onGlobalBoundsCalculated?.Invoke(m_GlobalBounds);

            if (settings.useStaticBatching)
                StaticBatchingUtility.Combine(m_Root.gameObject);
        }

        public void OnFilteredAssetEvent(SyncedData<StreamAsset> streamAsset, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    if (m_BoundingBoxesByStreamKey.TryGetValue(streamAsset.key, out var box))
                        SetStreamState(box, StreamState.FilteredAsset);
                    break;
                case StreamEvent.Removed:
                    if (m_BoundingBoxesByStreamKey.TryGetValue(streamAsset.key, out box))
                    {
                        SetStreamState(box, StreamState.Removed);
                        box.gameObject.SetActive(true);
                    }
                    break;
            }
        }

        public void OnInstanceEvent(SyncedData<StreamInstance> streamInstance, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    if (m_BoundingBoxesByStreamKey.TryGetValue(streamInstance.key, out var box))
                        SetStreamState(box, StreamState.Instance);
                    break;
            }
        }

        public void OnInstanceDataEvent(SyncedData<StreamInstanceData> streamInstanceData, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    if (m_BoundingBoxesByStreamKey.TryGetValue(streamInstanceData.key, out var box))
                        SetStreamState(box, StreamState.InstanceData);
                    break;
            }
        }

        public void OnGameObjectEvent(SyncedData<GameObject> gameObject, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    m_GameObjectsByStreamKey[gameObject.key] = gameObject.data;
                    if (m_BoundingBoxesByStreamKey.TryGetValue(gameObject.key, out var box))
                    {
                        SetStreamState(box, StreamState.GameObject);
                        // deactivate gameObject if box was inactive for loading hidden objects
                        var wasActive = box.gameObject.activeSelf;
                        gameObject.data.SetActive(wasActive);
                        box.gameObject.SetActive(m_DisplayOnlyBoundingBoxes && wasActive);
                    }
                    if (m_DisplayOnlyBoundingBoxes)
                        gameObject.data.SetActive(false);
                    if (settings.useStaticBatching && m_GameObjectsByStreamKey.Count == m_BoundingBoxesByStreamKey.Count)
                        StaticBatchingUtility.Combine(gameObject.data.transform.parent.gameObject);
                    break;
                // TODO: changed
                case StreamEvent.Removed:
                    m_GameObjectsByStreamKey.Remove(gameObject.key);
                    if (m_BoundingBoxesByStreamKey.TryGetValue(gameObject.key, out box))
                        box.gameObject.SetActive(true);
                    break;
            }
        }

        public void OnVisibilityEvent(SyncedData<GameObject> gameObject, StreamEvent eventType)
        {
            switch (eventType)
            {
                case StreamEvent.Added:
                    if (m_BoundingBoxesByStreamKey.TryGetValue(gameObject.key, out var box))
                        box.gameObject.SetActive(m_DisplayOnlyBoundingBoxes || gameObject.data == null);
                    if (m_GameObjectsByStreamKey.TryGetValue(gameObject.key, out var obj))
                        obj.SetActive(!m_DisplayOnlyBoundingBoxes && gameObject.data != null);
                    break;
                case StreamEvent.Removed:
                    if (m_BoundingBoxesByStreamKey.TryGetValue(gameObject.key, out box))
                        box.gameObject.SetActive(false);
                    if (m_GameObjectsByStreamKey.TryGetValue(gameObject.key, out obj))
                        obj.SetActive(false);
                    break;
            }
        }

        void SetDisplayOnlyBoundingBoxes(bool displayOnlyBoundingBoxes)
        {
            foreach (var kvp in m_GameObjectsByStreamKey)
            {
                kvp.Value.SetActive(!displayOnlyBoundingBoxes);
                if (m_BoundingBoxesByStreamKey.TryGetValue(kvp.Key, out var box))
                    box.gameObject.SetActive(displayOnlyBoundingBoxes);
            }
        }

        void EncapsulateGlobalBounds(Vector3 min, Vector3 max)
        {
            if (min.Equals(max))
                return;

            if (!m_GlobalBounds.size.Equals(Vector3.zero))
            {
                m_GlobalBounds.Encapsulate(min);
                m_GlobalBounds.Encapsulate(max);
                return;
            }

            m_GlobalBounds.SetMinMax(min, max);
        }

        public override void OnPipelineInitialized()
        {
            m_GlobalBounds.size = Vector3.zero;
            m_ErrorHandle = m_Hub.Subscribe<StreamingErrorEvent>(OnStreamingError);
        }

        public override void OnPipelineShutdown()
        {
            m_Hub.Unsubscribe(m_ErrorHandle);

            foreach (var kvp in m_BoundingBoxesByStreamKey)
                ReleaseBoundingBoxReference(kvp.Value);

            m_BoundingBoxesByStreamKey?.Clear();
            m_GameObjectsByStreamKey?.Clear();

            base.OnPipelineShutdown();
        }
    }
}
