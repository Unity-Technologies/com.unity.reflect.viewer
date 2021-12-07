using System;
using System.Collections.Generic;
using SharpFlux;
using Unity.Properties;
using Unity.TouchFramework;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public interface IPlacementValidation
    {
        bool IsValid(GameObject firstSelectedPlane, GameObject secondSelectedPlane, out ModalPopup.ModalPopupData errorMessage, GameObject currentSelectedObject = null);
    }

    public class SetARPlacementRuleAction : ActionBase
    {
        public object Data { get; }

        SetARPlacementRuleAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var placementRule = (SetModelFloorAction.PlacementRule)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;
            var prefPropertyName = "placementRule";

            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<SetModelFloorAction.PlacementRule>(ref boxed, prefPropertyName);
                PropertyContainer.SetValue(ref stateData, prefPropertyName, placementRule);
                boxed = stateData;
                var newValue = PropertyContainer.GetValue<SetModelFloorAction.PlacementRule>(ref boxed, prefPropertyName);
                if (!EqualityComparer<object>.Default.Equals(newValue, oldValue))
                    hasChanged = true;
            }

            prefPropertyName = nameof(IARPlacementDataProvider.placementRulesGameObject);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                var oldValue = PropertyContainer.GetValue<GameObject>(ref boxed, prefPropertyName);
                if (oldValue != null)
                {
                    MonoBehaviour.Destroy(oldValue);
                }

                var rule = PropertyContainer.GetValue<SetModelFloorAction.PlacementRule>(ref boxed, "placementRule");
                GameObject newPlacementGO = null;

                if (rule != SetModelFloorAction.PlacementRule.None)
                {
                    var placementPrefabs =
                        PropertyContainer.GetValue<List<GameObject>>(ref boxed, "placementRulesPrefabs");
                    var boundingBoxRoot = PropertyContainer.GetValue<Transform>(ref boxed, nameof(IARPlacementDataProvider.boundingBoxRootNode));
                    newPlacementGO = MonoBehaviour.Instantiate(placementPrefabs[(int)rule - 1], boundingBoxRoot);
                }

                PropertyContainer.SetValue(ref stateData, prefPropertyName, newPlacementGO);
                boxed = stateData;
                var newValue = PropertyContainer.GetValue<GameObject>(ref boxed, prefPropertyName);
                if (!EqualityComparer<object>.Default.Equals(newValue, oldValue))
                    hasChanged = true;
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetARPlacementRuleAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current || context == PipelineContext.current;
        }
    }

    public class ShowModelAction: ActionBase
    {
        public object Data { get; }

        ShowModelAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.showModel), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ShowModelAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current;
        }
    }
    public class ShowBoundingBoxModelAction: ActionBase
         {
             public object Data { get; }

             ShowBoundingBoxModelAction(object data)
             {
                 Data = data;
             }

             public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
             {
                 var data = (bool)viewerActionData;
                 object boxed = stateData;
                 var hasChanged = false;

                 hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IARPlacementDataProvider.showBoundingBoxModelAction), data);
                 if (hasChanged)
                    onStateDataChanged?.Invoke();
             }

             public static Payload<IViewerAction> From(object data)
                 => Payload<IViewerAction>.From(new ShowBoundingBoxModelAction(data), data);

             public override bool RequiresContext(IUIContext context, object viewerActionData)
             {
                 return context == ARPlacementContext.current;
             }
         }
}
