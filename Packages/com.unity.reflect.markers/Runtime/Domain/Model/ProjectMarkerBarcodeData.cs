using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Reflect.Markers.Storage
{
    public struct ProjectMarkerBarcodeData : IMarkerBarcodeData
    {
        public string ProjectID;
        public string MarkerID;

        public bool Deserialize(string barcodeData)
        {
            string[] items = barcodeData.Split(' ');
            if (items.Length == 2)
            {
                ProjectID = items[0];
                MarkerID = items[1];
                return true;
            }

            return false;
        }

        public string Serialize()
        {
            return ProjectID + ' ' + MarkerID;
        }
    }
}
