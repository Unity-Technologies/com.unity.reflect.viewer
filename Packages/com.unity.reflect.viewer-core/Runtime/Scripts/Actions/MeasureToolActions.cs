using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class ToggleMeasureToolAction: ActionBase
    {
        public enum AnchorType
        {
            Point = 0,
            Edge = 1,
            Plane = 2,
            Object = 3
        };

        public enum MeasureMode: int
        {
            RawDistance = 0,
            PerpendicularDistance = 1,
        }

        public enum MeasureFormat: int
        {
            Meters = 0,
            Feets = 1,
        }

        public struct ToggleMeasureToolData
        {
            public bool toolState;
            public AnchorType selectionType;
            public MeasureMode measureMode;
            public MeasureFormat measureFormat;

            public static readonly ToggleMeasureToolData defaultData = new ToggleMeasureToolData()
            {
                toolState = false,
                selectionType = AnchorType.Point,
                measureMode = MeasureMode.RawDistance,
                measureFormat = MeasureFormat.Meters,
            };
        }

        public object Data { get; }

        ToggleMeasureToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToggleMeasureToolData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.toolState), data.toolState);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.selectionType), data.selectionType);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.measureMode), data.measureMode);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.measureFormat), data.measureFormat);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ToggleMeasureToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MeasureToolContext.current;
        }
    }

    public class ChangeSelectionTypeMeasureToolAction: ActionBase
    {
        public object Data { get; }

        ChangeSelectionTypeMeasureToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToggleMeasureToolAction.AnchorType)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.selectionType), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ChangeSelectionTypeMeasureToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MeasureToolContext.current;
        }
    }

    public class ChangeMeasureFormatMeasureToolAction: ActionBase
    {
        public object Data { get; }

        ChangeMeasureFormatMeasureToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToggleMeasureToolAction.MeasureFormat)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.measureFormat), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ChangeMeasureFormatMeasureToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MeasureToolContext.current;
        }
    }

    public class ChangeMeasureModeMeasureToolAction: ActionBase
    {
        public object Data { get; }

        ChangeMeasureModeMeasureToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToggleMeasureToolAction.MeasureMode)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.measureMode), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ChangeMeasureModeMeasureToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MeasureToolContext.current;
        }
    }

    public class SelectObjectMeasureToolAction: ActionBase
    {
        public interface IAnchor
        {
            int objectId { get; }
            ToggleMeasureToolAction.AnchorType type { get; }
            Vector3 position { get; }
            Vector3 normal { get; }
        }

        public object Data { get; }

        SelectObjectMeasureToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IAnchor)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IMeasureToolDataProvider.selectedAnchor), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SelectObjectMeasureToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MeasureToolContext.current;
        }
    }
}
