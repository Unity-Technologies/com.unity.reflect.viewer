using System;
using System.Collections.Generic;

namespace UnityEngine.Reflect.MeasureTool
{
    public enum MeasureMode : int
    {
        RawDistance = 0,
        PerpendicularDistance = 1,
    }

    public enum MeasureFormat : int
    {
        Meters = 0,
        Feets = 1,
    }

    public enum MeasureToolInstructionUIState
    {
        Init = 0,
        Started,
        Completed
    };

    public interface IMeasureUIButtonValidator
    {
        bool ButtonValidate();
    }

    [Serializable]
    public struct MeasureToolStateData : IEquatable<MeasureToolStateData>
    {
        public static readonly MeasureToolStateData defaultData = new MeasureToolStateData()
        {
            toolState = false,
            selectionType = AnchorType.Point,
            measureMode = MeasureMode.RawDistance,
            measureFormat = MeasureFormat.Meters,
            selectedAnchorsContext = null
        };

        public bool toolState;
        public AnchorType selectionType;
        public MeasureMode measureMode;
        public MeasureFormat measureFormat;
        public List<AnchorSelectionContext> selectedAnchorsContext;

        public bool Equals(MeasureToolStateData other)
        {
            return toolState == other.toolState &&
                selectionType == other.selectionType &&
                measureMode == other.measureMode &&
                measureFormat == other.measureFormat &&
                selectedAnchorsContext == other.selectedAnchorsContext;
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
                hashCode = (hashCode * 397) ^ selectedAnchorsContext.GetHashCode();

                return hashCode;
            }
        }
    }
}
