using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetFilterGroupAction : ActionBase
    {
        public object Data { get; }

        SetFilterGroupAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            var prefPropertyName = nameof(IProjectSortDataProvider.filterGroup);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetFilterGroupAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current || context == PipelineContext.current;
        }
    }
}
