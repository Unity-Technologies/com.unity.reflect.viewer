using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public interface IButtonInteractable
    {
        public int type { get; }
        public bool interactable { get; }
    }

    public class SetAppBarButtonInteractableAction : ActionBase
    {
        public object Data { get; }

        SetAppBarButtonInteractableAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetAppBarButtonInteractableAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == AppBarContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IButtonInteractable)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IAppBarDataProvider.buttonInteractable), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
