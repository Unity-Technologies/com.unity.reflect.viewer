using System.Collections.Generic;
using System.Linq;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using UnityEngine.Reflect;

namespace Unity.Reflect.Viewer.Actors
{
    [Actor("dc9af142-51f8-4f93-af84-63766c250703", true)]
    public class ObjectMetadataCacheActor
    {
#pragma warning disable 649
        NetOutput<ToggleGameObject> m_ToggleGameObjectOutput;
        EventOutput<MetadataCategoriesChanged> m_MetadataCategoriesChangedOutput;
        EventOutput<MetadataGroupsChanged> m_MetadataGroupsChangedOutput;
        EventOutput<ObjectMetadataChanged> m_ObjectMetadataChanged;
#pragma warning restore 649

        protected string[] m_SafeList =
        {
            "Category", "Family", "Document", "System Classification", "Type", "Manufacturer", "Phase Created",
            "Phase Demolished", "Layer"
        };

        protected Dictionary<string, Dictionary<string, List<DynamicGuid>>> m_FilterGroups =
            new Dictionary<string, Dictionary<string, List<DynamicGuid>>>();

        Dictionary<string, List<string>> m_DisabledGroups = new Dictionary<string, List<string>>();

        [PipeInput]
        void OnGameObjectCreating(PipeContext<GameObjectCreating> ctx)
        {
            var idToGroupToFilterKeys = new List<(DynamicGuid, Dictionary<string, string>)>();
            var toggleCounts = new List<(DynamicGuid, int)>();

            foreach (var go in ctx.Data.GameObjectIds)
            {
                if (!go.GameObject.TryGetComponent<Metadata>(out var metadata))
                    continue;

                var groupToFilterKeys = new Dictionary<string, string>();

                foreach (var groupKey in m_SafeList)
                {
                    if (!metadata.GetParameters().TryGetValue(groupKey, out Metadata.Parameter category))
                        continue;

                    if (string.IsNullOrEmpty(category.value))
                        continue;

                    if (!m_FilterGroups.TryGetValue(groupKey, out var dicFilterData))
                    {
                        dicFilterData = m_FilterGroups[groupKey] = new Dictionary<string, List<DynamicGuid>>();
                        m_MetadataGroupsChangedOutput.Broadcast(new MetadataGroupsChanged(m_FilterGroups.Keys.OrderBy(e => e).ToList()));
                    }

                    if (!dicFilterData.TryGetValue(category.value, out var filterData))
                    {
                        filterData = dicFilterData[category.value] = new List<DynamicGuid>();
                        var diff = new Diff<string>();
                        diff.Added.Add(category.value);
                        m_MetadataCategoriesChangedOutput.Broadcast(new MetadataCategoriesChanged(groupKey, diff));
                    }

                    filterData.Add(go.Id);
                    groupToFilterKeys.Add(groupKey, category.value);
                }

                if (groupToFilterKeys.Count == 0)
                    continue;

                idToGroupToFilterKeys.Add((go.Id, groupToFilterKeys));
                var nbEnabled = -groupToFilterKeys.Count(x => m_DisabledGroups.TryGetValue(x.Key, out var filterKeys) && filterKeys.Contains(x.Value));
                if (nbEnabled < 0)
                    toggleCounts.Add((go.Id, nbEnabled));
            }

            if (idToGroupToFilterKeys.Count > 0)
                m_ObjectMetadataChanged.Broadcast(new ObjectMetadataChanged(idToGroupToFilterKeys));

            if (toggleCounts.Count > 0)
                m_ToggleGameObjectOutput.Send(new ToggleGameObject(toggleCounts));

            ctx.Continue();
        }

        [RpcInput]
        void GetMetadataCategoriesByGroup(RpcContext<MetadataGroup> ctx)
        {
            if(m_FilterGroups.TryGetValue(ctx.Data.Group, out var dicFilterData))
                ctx.SendSuccess(dicFilterData.Keys.ToArray());
            else
                ctx.SendSuccess(new  List<DynamicGuid>());
        }

        [RpcInput]
        void GetObjectByMetadataGroup(RpcContext<MetadataCategoryGroup> ctx)
        {
            if(GetFilterData(ctx.Data.Group, ctx.Data.FilterKey, out var ids))
                ctx.SendSuccess(ids);
            else
                ctx.SendSuccess(new  List<DynamicGuid>());
        }

        [NetInput]
        void ToggleObjectsByMetadataGroup(NetContext<MetadataCategoryGroupToggle> ctx)
        {
            var group = ctx.Data.Value.groupKey;
            var filter = ctx.Data.Value.filterKey;
            var visible = ctx.Data.Value.visible;

            if (!m_DisabledGroups.ContainsKey(group))
                m_DisabledGroups.Add(group, new List<string>());

            if (visible)
                m_DisabledGroups[group].Remove(filter);
            else
                m_DisabledGroups[group].Add(filter);

            if (GetFilterData(group, filter, out var ids))
                m_ToggleGameObjectOutput.Send(new ToggleGameObject(ids.Select(x => (x, visible ? 1 : -1)).ToList()));
        }

        protected bool GetFilterData(string groupKey, string filterKey, out List<DynamicGuid> filterData)
        {
            filterData = null;
            return m_FilterGroups.TryGetValue(groupKey, out var dicFilterData) &&
                dicFilterData.TryGetValue(filterKey, out filterData);
        }
    }
}
