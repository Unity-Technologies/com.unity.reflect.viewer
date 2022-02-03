using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetVREnableAction: ActionBase
    {
        [System.Flags]
        public enum DeviceCapability
        {
            None,
            ARCapability = 0x01,
            VRCapability = 0x02,
            SupportsAsyncGPUReadback = 0x04,
        }

        public object Data { get; }

        SetVREnableAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetVREnableAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == PipelineContext.current || context == VRContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IVREnableDataProvider.VREnable), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }

    public class SetXRControllerAction: ActionBase
    {
        public object Data { get; }

        SetXRControllerAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetXRControllerAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == PipelineContext.current || context == VRContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (Transform)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IVREnableDataProvider.RightController), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
