using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core
{
    public class SetDebugOptionsAction: ActionBase
    {
        public object Data { get; }

        SetDebugOptionsAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetDebugOptionsAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ApplicationContext.current || context == DebugOptionContext.current;
        }
    }
}
