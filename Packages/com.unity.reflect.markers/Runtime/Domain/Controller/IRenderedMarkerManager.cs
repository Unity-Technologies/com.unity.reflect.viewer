using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public interface IRenderedMarkerManager
    {

        /// <summary>
        /// Update a single rendered marker's position
        /// </summary>
        /// <param name="marker">Marker to update</param>
        public void Visualize(IMarker marker);
    }
}
