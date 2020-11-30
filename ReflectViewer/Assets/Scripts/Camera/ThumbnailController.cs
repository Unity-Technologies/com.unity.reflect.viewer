using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Reflect.IO;
using Unity.Reflect.Viewer.UI;
using Unity.XRTools.Utils;
using UnityEngine;
using UnityEngine.Reflect;
using UnityEngine.Rendering.Universal;

namespace Unity.Reflect.Viewer
{
    public class ThumbnailController : MonoBehaviour
    {
        public const int k_ThumbnailDimension = 144;
        public const string k_ThumbnailFolderName = "thumbnails";

        public const int k_ThumbnailHistoryAmount = 10;

        private static PlayerStorage m_PlayerStorage = new PlayerStorage();

#pragma warning disable CS0649
        [SerializeField]
        ScriptableRendererFeature[] m_CameraFeaturesToDisable;
        [SerializeField]
        Camera m_Camera;
        [SerializeField]
        LayerMask m_ThumbnailMask;
#pragma warning restore CS0649

        public void Awake()
        {
            m_PlayerStorage.SetEnvironment(UnityEngine.Reflect.ProjectServer.ProjectDataPath, true, false);
        }

        public static Sprite LoadThumbnailForProject(Project project)
        {
            var thumbnailPath = GetProjectThumbnailLoadPath(project);
            if (File.Exists(thumbnailPath))
            {
                var imageData = File.ReadAllBytes(thumbnailPath);
                var tex = new Texture2D(k_ThumbnailDimension, k_ThumbnailDimension);
                if (tex.LoadImage(imageData))
                {
                    return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                }
            }
            return null;
        }

        public Sprite CaptureActiveProjectThumbnail(UIProjectStateData projectStateData)
        {
            if (m_Camera == null || !m_Camera.gameObject.activeInHierarchy)
                m_Camera = Camera.main;

            if (m_Camera != null)
            {
                var thumbnailPositionRotation = FreeFlyCamera.CalculateViewFitPosition(projectStateData.rootBounds, 20.0f, 0.75f, m_Camera.fieldOfView);
                var thumbnailTexture = CaptureCameraFrame(
                    m_Camera,
                    k_ThumbnailDimension, k_ThumbnailDimension,
                    thumbnailPositionRotation,
                    m_ThumbnailMask,
                    m_CameraFeaturesToDisable); // There are some issue with SSAO, needs to disable it before Rendering or everything comes black

                var imageData = thumbnailTexture.EncodeToPNG();
                var path = GetProjectThumbnailSavePath(projectStateData.activeProject);
                File.WriteAllBytes(path, imageData);
                return Sprite.Create(thumbnailTexture, new Rect(0, 0, thumbnailTexture.width, thumbnailTexture.height), new Vector2(0.5f, 0.5f), 100.0f);
            }
            return null;
        }

        public static string GetProjectThumbnailSavePath(Project project)
        {
            var thumbnailFolder = new DirectoryInfo(string.Format("{0}/{1}", m_PlayerStorage.GetProjectFolder(project), k_ThumbnailFolderName));
            if (!Directory.Exists(thumbnailFolder.FullName))
                Directory.CreateDirectory(thumbnailFolder.FullName);

            var thumbnails = thumbnailFolder.GetFiles()
                .Where(file => file.Extension.Equals(".png"))
                .OrderByDescending(file => file.LastWriteTime)
                .Select(file => file.FullName)
                .ToArray();

            if(thumbnails.Length >= k_ThumbnailHistoryAmount)
            {
                for (int i = k_ThumbnailHistoryAmount-1; i < thumbnails.Length; i++)
                {
                    File.Delete(thumbnails[i]);
                }
                Array.Resize(ref thumbnails, k_ThumbnailHistoryAmount);
            }

            return string.Format("{0}/{1}.png", thumbnailFolder.FullName, Guid.NewGuid().ToString());
        }

        public static string GetProjectThumbnailLoadPath(Project project)
        {
            var thumbnailFolder = new DirectoryInfo(string.Format("{0}/{1}", m_PlayerStorage.GetProjectFolder(project), k_ThumbnailFolderName));
            if (!Directory.Exists(thumbnailFolder.FullName))
                Directory.CreateDirectory(thumbnailFolder.FullName);

            var mostRecentThumbnail = thumbnailFolder.GetFiles()
                .Where(file => file.Extension.Equals(".png"))
                .OrderByDescending(file => file.LastWriteTime)
                .Select(file => file.FullName)
                .FirstOrDefault();

            return mostRecentThumbnail ?? string.Empty;
        }

        public static Texture2D CaptureCameraFrame(Camera camera, int frameWidth, int frameHeight, CameraTransformInfo captureLocation, LayerMask layerToRender, ScriptableRendererFeature[] featuresToDisable = null)
        {
            var originalTransform = new CameraTransformInfo() { position = camera.transform.position, rotation = camera.transform.rotation.eulerAngles };
            var originalRenderTarget = camera.targetTexture;
            var originalRenderTexture = RenderTexture.active;
            var originalLayerMask = camera.cullingMask;
            var rt = RenderTexture.GetTemporary(frameWidth, frameHeight, 0, RenderTextureFormat.ARGB32);

            camera.transform.position = captureLocation.position;
            camera.transform.rotation = Quaternion.Euler(captureLocation.rotation);
            camera.targetTexture = rt;
            camera.cullingMask = layerToRender;
            RenderTexture.active = rt;
            var originalFeaturesActive = new bool[featuresToDisable.Length];
            if(featuresToDisable != null)
            {
                for (int i = 0; i < featuresToDisable.Length; i++)
                {
                    originalFeaturesActive[i] = featuresToDisable[i].isActive;
                    featuresToDisable[i].SetActive(false);
                }
            }

            camera.Render();
            var tex = new Texture2D(frameWidth, frameHeight, TextureFormat.RGB24, false);
            TextureUtils.RenderTextureToTexture2D(rt, tex);

            camera.transform.position = originalTransform.position;
            camera.transform.rotation = Quaternion.Euler(originalTransform.rotation);
            camera.targetTexture = originalRenderTarget;
            camera.cullingMask = originalLayerMask;
            RenderTexture.active = originalRenderTexture;

            if (featuresToDisable != null)
            {
                for (int i = 0; i < featuresToDisable.Length; i++)
                {
                    featuresToDisable[i].SetActive(originalFeaturesActive[i]);
                }
            }

            RenderTexture.ReleaseTemporary(rt);
            return tex;
        }

        public static void SaveTextureToPng(Texture2D texture2D, string pathWithName)
        {
            byte[] bytes = texture2D.EncodeToPNG();
            File.WriteAllBytes(pathWithName, bytes);
        }

        public static Texture2D RenderTextureToTexture2D(RenderTexture renderTexture)
        {
            Texture2D tex = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGB24, false);
            RenderTexture.active = renderTexture;
            tex.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            tex.Apply();
            return tex;
        }
    }
}
