using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Reflect.Markers.Camera
{
    public class WebCameraSource : MonoBehaviour, ICameraSource
    {

        public bool Ready => texture != null && texture.isReadable && texture.width > 100;
        public int RequestedWidth { get; set; } = 1280;
        public int RequestedHeight { get; set; } = 720;

        private WebCamTexture texture;

        public Texture Texture
        {
            get => texture;
        }

        // Start is called before the first frame update
        void Start()
        {
            texture = new WebCamTexture(RequestedWidth, RequestedHeight);

        }

        public void Run()
        {
            CheckPermission();
            texture.Play();
        }

#pragma warning disable 1998
        public async Task<Color32Result> GrabFrame()
        {
            return new Color32Result(texture.GetPixels32(), texture.width, texture.height);
        }
#pragma warning restore 1998

        public void Stop()
        {
            texture.Stop();
        }

        void CheckPermission()
        {
            //@@TODO: Add iOS and UWP
#if UNITY_ANDROID
            if (!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera);
            }
#endif
        }
    }
}
