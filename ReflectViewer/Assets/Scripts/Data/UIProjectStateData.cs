using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    [Serializable]
    public struct ObjectSelectionInfo : IEquatable<ObjectSelectionInfo>, SelectObjectAction.IObjectSelectionInfo
    {
        public List<GameObject> selectedObjects;
        public int currentIndex;
        public string userId;
        public int colorId;

        public GameObject CurrentSelectedObject()
        {
            if (selectedObjects != null && selectedObjects.Count > currentIndex)
                return selectedObjects[currentIndex];
            return null;
        }

        public override bool Equals(object obj)
        {
            return obj is ObjectSelectionInfo info && Equals(info);
        }

        public bool Equals(ObjectSelectionInfo other)
        {
            if (currentIndex != other.currentIndex)
                return false;
            if (userId != other.userId)
                return false;
            if (colorId != other.colorId)
                return false;
            if (selectedObjects != null && other.selectedObjects != null)
                return selectedObjects.SequenceEqual(other.selectedObjects);
            return selectedObjects == null && other.selectedObjects == null;
        }

        public static bool operator ==(ObjectSelectionInfo a, ObjectSelectionInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ObjectSelectionInfo a, ObjectSelectionInfo b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                if (selectedObjects == null || selectedObjects.Count == 0)
                    return currentIndex;

                var hashCode = selectedObjects.Aggregate(currentIndex, (hash, obj) => (hash * 397) ^ obj.GetHashCode());
                hashCode = (hashCode * 397) ^ userId.GetHashCode();
                hashCode = (hashCode * 397) ^ colorId.GetHashCode();

                return hashCode;
            }
        }
    }

    [Serializable]
    public struct HighlightFilterInfo : IEquatable<HighlightFilterInfo>, IHighlightFilterInfo
    {
        public string groupKey { get; set; }
        public string filterKey { get; set; }

        public bool IsValid
        {
            get => !string.IsNullOrWhiteSpace(groupKey) && !string.IsNullOrWhiteSpace(filterKey);
            set => IsValid = value;
        }

        public static bool operator ==(HighlightFilterInfo a, HighlightFilterInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(HighlightFilterInfo a, HighlightFilterInfo b)
        {
            return !(a == b);
        }

        public bool Equals(HighlightFilterInfo other)
        {
            return groupKey == other.groupKey && filterKey == other.filterKey;
        }

        public override bool Equals(object obj)
        {
            return obj is HighlightFilterInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((groupKey != null ? groupKey.GetHashCode() : 0) * 397) ^
                    (filterKey != null ? filterKey.GetHashCode() : 0);
            }
        }
    }

    [Serializable]
    public struct CameraTransformInfo : IEquatable<CameraTransformInfo>, ICameraTransformInfo
    {
        public Vector3 position;
        public Vector3 rotation;

        public static bool operator ==(CameraTransformInfo a, CameraTransformInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(CameraTransformInfo a, CameraTransformInfo b)
        {
            return !(a == b);
        }

        public bool Equals(CameraTransformInfo other)
        {
            return position == other.position && rotation == other.rotation;
        }

        public override bool Equals(object obj)
        {
            return obj is CameraTransformInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (position.GetHashCode() * 397) ^ rotation.GetHashCode();
            }
        }
    }

    [Serializable]
    public struct FilterItemInfo : IEquatable<FilterItemInfo>, SetVisibleFilterAction.IFilterItemInfo
    {
        public string groupKey { get; set; }
        public string filterKey { get; set; }
        public bool visible { get; set; }

        public void SetProperties(string _goupKey, string _filterKey, bool _visible)
        {
            groupKey = _goupKey;
            filterKey = _filterKey;
            visible = _visible;
        }

        public static bool operator ==(FilterItemInfo a, FilterItemInfo b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FilterItemInfo a, FilterItemInfo b)
        {
            return !(a == b);
        }

        public bool Equals(FilterItemInfo other)
        {
            return groupKey == other.groupKey &&
                filterKey == other.filterKey &&
                visible == other.visible;
        }

        public override bool Equals(object obj)
        {
            return obj is FilterItemInfo other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (groupKey != null ? groupKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (filterKey != null ? filterKey.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ visible.GetHashCode();
                return hashCode;
            }
        }
    }

    [Serializable]
    public struct ProjectListSortData : IEquatable<ProjectListSortData>, SetProjectSortMethodAction.IProjectSortListData
    {
        public ProjectSortField sortField;
        public ProjectSortMethod method;

        public static bool operator ==(ProjectListSortData a, ProjectListSortData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(ProjectListSortData a, ProjectListSortData b)
        {
            return !(a == b);
        }

        public bool Equals(ProjectListSortData other)
        {
            return sortField == other.sortField && method == other.method;
        }

        public override bool Equals(object obj)
        {
            return obj is ProjectListSortData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)sortField * 397) ^ (int)method;
            }
        }
    }

    public enum ProjectSortField
    {
        SortByDate = 0,
        SortByName = 1,
        SortByServer = 2,
        SortByOrganization = 3,
        SortByCollaborators = 4
    }

    [Serializable]
    public enum ProjectSortMethod
    {
        Ascending = 0,
        Descending = 1
    }

    [Serializable]
    public class MetadataGroupFilter : IEquatable<MetadataGroupFilter>, SetVisibleFilterAction.IFilterItemInfo
    {
        public string groupKey { get; set; }
        public string filterKey { get; set; }
        public bool visible { get; set; }

        public void SetProperties(string _goupKey, string _filterKey, bool _visible)
        {
            groupKey = _goupKey;
            filterKey = _filterKey;
            visible = _visible;
        }

        public MetadataGroupFilter(string groupKey, string filterKey, bool isVisible)
        {
            this.groupKey = groupKey;
            this.filterKey = filterKey;
            visible = isVisible;
        }

        public bool Equals(MetadataGroupFilter other)
        {
            return groupKey == other.groupKey && filterKey == other.filterKey && visible == other.visible;
        }
    }

    [Serializable, GeneratePropertyBag]
    public struct UIProjectStateData : IEquatable<UIProjectStateData>, IProjectSortDataProvider, IObjectSelectorDataProvider, ITeleportDataProvider, IProjectBound
    {
        [field: NonSerialized, DontCreateProperty]
        public Dictionary<string, string> queryArgs { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetProjectSortMethodAction.IProjectSortListData projectSortData { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Bounds rootBounds { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Bounds zoneBounds { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public List<string> filterGroupList { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string filterGroup { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IHighlightFilterInfo highlightFilter { get; set; }

        [CreateProperty]
        [field: SerializeReference, DontCreateProperty]
        public List<SetVisibleFilterAction.IFilterItemInfo> filterItemInfos { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string filterSearchString { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string bimSearchString { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SetVisibleFilterAction.IFilterItemInfo lastChangedFilterItem { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SelectObjectAction.IObjectSelectionInfo objectSelectionInfo { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IPicker objectPicker { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public IPicker teleportPicker { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Vector3 teleportTarget { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ICameraTransformInfo cameraTransformInfo { get; set; }

        public override string ToString()
        {
            return ToString("(Project {0} Bounds {1})");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object)this.rootBounds);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UIProjectStateData))
                return false;
            return Equals((UIProjectStateData)obj);
        }

        public bool Equals(UIProjectStateData other)
        {
            bool isEqual = true;
            if (projectSortData != null)
                isEqual &= projectSortData.Equals(other.projectSortData);
            isEqual &= rootBounds.Equals(other.rootBounds);
            isEqual &= Equals(filterGroupList, other.filterGroupList);
            isEqual &= highlightFilter?.Equals(other.highlightFilter) ?? other.highlightFilter == null;
            isEqual &= Equals(filterItemInfos, other.filterItemInfos);
            isEqual &= filterSearchString == other.filterSearchString;
            isEqual &= bimSearchString == other.bimSearchString;
            isEqual &= lastChangedFilterItem != null ? Equals(lastChangedFilterItem, other.lastChangedFilterItem) : other.lastChangedFilterItem == null;
            isEqual &= objectSelectionInfo != null ? Equals(objectSelectionInfo, other.objectSelectionInfo) : other.objectSelectionInfo == null;
            isEqual &= objectPicker != null ? Equals(objectPicker, other.objectPicker) : other.objectPicker == null;
            isEqual &= teleportPicker != null ? Equals(teleportPicker, other.teleportPicker) : other.teleportPicker == null;
            isEqual &= teleportTarget == other.teleportTarget;
            isEqual &= filterGroup == other.filterGroup;

            return isEqual;
        }

        public static bool operator ==(UIProjectStateData a, UIProjectStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(UIProjectStateData a, UIProjectStateData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = projectSortData.GetHashCode();
                hashCode = (hashCode * 397) ^ rootBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ zoneBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ (filterGroupList != null ? filterGroupList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ highlightFilter.GetHashCode();
                hashCode = (hashCode * 397) ^ (filterItemInfos != null ? filterItemInfos.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (filterSearchString != null ? filterSearchString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (bimSearchString != null ? bimSearchString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ lastChangedFilterItem.GetHashCode();
                hashCode = (hashCode * 397) ^ objectSelectionInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ (objectPicker != null ? objectPicker.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (teleportPicker != null ? teleportPicker.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ teleportTarget.GetHashCode();
                hashCode = (hashCode * 397) ^ filterGroup.GetHashCode();
                hashCode = (hashCode * 397) ^ cameraTransformInfo.GetHashCode();
                return hashCode;
            }
        }
    }
}
