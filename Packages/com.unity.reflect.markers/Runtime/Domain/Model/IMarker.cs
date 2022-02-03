using System;
using UnityEngine;
using UnityEngine.Reflect;

namespace Unity.Reflect.Markers.Storage
{
    public interface IMarker : IDataInstance, ICreatedTime, ILastUpdatedTime, ILastUsedTime, IEquatable<IMarker>
    {
        /// <summary>
        /// Human readable name
        /// </summary>
        public string Name { get; set; }

        // /// <summary>
        // /// Marker 2D Image
        // /// </summary>
        // public Texture2D texture { get; }

        /// <summary>
        /// Postion relative to the associated object
        /// </summary>
        public Vector3 RelativePosition { get; set; }

        /// <summary>
        /// Rotation relative to the associated object.
        /// </summary>
        public Vector3 RelativeRotationEuler { get; set; }

        /// <summary>
        /// Scale of the associated object.
        /// </summary>
        public Vector3 ObjectScale { get; set; }
    }
}
