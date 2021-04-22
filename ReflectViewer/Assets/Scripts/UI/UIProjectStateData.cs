using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer.Pipeline;

namespace Unity.Reflect.Viewer.UI
{
    public struct FilterItemInfo : IEquatable<FilterItemInfo>
    {
        public string groupKey;
        public string filterKey;
        public bool visible;
        public bool highlight;

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
                visible == other.visible &&
                highlight == other.highlight;
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
                hashCode = (hashCode * 397) ^ highlight.GetHashCode();
                return hashCode;
            }
        }
    }
    public struct HighlightFilterInfo : IEquatable<HighlightFilterInfo>
    {
        public string groupKey;
        public string filterKey;

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
                return ((groupKey != null ? groupKey.GetHashCode() : 0) * 397) ^ (filterKey != null ? filterKey.GetHashCode() : 0);
            }
        }
    }

    [Serializable]
    public struct ObjectSelectionInfo : IEquatable<ObjectSelectionInfo>
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
    public struct CameraTransformInfo : IEquatable<CameraTransformInfo>
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
    public struct ProjectListSortData : IEquatable<ProjectListSortData>
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

    [Serializable]
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
    public struct UIProjectStateData : IEquatable<UIProjectStateData>
    {
        [NonSerialized]
        public Project activeProject;
        [NonSerialized]
        public Sprite activeProjectThumbnail;
        public ProjectListSortData projectSortData;

        public Bounds rootBounds;
        public List<string> filterGroupList;
        public HighlightFilterInfo highlightFilter;
        public List<FilterItemInfo> filterItemInfos;
        public string filterSearchString;
        public string bimSearchString;
        public FilterItemInfo lastChangedFilterItem;
        public ObjectSelectionInfo objectSelectionInfo;
        public ISpatialPicker<Tuple<GameObject, RaycastHit>> objectPicker;
        public ISpatialPicker<Tuple<GameObject, RaycastHit>> teleportPicker;
        public Vector3? teleportTarget;
        public CameraTransformInfo cameraTransformInfo;

        public override string ToString()
        {
            return ToString("(Project {0} Bounds {1})");
        }

        public string ToString(string format)
        {
            return string.Format(format,
                (object)this.activeProject,
                (object)this.rootBounds);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is UIProjectStateData))
                return false;
            return this.Equals((UIProjectStateData) obj);
        }

        public bool Equals(UIProjectStateData other)
        {
            return Equals(activeProject, other.activeProject) &&
                projectSortData.Equals(other.projectSortData) &&
                rootBounds.Equals(other.rootBounds) &&
                Equals(filterGroupList, other.filterGroupList) &&
                highlightFilter.Equals(other.highlightFilter) &&
                Equals(filterItemInfos, other.filterItemInfos) &&
                filterSearchString == other.filterSearchString &&
                bimSearchString == other.bimSearchString &&
                lastChangedFilterItem.Equals(other.lastChangedFilterItem) &&
                objectSelectionInfo.Equals(other.objectSelectionInfo) &&
                objectPicker == other.objectPicker &&
                teleportPicker == other.teleportPicker &&
                teleportTarget == other.teleportTarget &&
                cameraTransformInfo == other.cameraTransformInfo;
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
                var hashCode = (activeProject != null ? activeProject.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ projectSortData.GetHashCode();
                hashCode = (hashCode * 397) ^ rootBounds.GetHashCode();
                hashCode = (hashCode * 397) ^ (filterGroupList != null ? filterGroupList.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ highlightFilter.GetHashCode();
                hashCode = (hashCode * 397) ^ (filterItemInfos != null ? filterItemInfos.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (filterSearchString != null ? filterSearchString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (bimSearchString != null ? bimSearchString.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ lastChangedFilterItem.GetHashCode();
                hashCode = (hashCode * 397) ^ objectSelectionInfo.GetHashCode();
                hashCode = (hashCode * 397) ^ (objectPicker != null ? objectPicker.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (teleportPicker != null ? teleportPicker.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (teleportTarget.HasValue ? teleportTarget.Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ cameraTransformInfo.GetHashCode();
                return hashCode;
            }
        }
    }
}
