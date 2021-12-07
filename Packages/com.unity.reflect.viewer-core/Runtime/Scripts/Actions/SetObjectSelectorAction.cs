using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public interface IPicker
    {
    }

    /// <summary>
    /// This class is not implemented as it is: this class is specialised in the Reflect Viewer with Spatial Selection in order to add colliders on meshes at runtime
    /// If you want to implement a simple Raycast Selection if you already have a project with colliders, do it here and feel free to contact us
    /// </summary>
    public class SetObjectSelectorAction: ActionBase
    {
        public object Data { get; }

        public SetObjectSelectorAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetObjectSelectorAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }
}
