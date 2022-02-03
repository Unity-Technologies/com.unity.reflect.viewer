using System;
using System.Collections.Generic;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetTimeOfDayAction : ActionBase
    {
        public object Data { get; }

        SetTimeOfDayAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISunstudyDataProvider.timeOfDay), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetTimeOfDayAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SunStudyContext.current;
        }
    }

    public class SetTimeOfYearAction: ActionBase
    {
        public object Data { get; }

        SetTimeOfYearAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISunstudyDataProvider.timeOfYear), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetTimeOfYearAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SunStudyContext.current;
        }
    }

    public class SetUtcAction: ActionBase
    {
        public object Data { get; }

        SetUtcAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISunstudyDataProvider.utcOffset), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetUtcAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SunStudyContext.current;
        }
    }

    public class SetLatitudeAction: ActionBase
    {
        public object Data { get; }

        SetLatitudeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISunstudyDataProvider.latitude), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLatitudeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SunStudyContext.current;
        }
    }

    public class SetLongitudeAction: ActionBase
    {
        public object Data { get; }

        SetLongitudeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISunstudyDataProvider.longitude), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLongitudeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SunStudyContext.current;
        }
    }

    public class SetNorthAngleAction: ActionBase
    {
        public object Data { get; }

        SetNorthAngleAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (int)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISunstudyDataProvider.northAngle), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetNorthAngleAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SunStudyContext.current;
        }
    }
}
