using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Reflect.Viewer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.XR.Management;
using UnityEngine;
#if URP_AVAILABLE
    using UnityEngine.Rendering.Universal;
#endif

namespace ReflectViewerEditorTests
{
    public class SettingTests
    {
#if URP_AVAILABLE
        [Test]
        [Ignore("This test is irrelevant as the pass in URP10 cannot be changed in settings. We do have to validate that iOS skybox is working correctly.")]
        public void Verify_ForwardRenderer_SSAO_RenderPassEvent_After_SKyboxTest()
        {
            var renderersGuids = AssetDatabase.FindAssets("t:ScriptableRendererData", new[] { "Assets/Settings" });
            foreach(var guid in renderersGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var renderer = AssetDatabase.LoadAssetAtPath<ScriptableRendererData>(path);
                var ssao = renderer.rendererFeatures; //.FirstOrDefault((f) => f is ScreenSpaceAmbientOcclusion);
                //if (ssao != null)
                //{
                //    var ssaoSettings = (ScreenSpaceAmbientOcclusionSettings)(typeof(ScreenSpaceAmbientOcclusion).GetField("m_Settings",
                //      BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).GetValue(ssao));
                //    Assert.That(ssaoSettings.renderPassEvent == RenderPassEvent.AfterRenderingSkybox);
                //}
            }
        }
#endif

        [Test]
        public void Verify_Android_XR_Settings()
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Android);
            var settingManager = generalSettings.Manager;

            Assert.AreEqual(true,generalSettings.InitManagerOnStart);
            Assert.AreEqual(1, settingManager.activeLoaders.Count());
            Assert.AreEqual("AR Core Loader", settingManager.activeLoaders[0].name);
        }

        [Test]
        public void Verify_iOS_XR_Settings()
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.iOS);
            var settingManager = generalSettings.Manager;

            Assert.AreEqual(true,generalSettings.InitManagerOnStart);
            Assert.AreEqual(1, settingManager.activeLoaders.Count());
            Assert.AreEqual("AR Kit Loader", settingManager.activeLoaders[0].name);
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
