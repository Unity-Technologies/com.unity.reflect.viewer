using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Reflect.Markers.Camera
{
    public interface ICameraSource
    {
        bool Ready { get; }
        int RequestedWidth { get; set; }
        int RequestedHeight { get; set; }

        Texture Texture { get; }

        Task<Color32Result> GrabFrame();
        void Run();
        void Stop();
    }
}
