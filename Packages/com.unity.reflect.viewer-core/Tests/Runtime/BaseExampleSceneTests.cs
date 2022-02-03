using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.TestTools;

namespace ReflectViewerCoreRuntimeTests
{
    public class BaseExampleSceneTests: BaseRuntimeTests
    {
        [UsedImplicitly]
        [UnitySetUp]
        public IEnumerator Setup()
        {
            yield return GivenTheSceneIsLoadedAndActive("ExampleApplication");
        }

        [UsedImplicitly]
        [UnityTearDown]
        public IEnumerator TearDown()
        {
            yield return GivenTheSceneIsUnloaded("ExampleApplication");
        }
    }
}
