using System;
using SharpFlux;
using UnityEngine.Reflect;
using UnityEngine.Reflect.Viewer;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace Unity.Reflect.Viewer.UI
{
    public class DownloadProjectAction : ActionBase
    {
        public object Data { get; }

        DownloadProjectAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (Project)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProgressDataProvider.progressState), SetProgressStateAction.ProgressState.PendingIndeterminate);
            ReflectProjectsManager.DownloadProject(data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new DownloadProjectAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProgressContext.current;
        }
    }

    public class RemoveProjectAction : ActionBase
    {
        public object Data { get; }

        RemoveProjectAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (Project)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IProgressDataProvider.progressState), SetProgressStateAction.ProgressState.PendingIndeterminate);
            ReflectProjectsManager.DeleteProjectLocally(data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new RemoveProjectAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProgressContext.current;
        }
    }
}
