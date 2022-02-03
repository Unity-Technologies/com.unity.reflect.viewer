using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class EnableBimFilterAction : ActionBase
    {
        public object Data { get; }

        EnableBimFilterAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISettingsToolDataProvider.bimFilterEnabled), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new EnableBimFilterAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SettingsToolContext.current;
        }
    }
    public class EnableSceneSettingsAction : ActionBase
    {
        public object Data { get; }

        EnableSceneSettingsAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISettingsToolDataProvider.sceneSettingsEnabled), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new EnableSceneSettingsAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SettingsToolContext.current;
        }
    }

    public class EnableSunStudyAction : ActionBase
    {
        public object Data { get; }

        EnableSunStudyAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISettingsToolDataProvider.sunStudyEnabled), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new EnableSunStudyAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SettingsToolContext.current;
        }
    }

    public class EnableMarkerSettingsAction : ActionBase
    {
        public object Data { get; }

        EnableMarkerSettingsAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(ISettingsToolDataProvider.markerSettingsEnabled), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new EnableMarkerSettingsAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SettingsToolContext.current;
        }
    }
}
