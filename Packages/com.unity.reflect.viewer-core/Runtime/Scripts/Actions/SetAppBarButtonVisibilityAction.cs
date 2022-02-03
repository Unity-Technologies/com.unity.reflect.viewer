using System;
using SharpFlux;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public interface IButtonVisibility
    {
        public int type { get; }
        public bool visible { get; }
    }

    public class SetAppBarButtonVisibilityAction : ActionBase
    {
        public object Data { get; }

        SetAppBarButtonVisibilityAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetAppBarButtonVisibilityAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == AppBarContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IButtonVisibility)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IAppBarDataProvider.buttonVisibility), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
