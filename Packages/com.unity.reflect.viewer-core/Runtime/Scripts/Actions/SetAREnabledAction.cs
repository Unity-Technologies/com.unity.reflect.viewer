using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetAREnabledAction: ActionBase
    {
        public object Data { get; }

        SetAREnabledAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var arEnabled = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            var prefPropertyName = nameof(IARModeDataProvider.arEnabled);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, arEnabled);

            prefPropertyName = nameof(IARPlacementDataProvider.modelScale);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, prefPropertyName, SetModelScaleAction.ArchitectureScale.OneToOne);

            prefPropertyName = nameof(IARPlacementDataProvider.placementRoot);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
                var newValue = oldValue;
                newValue.localScale = Vector3.one;
                newValue.position = Vector3.zero;
                newValue.rotation = Quaternion.identity;

                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }

            prefPropertyName = nameof(IPipelineDataProvider.rootNode);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
                var newValue = oldValue;
                newValue.localPosition = Vector3.one;

                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }

            prefPropertyName = nameof(IARPlacementDataProvider.boundingBoxRootNode);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
                var newValue = oldValue;
                newValue.localPosition = Vector3.one;

                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetAREnabledAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARContext.current || context == PipelineContext.current || context == ARPlacementContext.current;
        }
    }
}
