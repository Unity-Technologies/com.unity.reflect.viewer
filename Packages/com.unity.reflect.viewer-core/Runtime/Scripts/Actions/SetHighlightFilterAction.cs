using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetHighlightFilterAction : ActionBase
    {
        public object Data { get; }

        public SetHighlightFilterAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IHighlightFilterInfo) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            var prefPropertyName = nameof(IProjectSortDataProvider.highlightFilter);
            var filterGroupPropertyName = nameof(IProjectSortDataProvider.filterGroup);

            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName))
                && PropertyContainer.IsPathValid(ref stateData, new PropertyPath(filterGroupPropertyName)))
            {
                var filterGroupValue = PropertyContainer.GetValue<string>(ref boxed, filterGroupPropertyName);
                var oldValue = PropertyContainer.GetValue<IHighlightFilterInfo>(ref boxed, prefPropertyName);
                var newValue = data;

                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetHighlightFilterAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current || context == PipelineContext.current;
        }
    }
}
