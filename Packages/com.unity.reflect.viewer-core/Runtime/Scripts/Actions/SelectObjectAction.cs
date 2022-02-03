using System;
using System.Collections.Generic;
using System.Linq;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SelectObjectAction: ActionBase
    {
        public interface IObjectSelectionInfo
        {
        }

        public object Data { get; }

        public SelectObjectAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IObjectSelectionInfo)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IObjectSelectorDataProvider.objectSelectionInfo), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SelectObjectAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current;
        }
    }
}
