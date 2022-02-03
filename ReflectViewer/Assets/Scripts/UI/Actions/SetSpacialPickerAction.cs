using System;
using SharpFlux;
using Unity.Properties;
using Unity.Reflect.Viewer;
using UnityEngine;

namespace UnityEngine.Reflect.Viewer.Core.Actions
{
    /// <summary>
    /// This Action has been specialized for the Reflect Viewer
    /// </summary>
    public class SetSpatialSelectorAction: SetObjectSelectorAction
    {
        public object Data { get; }

        SetSpatialSelectorAction(object data) : base(data)
        {
            Data = data;
        }

        Transform m_RootNode = null;

        public override void ApplyPayload<T>(object viewerActionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (SpatialSelector)viewerActionData;
            object boxed = stateData;
            var hasChanged = false;

            var prefPropertyName = nameof(IPipelineDataProvider.rootNode);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)))
            {
                m_RootNode = PropertyContainer.GetValue<Transform>(ref boxed, prefPropertyName);
            }

            prefPropertyName = nameof(IObjectSelectorDataProvider.objectPicker);
            if (PropertyContainer.IsPathValid(ref stateData, new PropertyPath(prefPropertyName)) && m_RootNode != null)
            {
                var oldValue = PropertyContainer.GetValue<IPicker>(ref boxed, prefPropertyName);
                var newValue = data;

                var reflect = Object.FindObjectOfType<ViewerReflectBootstrapper>(true);
                newValue.SpatialPicker = reflect.ViewerBridge;
                newValue.SpatialPickerAsync = reflect.ViewerBridge;

                newValue.WorldRoot = m_RootNode;
                m_RootNode = null;

                hasChanged |= SetPropertyValue(ref stateData, prefPropertyName, newValue, oldValue);
            }
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SetSpatialSelectorAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == ProjectContext.current || context == PipelineContext.current;
        }
    }
}
