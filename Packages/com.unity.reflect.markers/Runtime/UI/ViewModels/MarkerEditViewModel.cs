using System;
using Unity.Properties;
using Unity.Reflect.Markers.Storage;
using Unity.Reflect.Markers.UI;
using Unity.Reflect.Model;
using Unity.TouchFramework;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.Reflect.Markers.Model
{
    [Serializable, GeneratePropertyBag]
    public struct MarkerEditViewModel : IEquatable<MarkerEditViewModel>
    {
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public string Name { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float X { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float Y { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float Z { get; set; }

        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float XAxis { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float YAxis { get; set; }
        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public float ZAxis { get; set; }


        [CreateProperty]
        [field: SerializeField, DontCreateProperty]
        public MarkerActions Actions { get; set; }

        public string Id { get; set; }

        public void Present(IMarker data)
        {
            Id = data.Id.ToString();
            Name = data.Name;

            X = data.RelativePosition.x;
            Y = data.RelativePosition.y;
            Z = data.RelativePosition.z;

            XAxis = data.RelativeRotationEuler.x;
            YAxis = data.RelativeRotationEuler.y;
            ZAxis = data.RelativeRotationEuler.z;
        }

        public IMarker Update(IMarker marker)
        {
            marker.Name = Name;
            marker.RelativePosition = new Vector3(X, Y, Z);
            marker.RelativeRotationEuler = new Vector3(XAxis, YAxis, ZAxis);

            return marker;
        }

        public Marker ToMarker()
        {
            Marker response = new Marker();
            response.Id = new SyncId(Id);
            response.Name = Name;
            response.RelativePosition = new Vector3(X, Y, Z);
            response.RelativeRotationEuler = new Vector3(XAxis, YAxis, ZAxis);
            response.ObjectScale = Vector3.one;

            return response;
        }

        public bool Equals(MarkerEditViewModel other)
        {
            return Name == other.Name &&
                X.Equals(other.X) &&
                Y.Equals(other.Y) &&
                Z.Equals(other.Z) &&
                XAxis.Equals(other.XAxis) &&
                YAxis.Equals(other.YAxis) &&
                ZAxis.Equals(other.ZAxis) &&
                Actions.Equals(other.Actions) &&
                Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is MarkerEditViewModel other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ X.GetHashCode();
                hashCode = (hashCode * 397) ^ Y.GetHashCode();
                hashCode = (hashCode * 397) ^ Z.GetHashCode();
                hashCode = (hashCode * 397) ^ XAxis.GetHashCode();
                hashCode = (hashCode * 397) ^ YAxis.GetHashCode();
                hashCode = (hashCode * 397) ^ ZAxis.GetHashCode();
                hashCode = (hashCode * 397) ^ Actions.GetHashCode();
                hashCode = (hashCode * 397) ^ (Id != null ? Id.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
