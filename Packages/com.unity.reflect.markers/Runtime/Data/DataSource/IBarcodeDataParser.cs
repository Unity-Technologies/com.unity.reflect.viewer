using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Camera
{
    /// <summary>
    /// Interface for parsers of barcode stored data
    /// This will take in raw Barcode data and produce a marker
    /// The marker could be decoded from the input data, or it could be stored elsewhere like in an IMarkerStore
    /// </summary>
    public interface IBarcodeDataParser
    {
        public bool TryParse(string inputData, out IMarker marker);
        public string Generate(IMarker marker, UnityProject project);
    }
}
