using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetInstructionUIStateAction: ActionBase
    {
        public enum InstructionUIState
        {
            Init = 0,
            Started,
            Completed,
            None
        };

        public object Data { get; }

        SetInstructionUIStateAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var instructionUIState = (InstructionUIState)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefPropertyName = nameof(IARModeDataProvider.instructionUIState);

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, instructionUIState);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetInstructionUIStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARContext.current;
        }
    }
}
