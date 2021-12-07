using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetLandingScreenFilterProjectServerAction: ActionBase
    {
        [Flags]
        public enum ProjectServerType
        {
            None = 0,
            Local = 1 << 0,
            Network = 1 << 1,
            Cloud = 1 << 2,
            All = (Cloud << 1) - 1
        }

        public object Data { get; }

        SetLandingScreenFilterProjectServerAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ProjectServerType) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectListFilterDataProvider.projectServerType), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLandingScreenFilterProjectServerAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == LandingScreenContext.current;
        }
    }

    public class SetLandingScreenFilterSearchStringAction: ActionBase
    {
        public object Data { get; }

        SetLandingScreenFilterSearchStringAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string) viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectListFilterDataProvider.searchString), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetLandingScreenFilterSearchStringAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == LandingScreenContext.current;
        }
    }
}
