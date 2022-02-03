using System.Threading.Tasks;
using UnityEngine;

namespace Unity.Reflect.Markers.Camera
{
    public class WebCameraSource : MonoBehaviour, ICameraSource
    {

        public bool Ready => m_Texture != null && m_Texture.isReadable && m_Texture.width > 100;
        public int RequestedWidth { get; set; } = 1280;
        public int RequestedHeight { get; set; } = 720;

        private WebCamTexture m_Texture;

        public Texture Texture
        {
            get => m_Texture;
        }

        public void Run()
        {
            if (!m_Texture)
                m_Texture = new WebCamTexture(RequestedWidth, RequestedHeight);
            CheckPermission();
            m_Texture.Play();
        }

#pragma warning disable 1998
        public async Task<Color32Result> GrabFrame()
        {
            return new Color32Result(m_Texture.GetPixels32(), m_Texture.width, m_Texture.height);
        }
#pragma warning restore 1998

        public void Stop()
        {
            m_Texture.Stop();
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
