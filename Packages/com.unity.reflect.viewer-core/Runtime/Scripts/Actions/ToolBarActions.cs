using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetActiveToolBarAction : ActionBase
    {
        public enum ToolbarType
        {
            OrbitSidebar = 0,
            FlySidebar,
            WalkSidebar,
            ARSidebar,
            ARModelAlignSidebar,
            ARInstructionSidebar,
            ARScaleDial,
            TimeOfDayYearDial,
            AltitudeAzimuthDial,
            VRSidebar,
            TopSidebar,
            NavigationSidebar,
            NoSidebar,
            LandingScreen
        }

        public object Data { get; }

        SetActiveToolBarAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToolbarType) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolBarDataProvider.activeToolbar), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetActiveToolBarAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current;
        }
    }

    public class ResetToolBarAction : ActionBase
    {
        public object Data { get; }

        ResetToolBarAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolBarDataProvider.activeToolbar), SetActiveToolBarAction.ToolbarType.OrbitSidebar);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolBarDataProvider.toolbarsEnabled), true);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolStateDataProvider.activeTool), SetActiveToolAction.ToolType.OrbitTool);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IToolStateDataProvider.orbitType), SetOrbitTypeAction.OrbitType.OrbitAtPoint);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ResetToolBarAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == UIStateContext.current && context == ToolStateContext.current;
        }
    }
}
