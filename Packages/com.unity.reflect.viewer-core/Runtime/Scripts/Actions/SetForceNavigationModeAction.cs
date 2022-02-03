using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetForceNavigationModeAction : ActionBase
    {
        [Serializable, GeneratePropertyBag]
        public struct ForceNavigationModeTrigger
        {
            public bool trigger;
            public int mode;

            public ForceNavigationModeTrigger(int mode)
            {
                trigger = true;
                this.mode = mode;
            }
        }

        public object Data { get; }

        SetForceNavigationModeAction(object data)
        {
            Data = data;
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetForceNavigationModeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ForceNavigationModeContext.current;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ForceNavigationModeTrigger)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IForceNavigationModeDataProvider.navigationMode), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }
    }
}
