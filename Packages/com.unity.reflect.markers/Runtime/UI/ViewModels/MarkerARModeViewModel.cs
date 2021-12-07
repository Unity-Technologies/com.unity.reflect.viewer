using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

namespace Unity.Reflect.Markers.Model
{
    [Serializable, GeneratePropertyBag]
    public struct MarkerARModeViewModel : IEquatable<MarkerARModeViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Action QR { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public Action Retrack { get; set; }

        public bool Equals(MarkerARModeViewModel other)
        {
            return Equals(QR, other.QR) && Equals(Retrack, other.Retrack);
        }

        public override bool Equals(object obj)
        {
            return obj is MarkerARModeViewModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((QR != null ? QR.GetHashCode() : 0) * 397) ^ (Retrack != null ? Retrack.GetHashCode() : 0);
            }
        }
    }
}
