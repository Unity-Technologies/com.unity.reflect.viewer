using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TestTools;

namespace ReflectViewerRuntimeTests
{
    public class BaseReflectSceneTests: BaseRuntimeTests
    {
        [UsedImplicitly]
        [UnitySetUp]
        public IEnumerator Setup()
        {
            LogAssert.ignoreFailingMessages = true;
            yield return GivenTheSceneIsLoadedAndActive("Reflect");
        }

        [UsedImplicitly]
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameObject.DestroyImmediate(GameObject.Find("Multiplayer"));
            yield return GivenTheSceneIsUnloaded("Reflect");
        }

    }
}
