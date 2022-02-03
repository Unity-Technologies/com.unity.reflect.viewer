using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetCameraTransformInfoAction: ActionBase
    {
        public object Data { get; }

        public SetCameraTransformInfoAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ICameraTransformInfo)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ITeleportDataProvider.cameraTransformInfo), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetCameraTransformInfoAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }
}
