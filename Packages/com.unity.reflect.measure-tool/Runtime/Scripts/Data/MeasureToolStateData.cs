using System;
using Unity.Properties;
using UnityEngine.Reflect.Viewer.Core;
using UnityEngine.Reflect.Viewer.Core.Actions;

namespace UnityEngine.Reflect.MeasureTool
{
    [Serializable, GeneratePropertyBag]
    public struct MeasureToolStateData : IEquatable<MeasureToolStateData>, IMeasureToolDataProvider
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public bool toolState { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ToggleMeasureToolAction.AnchorType selectionType { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ToggleMeasureToolAction.MeasureMode measureMode { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public ToggleMeasureToolAction.MeasureFormat measureFormat { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public SelectObjectMeasureToolAction.IAnchor selectedAnchor { get; set; }

        public static readonly MeasureToolStateData defaultData = new MeasureToolStateData()
        {
            toolState = false,
            selectionType = ToggleMeasureToolAction.AnchorType.Point,
            measureMode = ToggleMeasureToolAction.MeasureMode.RawDistance,
            measureFormat = ToggleMeasureToolAction.MeasureFormat.Meters,
            selectedAnchor = null
        };

        public bool Equals(MeasureToolStateData other)
        {
            return toolState == other.toolState &&
                selectionType == other.selectionType &&
                measureMode == other.measureMode &&
                measureFormat == other.measureFormat &&
                selectedAnchor == other.selectedAnchor;
        }

        public override bool Equals(object obj)
        {
            return obj is MeasureToolStateData other && Equals(other);
        }

        public static bool operator ==(MeasureToolStateData a, MeasureToolStateData b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MeasureToolStateData a, MeasureToolStateData b)
        {
            return !(a == b);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = toolState.GetHashCode();
                hashCode = (hashCode * 397) ^ selectionType.GetHashCode();
                hashCode = (hashCode * 397) ^ measureMode.GetHashCode();
                hashCode = (hashCode * 397) ^ measureFormat.GetHashCode();
                hashCode = (hashCode * 397) ^ selectedAnchor.GetHashCode();

                return hashCode;
            }
        }
    }
}
