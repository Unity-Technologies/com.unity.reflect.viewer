using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Reflect.Viewer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace ReflectViewerEditorTests
{
    public class SettingTests
    {
        [Test]
        public void Verify_ForwardRenderer_SSAO_RenderPassEvent_After_SKyboxTest()
        {
            var renderersGuids = AssetDatabase.FindAssets("t:ScriptableRendererData", new[] { "Assets/Settings" });
            foreach(var guid in renderersGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
                var ssao = renderer.rendererFeatures.FirstOrDefault((f) => f is ScreenSpaceAmbientOcclusion);
                if (ssao != null)
                {
                    var ssaoSettings = (ScreenSpaceAmbientOcclusionSettings)(typeof(ScreenSpaceAmbientOcclusion).GetField("m_Settings",
                      BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ssao));
                    Assert.That(ssaoSettings.renderPassEvent == RenderPassEvent.AfterRenderingSkybox);
                }
            }
        }

        [Test]
        public void Verify_Thumbnail_Mask_Is_Deafult()
        {
            var openScene = EditorSceneManager.OpenScene("Assets/Scenes/Reflect.unity");
            var thumbnailController = GameObject.FindObjectOfType<ThumbnailController>();

            var layerMasky = (LayerMask)(typeof(ThumbnailController).GetField("m_ThumbnailMask",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(thumbnailController));
            Assert.That(layerMasky == LayerMask.GetMask("Default"));
        }
    }
}
