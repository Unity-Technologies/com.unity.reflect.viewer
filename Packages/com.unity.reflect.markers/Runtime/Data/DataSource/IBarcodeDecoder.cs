using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Reflect.Markers.Camera
{
    public interface IBarcodeDecoder: IDisposable
    {
        Task<string> Decode(Color32Result image);
    }
}
