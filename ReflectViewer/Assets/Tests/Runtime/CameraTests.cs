using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using Unity.MARS.Providers;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace ReflectViewerRuntimeTests
{
    public class CameraTests
    {
        bool sceneLoaded;
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene("Reflect", LoadSceneMode.Single);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneLoaded = true;
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator Verify_Camera_ClearFlags_SkyboxTest()
        {
            yield return new WaitWhile(() => sceneLoaded == false);
            yield return null;

            IUsesSessionControl uiStateManager = Resources.FindObjectsOfTypeAll<UIStateManager>().First();
            yield return new WaitWhile(() => uiStateManager.SessionReady() == false);
            yield return null;

            Camera[] objects = Resources.FindObjectsOfTypeAll<Camera>();
            Camera mainCamera = objects.SingleOrDefault(e => e.name == "Main Camera");

            Assert.That(mainCamera.clearFlags == CameraClearFlags.Skybox);
        }
    }
}
