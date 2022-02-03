using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public interface IWalkInstructionUI : IInstructionUIIterable, IInstructionUICancelable
    {
        void Reset(Vector3 offset);
    }

    public class SetWalkInstructionStateAction : ActionBase
    {
        public object Data { get; }

        SetWalkInstructionStateAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetWalkInstructionStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == PipelineContext.current || context == WalkModeContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IWalkModeDataProvider.instructionUIState), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
