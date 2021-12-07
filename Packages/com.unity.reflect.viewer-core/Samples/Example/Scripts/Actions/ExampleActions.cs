using System;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEngine.Reflect.Viewer.Example
{
    class DefaultAction : ActionBase
    {
        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ExampleContext.current;
        }
    }

    class DefaultActionOverride : ActionBase
    {
        public override void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool) actionData;
            object boxed = stateData;

            if (SetPropertyValue(ref stateData, ref boxed, nameof(IStateFlagData.stateFlag), data))
                onStateDataChanged?.Invoke();
        }

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ExampleContext.current;
        }
    }

    class StateDataActionOverride : ActionBase<IStateFlagData>
    {
        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ExampleContext.current;
        }

        protected override void ApplyPayloadToState(object actionData, ref IStateFlagData stateData, out bool hasChanged)
        {
            hasChanged = false;
            bool value = (bool)actionData;

            if (stateData.stateFlag != value)
            {
                stateData.stateFlag = value;
                hasChanged = true;
            }
        }
    }

    class StateDataTActionTDataOverride : ActionBase<IStateFlagData, ExampleStateData>
    {
        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ExampleContext.current;
        }

        protected override void ApplyPayloadToState(IStateFlagData actionData, ref ExampleStateData stateData, out bool hasChanged)
        {
            hasChanged = false;

            if (stateData.stateFlag != actionData.stateFlag)
            {
                stateData.stateFlag = actionData.stateFlag;
                hasChanged = true;
            }
        }
    }

    class SetTextDataAction : ReflectAction<string, IStateTextData, ExampleContext>
    {
        protected override void ApplyPayloadToState(string actionData, ref IStateTextData stateData, out bool hasChanged)
        {
            hasChanged = false;

            if (stateData.stateText != actionData)
            {
                stateData.stateText = actionData;
                hasChanged = true;
            }
        }
    }
}
