using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetModelScaleAction: ActionBase
    {
        public enum ArchitectureScale
        {
            OneToFiveThousand = 5000,
            OneToOneThousand = 1000,
            OneToFiveHundred = 500,
            OneToFourHundred = 400,
            OneToThreeHundred = 300,
            OneToTwoHundred = 200,
            OneToOneHundred = 100,
            OneToFifty = 50,
            OneToTwenty = 20,
            OneToTen = 10,
            OneToFive = 5,
            OneToOne = 1,
        }

        public object Data { get; }

        SetModelScaleAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var modelScale = (ArchitectureScale)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefPropertyName = nameof(IARPlacementDataProvider.modelScale);

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, modelScale);

            prefPropertyName = nameof(IARPlacementDataProvider.placementRoot);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
                var scalef = (float)modelScale;
                var newValue = oldValue;
                newValue.localScale = new Vector3(1.0f / scalef, 1.0f / scalef, 1.0f / scalef);
                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetModelScaleAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current || context == ARContext.current;;
        }
    }
}
