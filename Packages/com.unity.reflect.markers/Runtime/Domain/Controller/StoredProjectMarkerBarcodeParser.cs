using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Reflect.Markers.Camera;
using Unity.Reflect.Markers.Storage;
using UnityEngine;

namespace Unity.Reflect.Markers.Domain.Controller
{
    public class StoredProjectMarkerBarcodeParser : MonoBehaviour, IBarcodeDataParser
    {
        [SerializeField]
        MarkerController m_MarkerController;

        void Start()
        {
            m_MarkerController.BarcodeDataParser = this;
        }

        public bool TryParse(string inputData, out IMarker marker)
        {
            ProjectMarkerBarcodeData data = new ProjectMarkerBarcodeData();
            if (data.Deserialize(inputData))
            {
                // @@TODO: We can add searching & switching over to a project if the barcode was for something else.

                // Look inside the current marker storage for this marker.
                var markerSuccess = m_MarkerController.MarkerStorage.Get(data.MarkerID);
                if (markerSuccess != null)
                {
                    marker = markerSuccess.Value;
                    return true;
                }
            }

            marker = null;
            return false;
        }

        public string Generate(IMarker marker, UnityProject project)
        {
            ProjectMarkerBarcodeData data = new ProjectMarkerBarcodeData();
            data.MarkerID = marker.Id.ToString();
            data.ProjectID = project.ProjectId;
            return data.Serialize();
        }
    }
}
