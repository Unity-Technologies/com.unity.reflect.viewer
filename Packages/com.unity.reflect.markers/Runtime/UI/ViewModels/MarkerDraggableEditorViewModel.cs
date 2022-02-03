using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.Reflect.Markers.Placement;
using Unity.Reflect.Markers.UI;
using UnityEngine;


namespace Unity.Reflect.Markers.Model
{
    [Serializable, GeneratePropertyBag]
    public struct MarkerDraggableEditorViewModel : IEquatable<object>, IDragMarkerToolDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool toolState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Action Save { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Action Cancel { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SelectObjectDragToolAction.IAnchor selectedAnchor { get; set; }
    }
}
