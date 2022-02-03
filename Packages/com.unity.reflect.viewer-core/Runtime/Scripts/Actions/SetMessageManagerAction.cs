using System;
using SharpFlux;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetStatusMessage : ActionBase
    {
        public object Data { get; }

        SetStatusMessage(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;

            StatusMessageData statusMessageData = new StatusMessageData();
            statusMessageData.text = data;
            statusMessageData.type = StatusMessageType.Info;

            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IStatusMessageData.statusMessageData), statusMessageData);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetStatusMessage(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MessageManagerContext.current;
        }
    }

    public class SetInstructionMode : ActionBase
    {
        public object Data { get; }

        SetInstructionMode(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IStatusMessageData.isInstructionMode), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetInstructionMode(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MessageManagerContext.current;
        }
    }

    public class SetStatusMessageWithType : ActionBase
    {
        public object Data { get; }

        SetStatusMessageWithType(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (StatusMessageData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IStatusMessageData.statusMessageData), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetStatusMessageWithType(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MessageManagerContext.current;
        }
    }

    public class ClearStatusAction : ActionBase
    {
        public object Data { get; }

        ClearStatusAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            SetPropertyValue(ref stateData, ref boxed, nameof(IStatusMessageData.statusMessageData), new StatusMessageData());
            SetPropertyValue(ref stateData, ref boxed, nameof(IStatusMessageData.isClearAll), data);

            onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ClearStatusAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MessageManagerContext.current;
        }
    }
}
