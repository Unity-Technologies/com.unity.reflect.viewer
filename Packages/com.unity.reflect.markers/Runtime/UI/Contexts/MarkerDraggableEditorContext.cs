using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Placement;
using UnityEngine;
using UnityEngine.Reflect.Viewer.Core;

namespace Unity.Reflect.Markers.UI
{

    public interface IDragMarkerToolDataProvider
    {
        public bool toolState { get; set; }
        public SelectObjectDragToolAction.IAnchor selectedAnchor { get; set; }
    }

    public class MarkerDraggableEditorContext : ContextBase<MarkerDraggableEditorContext>
    {
        public override List<Type> implementsInterfaces => new List<Type> {typeof(IDragMarkerToolDataProvider)};
    }
}
