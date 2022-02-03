using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Model;
using UnityEngine;

namespace Unity.Reflect.Markers.UI
{
    [Serializable]
    public struct MarkerListProperty : IEquatable<MarkerListProperty>
    {
        public List<Marker> Markers;
        public List<SyncId> Selected;
        public SyncId Active;
        public SelectionMode Mode;
        public enum SelectionMode
        {
            Single,
            Multiple
        };
        public Action OnSelectionUpdated;

        public bool Equals(MarkerListProperty other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(Markers, other.Markers) && Equals(Selected, other.Selected) && Active.Equals(other.Active) && Mode == other.Mode && Equals(OnSelectionUpdated, other.OnSelectionUpdated);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MarkerListProperty)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Markers != null ? Markers.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Selected != null ? Selected.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Active.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Mode;
                hashCode = (hashCode * 397) ^ (OnSelectionUpdated != null ? OnSelectionUpdated.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
