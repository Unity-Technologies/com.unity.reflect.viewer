using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetActiveToolAction : ActionBase
    {
        public enum ToolType
        {
            None = 0,
            SelectTool = 1,
            OrbitTool = 2,
            ZoomTool = 3,
            PanTool = 4,
            ClippingTool = 5,
            SunstudyTool = 6
        }

        public object Data { get; }

        SetActiveToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToolType)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolStateDataProvider.activeTool), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetActiveToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ToolStateContext.current;
        }
    }

    public class SetOrbitTypeAction : ActionBase
    {
        public enum OrbitType
        {
            None = -1,
            WorldOrbit = 0,
            OrbitAtSelection = 1,
            OrbitAtPoint = 2,
        }

        public object Data { get; }

        SetOrbitTypeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (OrbitType)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolStateDataProvider.orbitType), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetOrbitTypeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ToolStateContext.current;
        }
    }

    public class SetInfoTypeAction : ActionBase
    {
        public enum InfoType
        {
            Info = 0,
            Debug = 1,
        }

        public object Data { get; }

        SetInfoTypeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (InfoType)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolStateDataProvider.infoType), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetInfoTypeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ToolStateContext.current;
        }
    }

    public class SetClippingToolAction : ActionBase
    {
        public enum ClippingTool
        {
            AddXPlane = 0,
            AddYPlane = 1,
            AddZPlane = 2,
        }

        public object Data { get; }

        SetClippingToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ClippingTool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolStateDataProvider.clippingTool), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetClippingToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ToolStateContext.current;
        }
    }
}
