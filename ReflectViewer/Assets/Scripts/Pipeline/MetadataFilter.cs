using System.Collections.Generic;
using System.Linq;
using Unity.Reflect;
using Unity.Reflect.Model;
using UnityEngine.Reflect.Pipeline;
#if URP_AVAILABLE
using UnityEngine.Rendering.Universal;
#endif

namespace UnityEngine.Reflect.Viewer.Pipeline
{
    public class MetadataFilterNode : ReflectNode<MetadataFilter>
    {
        public StreamInstanceInput instanceInput = new StreamInstanceInput();
        public GameObjectInput gameObjectInput = new GameObjectInput();

        public MetadataFilterSettings settings;

        protected override MetadataFilter Create(ReflectBootstrapper hook, ISyncModelProvider provider, IExposedPropertyTable resolver)
        {
            var p = new MetadataFilter(settings);

            instanceInput.streamEvent = p.OnStreamInstanceEvent;
            instanceInput.streamEnd = p.OnStreamInstanceEnd;

            gameObjectInput.streamEvent = p.OnGameObjectAdded;
            gameObjectInput.streamEnd = p.OnGameObjectEnd;

            return p;
        }
    }


    public class MetadataFilter : IReflectNodeProcessor
    {
        FilterData m_HighlightedFilterData;
        MetadataFilterSettings m_Settings;

        public static readonly int k_SelectedLayer = LayerMask.NameToLayer("BimFilterSelect");
        public static readonly int k_OtherLayer = LayerMask.NameToLayer("BimFilterOthers");
        public static readonly int k_LayerMask = LayerMask.GetMask("BimFilterSelect", "BimFilterOthers");

        public MetadataFilter(MetadataFilterSettings settings)
        {
            m_Settings = settings;
        }

        class FilterData
        {
            public bool visible = true;
            public HashSet<GameObject> instances = new HashSet<GameObject>();
        }

        HashSet<GameObject> m_GameObjects = new HashSet<GameObject>();

        Dictionary<string, Dictionary<string, FilterData>> m_FilterGroups =
            new Dictionary<string, Dictionary<string, FilterData>>();

        public IEnumerable<string> GetFilterKeys(string groupKey)
        {
            if (m_FilterGroups.TryGetValue(groupKey, out var dicFilterData))
                return dicFilterData.Keys.OrderBy(e => e);

            return null;
        }

        public void OnStreamInstanceEvent(SyncedData<StreamInstance> streamInstance, StreamEvent streamEvent)
        {
            if (streamEvent == StreamEvent.Added)
            {
                OnStreamAdded(streamInstance);
            }
        }

        void OnStreamAdded(SyncedData<StreamInstance> stream)
        {
            var syncMetadata = stream.data.instance.Metadata;

            if (syncMetadata == null)
                return;


            foreach (var key in m_Settings.m_Safelist)
            {
                if (syncMetadata.Parameters.TryGetValue(key, out SyncParameter category))
                {
                    if (string.IsNullOrEmpty(category.Value))
                        continue;

                    if (!m_FilterGroups.ContainsKey(key))
                    {
                        m_FilterGroups[key] = new Dictionary<string, FilterData>();
                        m_Settings.groupsChanged?.Invoke(m_FilterGroups.Keys.OrderBy(e => e));
                    }

                    var dicFilterData = m_FilterGroups[key];
                    if (!dicFilterData.ContainsKey(category.Value))
                    {
                        dicFilterData[category.Value] = new FilterData();
                        m_Settings.categoriesChanged?.Invoke(key, dicFilterData.Keys.OrderBy(e => e));
                    }
                }
            }
        }

        public void OnStreamInstanceEnd()
        {
        }

        public void OnGameObjectEnd()
        {
        }

        public void OnGameObjectAdded(SyncedData<GameObject> gameObject, StreamEvent streamEvent)
        {
            if (streamEvent == StreamEvent.Added)
            {
                OnStreamAdded(gameObject);
            }
            else if (streamEvent == StreamEvent.Removed)
            {
                OnStreamRemoved(gameObject);
            }
        }

