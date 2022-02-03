using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetProgressStateAction: ActionBase
    {
        public enum ProgressState
        {
            NoPendingRequest,
            PendingIndeterminate,
            PendingDeterminate,
        }

        public object Data { get; }

        SetProgressStateAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ProgressState)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProgressDataProvider.progressState), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetProgressStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProgressContext.current;
        }
    }
}
