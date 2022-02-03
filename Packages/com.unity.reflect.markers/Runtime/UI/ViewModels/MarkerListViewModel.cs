using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using Unity.Reflect.Markers.Storage;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Reflect.Markers.UI
{
    [Serializable, GeneratePropertyBag]
    public struct MarkerListViewModel : IEquatable<MarkerListViewModel>
    {

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public MarkerListProperty Markers { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public MarkerListActions Actions { get; set; }

        public bool Equals(MarkerListViewModel other)
        {
            return Markers.Equals(other.Markers) && Actions.Equals(other.Actions);
        }
    }
}
