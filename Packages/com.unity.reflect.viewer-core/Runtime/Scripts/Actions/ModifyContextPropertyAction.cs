using System;
using System.Collections.Generic;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public struct ModifyContextPropertyActionData
    {
        public IUIContext context;
        public string propertyName;
        public object propertyValue;
    }

    public class ModifyContextPropertyAction: ActionBase
    {
        public object Data { get; }

        ModifyContextPropertyAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ModifyContextPropertyAction(data), data);

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var modifyContextPropertyActionData = (ModifyContextPropertyActionData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, modifyContextPropertyActionData.propertyName, modifyContextPropertyActionData.propertyValue);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            var modifyContextPropertyActionData = (ModifyContextPropertyActionData)viewerActionData;
            return EqualityComparer<object>.Default.Equals(context, modifyContextPropertyActionData.context);
        }
    }
}
