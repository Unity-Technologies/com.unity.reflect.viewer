using System;
using System.Collections.Generic;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetProjectSortMethodAction : ActionBase
    {
        public interface IProjectSortListData { }

        public object Data { get; }

        SetProjectSortMethodAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IProjectSortListData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectSortDataProvider.projectSortData), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetProjectSortMethodAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }

    public class SetFilterSearchAction : ActionBase
    {
        public object Data { get; }

        SetFilterSearchAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectSortDataProvider.filterSearchString), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetFilterSearchAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }

    public class SetBimSearchAction : ActionBase
    {
        public object Data { get; }

        SetBimSearchAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectSortDataProvider.bimSearchString), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetBimSearchAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }

    public class SetVisibleFilterAction : ActionBase
    {
        public interface IFilterItemInfo
        {
            public string groupKey { get; set; }
            public string filterKey { get; set; }
            public bool visible { get; set; }

            public void SetProperties(string _goupKey, string _filterKey, bool _visible);
        }

        public object Data { get; }

        SetVisibleFilterAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IFilterItemInfo)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectSortDataProvider.lastChangedFilterItem), data);

            var prefPropertyName = "filterItemInfos";
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<IList<IFilterItemInfo>>(ref boxed, prefPropertyName);

                for (int i = 0; i < oldValue.Count; i++)
                {
                    if (oldValue[i].groupKey == data.groupKey &&
                        oldValue[i].filterKey == data.filterKey &&
                        oldValue[i].visible != data.visible)
                    {
                        oldValue[i].visible = data.visible;
                        hasChanged = true;
                    }
                }

                SetPropertyValue(ref stateData, ref boxed, prefPropertyName, oldValue);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetVisibleFilterAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }
}
