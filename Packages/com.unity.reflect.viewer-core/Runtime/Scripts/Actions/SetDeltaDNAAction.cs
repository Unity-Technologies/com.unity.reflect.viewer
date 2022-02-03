using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetDeltaDNAButtonAction : ActionBase
    {
        public object Data { get; }

        SetDeltaDNAButtonAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetDeltaDNAButtonAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == DeltaDNAContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (string)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDeltaDNADataProvider.buttonName), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
