using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class TeleportAction: ActionBase
    {
        public object Data { get; }

        public TeleportAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (Vector3)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ITeleportDataProvider.teleportTarget), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new TeleportAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }

    public class SetFinishTeleportAction: ActionBase
    {
        public object Data { get; }

        public SetFinishTeleportAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IWalkModeDataProvider.isTeleportFinish), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetFinishTeleportAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == WalkModeContext.current;
        }
    }
}
