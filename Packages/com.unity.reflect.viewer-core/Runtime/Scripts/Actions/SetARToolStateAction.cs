using System;
using SharpFlux;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetARToolStateAction: ActionBase
    {
        public class EmptyUIButtonValidator : IUIButtonValidator
        {
            public bool ButtonValidate()
            {
                throw new NotImplementedException();
            }
        }

        public interface IUIButtonValidator
        {
            bool ButtonValidate();
        }

        public struct SetARToolStateData
        {
            public bool selectionEnabled;
            public bool navigationEnabled;
            public bool previousStepEnabled;
            public bool okEnabled;
            public bool cancelEnabled;
            public bool scaleEnabled;
            public bool wallIndicatorsEnabled;
            public bool anchorPointsEnabled;
            public bool arWallIndicatorsEnabled;
            public bool arAnchorPointsEnabled;
            public bool rotateEnabled;
            public bool measureToolEnabled;
            public IUIButtonValidator okButtonValidator;

            public static readonly SetARToolStateData defaultData = new SetARToolStateData()
            {
                selectionEnabled = false,
                navigationEnabled = false,
                previousStepEnabled = false,
                okEnabled = true,
                cancelEnabled = false,
                scaleEnabled = false,
                wallIndicatorsEnabled = false,
                anchorPointsEnabled = false,
                arWallIndicatorsEnabled = false,
                arAnchorPointsEnabled = false,
                rotateEnabled = false,
                measureToolEnabled = false,
                okButtonValidator = new EmptyUIButtonValidator(),
            };
        }

        public object Data { get; }

        SetARToolStateAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SetARToolStateData)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.selectionEnabled), data.selectionEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.navigationEnabled), data.navigationEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.previousStepEnabled), data.previousStepEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.okEnabled), data.okEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.cancelEnabled), data.cancelEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.scaleEnabled), data.scaleEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.wallIndicatorsEnabled), data.wallIndicatorsEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.anchorPointsEnabled), data.anchorPointsEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.arWallIndicatorsEnabled), data.arWallIndicatorsEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.arAnchorPointsEnabled), data.arAnchorPointsEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.rotateEnabled), data.rotateEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.measureToolEnabled), data.measureToolEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARToolStatePropertiesDataProvider.okButtonValidator), data.okButtonValidator);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARToolStateAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARToolStateContext.current;
        }
    }
}
