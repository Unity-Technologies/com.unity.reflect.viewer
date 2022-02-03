using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class OpenProjectActions<TValue> : ActionBase
    {
        public object Data { get; }

        OpenProjectActions(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (TValue)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectDataProvider<TValue>.activeProject), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new OpenProjectActions<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectManagementContext<TValue>.current;
        }
    }

    public class OpenURLActions<TValue> : ActionBase
    {
        public object Data { get; }

        OpenURLActions(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            Application.OpenURL(data);

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectDataProvider<TValue>.url), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new OpenURLActions<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectManagementContext<TValue>.current;
        }
    }

    public class CloseProjectActions<TValue> : ActionBase
    {
        public object Data { get; }

        CloseProjectActions(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (TValue)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectDataProvider<TValue>.activeProject), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new CloseProjectActions<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectManagementContext<TValue>.current;
        }
    }

    public class LoadSceneActions<TValue> : ActionBase
    {
        public object Data { get; }

        LoadSceneActions(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectDataProvider<TValue>.loadSceneName), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new LoadSceneActions<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectManagementContext<TValue>.current;
        }
    }

    public class UnloadSceneActions<TValue> : ActionBase
    {
        public object Data { get; }

        UnloadSceneActions(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProjectDataProvider<TValue>.unloadSceneName), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new UnloadSceneActions<TValue>(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectManagementContext<TValue>.current;
        }
    }
}
