using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class RawMarkerBarcodeParser : MonoBehaviour, IBarcodeDataParser
    {
        [SerializeField]
        MarkerController m_MarkerController;

        void Start()
        {
            m_MarkerController.BarcodeDataParser = this;
        }

        public bool TryParse(string inputData, out IMarker marker)
        {
            RawMarkerBarcodeData data = new RawMarkerBarcodeData();
            if (data.Deserialize(inputData))
            {
                marker = data.marker;
                return true;
            }

            marker = null;
            return false;
        }


        public string Generate(IMarker marker, UnityProject project)
        {
            RawMarkerBarcodeData data = new RawMarkerBarcodeData();
            data.marker = (Marker)marker;
            return data.Serialize();
        }
    }
}
