using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetARModeAction: ActionBase
    {
        public enum ARMode
        {
            ViewBased,
            WallBased,
            TableTop,
            MarkerBased,
            None
        }

        public object Data { get; }

        SetARModeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var arMode = (ARMode)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefPropertyName = nameof(IARModeDataProvider.arMode);

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, arMode);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARModeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARContext.current;
        }
    }
}
