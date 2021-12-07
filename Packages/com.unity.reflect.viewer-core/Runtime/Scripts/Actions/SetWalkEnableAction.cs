using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetWalkEnableAction : ActionBase
    {
        public object Data { get; }

        SetWalkEnableAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetWalkEnableAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == PipelineContext.current || context == WalkModeContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IWalkModeDataProvider.walkEnabled), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }

    public class SetWalkStateAction : ActionBase
    {
        public object Data { get; }

        SetWalkStateAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetWalkStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == WalkModeContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetInstructionUIStateAction.InstructionUIState)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IWalkModeDataProvider.instructionUIState), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
