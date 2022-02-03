using System;
using System.Collections.Generic;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.Viewer.Core
{
    public interface IProjectSortDataProvider
    {
        public SetProjectSortMethodAction.IProjectSortListData projectSortData { get; set; }
        public SetVisibleFilterAction.IFilterItemInfo lastChangedFilterItem { get; set; }
        public string filterSearchString { get; set; }
        public string filterGroup { get; set; }
        public List<string> filterGroupList { get; set; }
        public List<SetVisibleFilterAction.IFilterItemInfo> filterItemInfos { get; set; }
        public string bimSearchString { get; set; }
        public IHighlightFilterInfo highlightFilter { get; set; }
    }

    public interface IHighlightFilterInfo
    {
        public string groupKey{ get; set; }
        public string filterKey{ get; set; }
        public bool IsValid { get; set; }
    }

    public interface IProjectBound
    {
        Bounds zoneBounds{ get; set; }
        Bounds rootBounds { get; set; }
    }

    public interface IObjectSelectorDataProvider
    {
        public IPicker objectPicker { get; set; }
        public SelectObjectAction.IObjectSelectionInfo objectSelectionInfo { get; set; }
    }
    public interface ICameraTransformInfo
    {}

    public interface ITeleportDataProvider
    {
        public Vector3 teleportTarget { get; set; }
        public IPicker teleportPicker { get; set; }
        public ICameraTransformInfo cameraTransformInfo { get; set; }
    }

    public class ProjectContext : ContextBase<ProjectContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IProjectSortDataProvider), typeof(IObjectSelectorDataProvider), typeof(ITeleportDataProvider)};
    }
}
