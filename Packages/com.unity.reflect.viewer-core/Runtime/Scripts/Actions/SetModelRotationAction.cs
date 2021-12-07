using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetModelRotationAction: ActionBase
    {
        public object Data { get; }

        SetModelRotationAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var modelRotation = (Vector3)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefPropertyName = nameof(IARPlacementDataProvider.placementRoot);

            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
                var newValue = oldValue;
                newValue.Rotate(modelRotation);
                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetModelRotationAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current || context == ARContext.current;
        }
    }
}
