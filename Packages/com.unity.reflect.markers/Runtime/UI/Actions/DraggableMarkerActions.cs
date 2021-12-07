using System;
using System.Collections;
using System.Collections.Generic;
using SharpFlux;
using Unity.Reflect.Markers.UI;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Markers.Placement
{

    public class ToggleMarkerDragToolAction: ActionBase
    {
        public struct ToggleMarkerToolData
        {
            public bool toolState;

            public static readonly ToggleMarkerToolData defaultData = new ToggleMarkerToolData()
            {
                toolState = false
            };
        }

        public object Data { get; }

        ToggleMarkerDragToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (ToggleMarkerToolData)actionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDragMarkerToolDataProvider.toolState), data.toolState);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new ToggleMarkerDragToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MarkerDraggableEditorContext.current;
        }
    }

    public class MarkerAnchor : SelectObjectDragToolAction.IAnchor
    {
        public int objectId { get; }
        public Vector3 position { get; }
        public Vector3 normal { get; }

        public MarkerAnchor(int _objectid, Vector3 _position, Vector3 _normal)
        {
            objectId = _objectid;
            position = _position;
            normal = _normal;
        }
    }

    public class SelectObjectDragToolAction: ActionBase
    {
        public interface IAnchor
        {
            int objectId { get; }
            Vector3 position { get; }
            Vector3 normal { get; }
        }

        public object Data { get; }

        SelectObjectDragToolAction(object data)
        {
            Data = data;
        }

        public override void ApplyPayload<T>(object actionData, ref T stateData, Action onStateDataChanged)
        {
            var data = (IAnchor)actionData;
            object boxed = stateData;
            var hasChanged = false;

            hasChanged |= SetPropertyValue(ref stateData, ref boxed, nameof(IDragMarkerToolDataProvider.selectedAnchor), data);
            if (hasChanged)
                onStateDataChanged?.Invoke();
        }

        public static Payload<IViewerAction> From(object data)
            => Payload<IViewerAction>.From(new SelectObjectDragToolAction(data), data);

        public override bool RequiresContext(IUIContext context, object viewerActionData)
        {
            return context == MarkerDraggableEditorContext.current;
        }
    }
}
