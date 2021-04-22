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
    public class CameraTests : BaseReflectSceneTests
    {

        [UnityTest]
        public IEnumerator Camera_IfNoInputGiven_CameraDoesntMove()
        {
            //Given the main camera is in a certain position
            Camera mainCamera = GivenObjectNamed<Camera>("Main Camera");
            var position = mainCamera.transform.position;

            //When there is not input between frames
            yield return WaitAFrame();

            //Then the camera should remain in that position
            Assert.That(mainCamera.transform.position.Equals(position));
        }

        [UnityTest]
        public IEnumerator Camera_OnStartup_ClearFlagsIsSetToSkybox()
        {
            //Given Session is ready and there is a main camera
            yield return WaitAFrame();
            Camera mainCamera = GivenObjectNamed<Camera>("Main Camera");

            //Then the camera's clear flags should be set to skybox
            Assert.That(mainCamera.clearFlags == CameraClearFlags.Skybox);
        }
    }
}