        void OnStreamAdded(SyncedData<GameObject> stream)
        {
            if (!stream.data.TryGetComponent<Metadata>(out var metadata))
                return;

            m_GameObjects.Add(stream.data);

            bool setHighlight = false;

            foreach (var groupKey in m_Settings.m_Safelist)
            {
                if (metadata.GetParameters().TryGetValue(groupKey, out Metadata.Parameter category))
                {
                    if (!GetFilterData(groupKey, category.value, out var filterData))
                        return;

                    filterData.instances.Add(stream.data);

                    if (!filterData.visible)
                        stream.data.SetActive(false);

                    if (m_HighlightedFilterData == filterData)
                    {
                        setHighlight = true;
                    }
                }
            }
            if (m_HighlightedFilterData != null)
            {
                stream.data.SetLayerRecursively(setHighlight ? k_SelectedLayer : k_OtherLayer);
            }
        }

        void OnStreamRemoved(SyncedData<GameObject> stream)
        {
            if (!stream.data.TryGetComponent<Metadata>(out var metadata))
                return;

            m_GameObjects.Remove(stream.data);

            foreach (var groupKey in m_Settings.m_Safelist)
            {
                if (metadata.GetParameters().TryGetValue(groupKey, out Metadata.Parameter category))
                {
                    if (!GetFilterData(groupKey, category.value, out var filterData))
                        return;

                    filterData.instances.Remove(stream.data);
                }
            }
        }

        public bool IsVisible(string groupKey, string filterKey)
        {
            if (!GetFilterData(groupKey, filterKey, out var filterData))
                return true;

            return filterData.visible;
        }


        public void SetVisibility(string groupKey, string filterKey, bool visible)
        {
            if (!GetFilterData(groupKey, filterKey, out var filterData))
                return;

            if (filterData.visible == visible)
                return;

            filterData.visible = visible;

            foreach (var instance in filterData.instances)
            {
                instance.SetActive(visible);
            }
        }

        public bool IsHighlighted(string groupKey, string filterKey)
        {
            if (!GetFilterData(groupKey, filterKey, out var filterData))
                return true;

            return filterData == m_HighlightedFilterData;
        }

        public void SetHighlightFilter(string groupKey, string filterKey)
        {
            if (!GetFilterData(groupKey, filterKey, out var filterData))
                return;

            if (m_HighlightedFilterData == filterData)
            {
                m_HighlightedFilterData = null;
                RestoreHighlight();
                return;
            }

            m_HighlightedFilterData = filterData;

            foreach (var obj in m_GameObjects)
            {
                obj.SetLayerRecursively(filterData.instances.Contains(obj) ? k_SelectedLayer : k_OtherLayer);
            }

            m_HighlightedFilterData = filterData;
        }

        bool GetFilterData(string groupKey, string filterKey, out FilterData filterData)
        {
            filterData = null;
            return m_FilterGroups.TryGetValue(groupKey, out var dicFilterData) &&
                dicFilterData.TryGetValue(filterKey, out filterData);
        }

        void RestoreHighlight()
        {
            var defaultLayer = LayerMask.NameToLayer("Default");
            foreach (var obj in m_GameObjects)
            {
                obj.SetLayerRecursively(defaultLayer);
            }
        }

#if URP_AVAILABLE
        readonly Dictionary<ForwardRendererData, LayerMask> m_OpaqueLayerMasks = new Dictionary<ForwardRendererData, LayerMask>();
        readonly Dictionary<ForwardRendererData, LayerMask> m_TransparentLayerMasks = new Dictionary<ForwardRendererData, LayerMask>();
#endif

        public void OnPipelineInitialized()
        {
#if URP_AVAILABLE
            foreach (var data in m_Settings.forwardRendererDatas)
            {
                m_OpaqueLayerMasks[data] = data.opaqueLayerMask;
                m_TransparentLayerMasks[data] = data.transparentLayerMask;
                data.opaqueLayerMask &= ~k_LayerMask;
                data.transparentLayerMask &= ~k_LayerMask;
            }
#endif
        }

        public void OnPipelineShutdown()
        {
#if URP_AVAILABLE
            foreach (var data in m_Settings.forwardRendererDatas)
            {
                data.opaqueLayerMask = m_OpaqueLayerMasks[data];
                data.transparentLayerMask = m_TransparentLayerMasks[data];
            }
#endif

            m_GameObjects.Clear();
            m_FilterGroups.Clear();
            m_HighlightedFilterData = null;
            m_Settings.groupsChanged?.Invoke(Enumerable.Empty<string>());
        }
    }
}
