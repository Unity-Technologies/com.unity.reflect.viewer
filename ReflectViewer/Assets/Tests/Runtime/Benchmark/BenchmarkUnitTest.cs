using NUnit.Framework;
using ReflectViewerRuntimeTests;
using System.Collections;
using System.Linq;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Viewer;
using UnityEngine;
using UnityEngine.TestTools;

[TestFixture, UnityPlatform(include: new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer })]
public class BenchmarkUnitTest
{
    [UnityTest]
    public IEnumerator TestBenchmarkSetup()
    {
        yield return new PerformanceTests().Setup();
        var viewerReflectBootstrapper = Object.FindObjectsOfType<ViewerReflectBootstrapper>().First();
        ActorSystemSetupAnalyzer.MigrateInPlace(viewerReflectBootstrapper.Asset);
    }
}
