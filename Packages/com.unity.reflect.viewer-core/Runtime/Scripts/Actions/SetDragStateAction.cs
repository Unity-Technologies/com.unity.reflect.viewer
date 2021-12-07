using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetDragStateAction: ActionBase
    {
        public object Data { get; }

        SetDragStateAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetDragStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == DragStateContext.current;
        }
    }
}
