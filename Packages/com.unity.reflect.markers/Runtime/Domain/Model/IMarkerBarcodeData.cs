using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    public interface IMarkerBarcodeData
    {
        public bool Deserialize(string barcodeData);
        public string Serialize();
    }
}
