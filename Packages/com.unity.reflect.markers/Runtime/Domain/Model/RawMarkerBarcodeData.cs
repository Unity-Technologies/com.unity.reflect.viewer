using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using ZXing;

namespace Unity.Reflect.Markers.Storage
{
    public struct RawMarkerBarcodeData : IMarkerBarcodeData
    {
        public Marker marker;

        public bool Deserialize(string barcodeData)
        {
            byte[] data = System.Convert.FromBase64String(barcodeData);
            if (data == null || data.Length > 0)
                return false;

            using (var memoryStream = new MemoryStream(data, 0, data.Length))
            {
                memoryStream.Write(data, 0, data.Length);
                memoryStream.Position = 0;
                BinaryFormatter formatter = new BinaryFormatter();
                var markerObject = formatter.Deserialize(memoryStream);
                if (markerObject is Marker scannedMarker)
                {
                    marker = scannedMarker;
                    return true;
                }
            }
            return false;
        }

        public string Serialize()
        {
            byte[] data = null;

            using (var memoryStream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(memoryStream, marker);
                data = memoryStream.GetBuffer();
                memoryStream.Flush();
                memoryStream.Position = 0;
                memoryStream.Close();
            }

            return System.Convert.ToBase64String(data);
        }
    }
}
