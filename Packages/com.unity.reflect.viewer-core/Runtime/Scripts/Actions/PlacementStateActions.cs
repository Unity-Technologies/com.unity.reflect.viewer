using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetModelFloorAction: ActionBase
    {
        public enum PlacementRule
        {
            None = 0,
            FloorPlacementRule = 1,
            TableTopPlacementRule = 2,
            WallPlacementRule = 3,
            MarkerPlacementRule = 4
        }

        public object Data { get; }

        SetModelFloorAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (GameObject)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.modelFloor), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetModelFloorAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetValidTargetAction: ActionBase
    {
        public object Data { get; }

        SetValidTargetAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.validTarget), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetValidTargetAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetFirstPlaneAction: ActionBase
    {
        public object Data { get; }

        SetFirstPlaneAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (GameObject)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.firstSelectedPlane), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetFirstPlaneAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetModelPlacementLocationAction: ActionBase
    {
        public object Data { get; }

        SetModelPlacementLocationAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (Vector3)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.modelPlacementLocation), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetModelPlacementLocationAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetAnchorPointAction: ActionBase
    {
        public struct SetAnchorPointData
        {
            public GameObject secondSelectedPlane;
            public Vector3 modelPlacementLocation;
            public float beamHeight;
        }

        public object Data { get; }

        SetAnchorPointAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetAnchorPointData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.secondSelectedPlane), data.secondSelectedPlane);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.modelPlacementLocation), data.modelPlacementLocation);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.beamHeight), data.beamHeight);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetAnchorPointAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetARFloorAction: ActionBase
    {
        public object Data { get; }

        SetARFloorAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (GameObject)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.arFloor), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARFloorAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetARFirstWallAlignmentAction: ActionBase
    {
        public struct SetARFirstWallData
        {
            public GameObject firstARSelectedPlane;
            public Vector3 arPlacementAlignment;
        }

        public object Data { get; }

        SetARFirstWallAlignmentAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetARFirstWallData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.firstARSelectedPlane), data.firstARSelectedPlane);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.arPlacementAlignment), data.arPlacementAlignment);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARFirstWallAlignmentAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetARSecondPlaneAction: ActionBase
    {
        public object Data { get; }

        SetARSecondPlaneAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (GameObject)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.secondARSelectedPlane), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARSecondPlaneAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }

    public class SetARAnchorPointAction: ActionBase
    {

        public struct SetARAnchorPointData
        {
            public Vector3 arPlacementLocation;
            public float beamHeight;
        }

        public object Data { get; }

        SetARAnchorPointAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetARAnchorPointData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.arPlacementLocation), data.arPlacementLocation);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.beamHeight), data.beamHeight);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARAnchorPointAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }
}
