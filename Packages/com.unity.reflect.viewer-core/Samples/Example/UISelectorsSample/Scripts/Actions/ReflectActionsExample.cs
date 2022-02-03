using System;
using UnityEngine.Reflect.Viewer.Core;

namespace UnityEngine.Reflect.Viewer.Example.ReflectActions
{
    class CopyAllPropertiesReflectAction : ReflectAction<ExampleContext>
    {
    }

    class CustomReflectAction : ReflectAction<ExampleContext>
    {
        public override void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool) actionData;
            object boxed = stateData;

            if (SetPropertyValue(ref stateData, ref boxed, nameof(IStateFlagData.stateFlag), data))
                onStateDataChanged?.Invoke();
        }
    }

    class ReflectAction_StateDataT : ReflectAction<IStateFlagData, ExampleContext>
    {
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

    class ReflectAction_ActionDataT_StateDataT : ReflectAction<IStateFlagData, ExampleStateData, ExampleContext>
    {
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

    class SetTextDataPropertyReflectAction : ReflectSetPropertyAction<ExampleContext>
    {
        protected override string GetTargetPropertyName()
        {
            return nameof(IStateTextData.stateText);
        }
    }
}
