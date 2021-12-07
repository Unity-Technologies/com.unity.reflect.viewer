using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetSceneOptionAction: ActionBase
    {
        public object Data { get; }

        SetSceneOptionAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetSceneOptionAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == SceneOptionContext.current;
        }
    }
}
