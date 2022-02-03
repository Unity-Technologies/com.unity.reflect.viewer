using System;
using System.Collections.Generic;
using Unity.Reflect.Actors;
using Unity.Reflect.Viewer.UI;

namespace Unity.Reflect.Viewer
{
    public class MetadataCategoriesChanged
    {
        public string GroupKey;
        public Diff<string> FilterKeys;

        public MetadataCategoriesChanged(string groupKey, Diff<string> filterKeys)
        {
            GroupKey = groupKey;
            FilterKeys = filterKeys;
        }
    }

    public class MetadataGroupsChanged
    {
        public List<string> GroupKeys;

        public MetadataGroupsChanged(List<string> groupKeys)
        {
            GroupKeys = groupKeys;
        }
    }
    
    public class SetObjectState
    {
        public List<DynamicGuid> Instances;
        public bool Enabled;
    }

    public class MetadataGroup : IEquatable<MetadataGroup>
    {
        public string Group;
        public bool Equals(MetadataGroup other)
        {
            return Group == other.Group;
        }
    }

    // This class is the same as the layout as the struct HighlightFilterInfo
    // (should probably be renamed to MetadataGroupFilter or something like that later)
    // If it fits please change this to Boxed<HighlightFilterInfo> later
    public class MetadataCategoryGroup : IEquatable<MetadataCategoryGroup>
    {
        public string FilterKey;
        public string Group;

        public bool Equals(MetadataCategoryGroup other)
        {
            return FilterKey == other.FilterKey && Group == other.Group;
        }
    }

    public class MetadataCategoryGroupToggle : Boxed<FilterItemInfo>
    {
        public MetadataCategoryGroupToggle(FilterItemInfo filterItemInfo)
            : base(filterItemInfo) { }
    }

    public class ObjectMetadataChanged
    {
        public List<(DynamicGuid, Dictionary<string, string>)> IdToGroupToFilterKeys;

        public ObjectMetadataChanged(List<(DynamicGuid, Dictionary<string, string>)> idToGroupToFilterKeys)
        {
            IdToGroupToFilterKeys = idToGroupToFilterKeys;
        }
    }

    public class SetHighlight
    {
        public List<DynamicGuid> HighlightedInstances;
    }
    public class AddToHighlight
    {
        public List<DynamicGuid> HighlightedInstances;
    }
    public class RemoveFromHighlight
    {
        public List<DynamicGuid> OtherInstances;
    }

    public class CancelHighlight { }

}
