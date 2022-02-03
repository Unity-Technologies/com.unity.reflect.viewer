using System;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace Unity.Reflect.Markers.Camera
{
    /// <summary>
    /// Provides Color32 frames from an XR Camera source
    /// </summary>
    public class XRCameraSource : MonoBehaviour, ICameraSource
    {
        [SerializeField]
        ARCameraManager m_CamManager;
        XRCameraConfiguration m_DefaultCameraConfig;
        public bool Ready
        {
            get => m_CamManager && m_CamManager.isActiveAndEnabled && m_CamManager.permissionGranted;
        }

        public int RequestedWidth { get; set; } = 1920;
        public int RequestedHeight { get; set; } = 1080;

        bool m_CameraConfiguredForQR = false;

        public Texture Texture => null;


        public void Run()
        {
            if (!m_CamManager)
                m_CamManager = FindObjectOfType<ARCameraManager>();
            Debug.Assert(m_CamManager);

        }

        /// <summary>
        /// Set the XRCameraConfiguration to a higher resolution for QR scanning
        /// </summary>
        async Task SetConfiguration()
        {
            if (m_CameraConfiguredForQR)
                return;
            m_CamManager.subsystem.Stop();
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            var configurations = m_CamManager.GetConfigurations(Allocator.Temp);
            int bestIndex = 0;
            int bestWidth = int.MaxValue;
            var currentConfiguration = m_CamManager.currentConfiguration;
            if (currentConfiguration != null)
                m_DefaultCameraConfig = currentConfiguration.Value;
            for (int i = 0; i < configurations.Length; i++)
            {
                if (configurations[i].resolution.x >= RequestedWidth)
                {
                    int diff = Mathf.Abs(configurations[i].resolution.x - RequestedWidth);
                    if (diff < bestWidth)
                    {
                        bestWidth = diff;
                        bestIndex = i;
                    }
                }
            }
            m_CamManager.subsystem.currentConfiguration = configurations[bestIndex];

            m_CamManager.subsystem.autoFocusRequested = true;
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            m_CamManager.subsystem.Start();
            await Task.Delay(TimeSpan.FromMilliseconds(10));
            m_CameraConfiguredForQR = true;

        }

        /// <summary>
        /// Reset the configuration to the state before capturing begun
        /// </summary>
        void ResetConfiguration()
        {
            if (!m_CameraConfiguredForQR)
            {
                m_CamManager.subsystem.currentConfiguration = m_DefaultCameraConfig;
                m_CamManager.subsystem.autoFocusRequested = false;
                m_CameraConfiguredForQR = false;

            }
        }

        public async Task<Color32Result> GrabFrameSyncronus()
        {
            await SetConfiguration();
            unsafe
            {
                Vector2Int imageSize = Vector2Int.zero;
                Color32[] colors = null;

                // Obtain image from AR Camera
                XRCpuImage cameraImage;
                if (!m_CamManager.TryAcquireLatestCpuImage(out cameraImage))
                {
                    Debug.LogError("Failed to get CPU image");
                    return null;
                }

                imageSize = new Vector2Int(cameraImage.width, cameraImage.height);
                // Configure the conversion from native camera output to RGBA32
                var conversionParams = new XRCpuImage.ConversionParams
                {
                    inputRect = new RectInt(0, 0, cameraImage.width, cameraImage.height),
                    outputDimensions = imageSize,
                    outputFormat = TextureFormat.RGBA32,
                    transformation = XRCpuImage.Transformation.MirrorY
                };

                Debug.Log("Get size");
                int size = 0;
                try
                {
                    // Convert the image data into a buffer
                    size = cameraImage.GetConvertedDataSize(conversionParams);
                }
                catch (Exception e)
                {
                    Debug.Log($"Error parsing params {e}");
                }

                Debug.Log("Convert");

                var buffer = new NativeArray<Color32>(size, Allocator.Temp);
                cameraImage.Convert(conversionParams, new System.IntPtr(buffer.GetUnsafePtr()), buffer.Length);

                if (!buffer.IsCreated || size == 0)
                {
                    buffer.Dispose();
                    cameraImage.Dispose();
                    Debug.LogError($"Unable to create buffer, or size zero. BufferCreated:{buffer.IsCreated} DataSize:{size}");
                    return null;
                }
                //Copy buffer to Color32 array
                try
                {
                    colors = new Color32[buffer.Length];
                    buffer.CopyTo(colors);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                //
                buffer.Dispose();
                cameraImage.Dispose();
                return new Color32Result(colors, imageSize.x, imageSize.y);
            }
        }

        public async Task<Color32Result> GrabFrame()
        {
            await SetConfiguration();
            Vector2Int imageSize = Vector2Int.zero;
            Color32[] colors = null;

            // Obtain image from AR Camera
            XRCpuImage cameraImage;
            if (!m_CamManager.TryAcquireLatestCpuImage(out cameraImage))
            {
                Debug.LogError("Failed to get CPU image");
                return null;
            }
            imageSize = new Vector2Int(RequestedWidth, RequestedHeight);

            // Configure the conversion from native camera output to RGBA32
            var conversionParams = new XRCpuImage.ConversionParams
            {
                inputRect = new RectInt(0, 0, cameraImage.width, cameraImage.height),
                outputDimensions = imageSize,
                outputFormat = TextureFormat.RGBA32,
                transformation = XRCpuImage.Transformation.MirrorY
            };
            int size = 0;
            try
            {
                // Convert the image data into a buffer
                size = cameraImage.GetConvertedDataSize(conversionParams);
            }
            catch (Exception e)
            {
                Debug.Log($"Error parsing params {e}");
            }

            Debug.Log("Convert");
            XRCpuImage.AsyncConversion conversionResult = cameraImage.ConvertAsync(conversionParams);

            while (!conversionResult.status.IsDone())
            {
                await Task.Delay(1);
            }

            if (conversionResult.status.IsError())
            {
                cameraImage.Dispose();
                Debug.LogError($"Failed to convert image Reason: {conversionResult.status}");
                conversionResult.Dispose();
                return null;
            }
            //Copy buffer to Color32 array
            try
            {
                var temp = conversionResult.GetData<Color32>().ToArray();
                colors = new Color32[temp.Length];
                temp.CopyTo(colors, 0);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            // Dispose of the temporary objects
            conversionResult.Dispose();
            cameraImage.Dispose();
            return new Color32Result(colors, imageSize.x, imageSize.y);
        }

        public void Stop()
        {
            ResetConfiguration();
        }
    }
}
