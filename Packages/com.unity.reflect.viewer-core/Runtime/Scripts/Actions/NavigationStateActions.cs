using System;
using SharpFlux;
using Unity.Properties;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    public class SetNavigationModeAction: ActionBase
    {
        public enum NavigationMode : int
        {
            Orbit = 0,
            Fly = 1,
            Walk = 2,
            AR = 3,
            VR = 4,
        }

        public object Data { get; }
        bool m_UpdateInstructionUIStep = false;

        SetNavigationModeAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var navigationMode = (NavigationMode)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.navigationMode), navigationMode);

            if (hasChanged && navigationMode == NavigationMode.AR)
            {
                m_UpdateInstructionUIStep = true;
            }

            var prefPropertyName = nameof(IARModeDataProvider.instructionUIStep);
            if(PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)) && m_UpdateInstructionUIStep)
            {
                var oldValue = PropertyContainer.GetValue<int>(ref boxed, prefPropertyName);
                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, 0, oldValue);
                m_UpdateInstructionUIStep = false;
            }

            prefPropertyName = nameof(IARPlacementDataProvider.modelScale);
            if(PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)) && navigationMode != NavigationMode.AR)
            {
                var oldValue = PropertyContainer.GetValue<SetModelScaleAction.ArchitectureScale>(ref boxed, prefPropertyName);
                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, SetModelScaleAction.ArchitectureScale.OneToOne, oldValue);

                prefPropertyName = nameof(IARPlacementDataProvider.boundingBoxRootNode);
                if(PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
                {
                    var BBRootNode = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
                    BBRootNode.gameObject.SetActive(true);
                }
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetNavigationModeAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ARPlacementContext.current || context == NavigationContext.current || context == ARContext.current;
        }
    }

    public class SetMoveEnabledAction: ActionBase
    {
        public object Data { get; }
        bool m_UpdateInstructionUIStep = false;

        SetMoveEnabledAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var moveEnabled = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.moveEnabled), moveEnabled);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetMoveEnabledAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == NavigationContext.current;
        }
    }

    public class SetShowScaleReferenceAction: ActionBase
    {
        public object Data { get; }
        bool m_UpdateInstructionUIStep = false;

        SetShowScaleReferenceAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var showScaleReference = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.showScaleReference), showScaleReference);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetShowScaleReferenceAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == NavigationContext.current;
        }
    }

    public class EnableAllNavigationAction: ActionBase
    {
        public object Data { get; }
        bool m_UpdateInstructionUIStep = false;

        EnableAllNavigationAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var enable = (bool)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.freeFlyCameraEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.orbitEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.panEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.zoomEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.moveEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.worldOrbitEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.teleportEnabled), enable);
            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(INavigationDataProvider.gizmoEnabled), enable);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new EnableAllNavigationAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == NavigationContext.current;
        }
    }
}
