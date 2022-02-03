using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{

    public class SetWalkInstructionUIAction : ActionBase
    {
        public object Data { get; }

        SetWalkInstructionUIAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetWalkInstructionUIAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == PipelineContext.current || context == WalkModeContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IWalkModeDataProvider.instruction), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
