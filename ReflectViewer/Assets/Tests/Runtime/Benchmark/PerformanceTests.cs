using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Reflect.ActorFramework;
using Unity.Reflect.Actors;
using Unity.Reflect.Viewer;
using Unity.Reflect.Viewer.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;
using UnityEngine.Reflect;
using Object = UnityEngine.Object;

namespace ReflectViewerRuntimeTests
{
    [TestFixture, UnityPlatform(include: new[] { RuntimePlatform.WindowsEditor, RuntimePlatform.WindowsPlayer })]
    public class PerformanceTests : IPrebuildSetup, IPostBuildCleanup
    {
        // Change this flag to false to run the test locally
        bool k_IsRunningUnderUtr = true;

        bool m_SceneLoaded;
        Bounds m_ModelBounds;
        bool m_IsModelLoaded;
        static bool s_UtrBuild;
        public static int s_InstanceCount;

        ViewerReflectBootstrapper m_ViewerReflectBootstrapper;
        string m_ModelFolderPath;
        BenchmarkUtils m_BenchmarkTestsUtils;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(t => typeof(BenchmarkUtils).IsAssignableFrom(t) && t.IsClass)
                .OrderByDescending(x => ((BenchmarkUtilsImplPriorityAttribute)Attribute.GetCustomAttribute(x, typeof(BenchmarkUtilsImplPriorityAttribute))).Priority)
                .ToList();

            m_BenchmarkTestsUtils = (BenchmarkUtils)Activator.CreateInstance(types.First());

            if (k_IsRunningUnderUtr)
            {
                m_ModelFolderPath = Directory.EnumerateDirectories(Directory.GetParent(Application.dataPath).Parent.FullName, ".PerformanceTestProjects", SearchOption.AllDirectories).FirstOrDefault();

                QualitySettings.SetQualityLevel(0);
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                Screen.fullScreen = true;
                QualitySettings.vSyncCount = 0;
            }
            else
                m_ModelFolderPath = FindProjectsPath();

            if (m_ModelFolderPath == null)
                Debug.LogError("Unable to find Samples data. Reflect Samples require local Reflect Model data in '.PerformanceTestProjects' in " + Directory.GetParent(Application.dataPath).Parent.FullName);

            if (!m_SceneLoaded)
            {
                //Load scene
                SceneManager.sceneLoaded += (s, a) => { m_SceneLoaded = true; };
                SceneManager.LoadScene(m_BenchmarkTestsUtils.GetScenePath(), LoadSceneMode.Single);

                yield return new WaitWhile(() => m_SceneLoaded == false);

                m_BenchmarkTestsUtils.PostSceneSetup();

                //Too many exception with UI without user
                var uiStateManager = Object.FindObjectOfType<UIStateManager>();
                if (uiStateManager != null)
                    uiStateManager.GetComponent<Canvas>().enabled = false;

                var loginManager = Object.FindObjectOfType<LoginManager>();
                if (loginManager != null)
                    loginManager.enabled = false;

                yield return null;

                //Activate Mars session to get the main camera
                var scene = SceneManager.GetActiveScene();
                var rootGameObjects = new List<GameObject>();
                scene.GetRootGameObjects(rootGameObjects);
                foreach (var go in rootGameObjects)
                {
                    if (go.name.Equals("MARS Session"))
                    {
                        go.SetActive(true);
                        break;
                    }
                }

                m_ViewerReflectBootstrapper = UnityEngine.Object.FindObjectsOfType<ViewerReflectBootstrapper>().First();

                var actorSystemSetup = UnityEngine.Object.Instantiate(m_ViewerReflectBootstrapper.Asset);

                //Clear exclude and Migrate to get benchmark Actor
                actorSystemSetup.ExcludedAssemblies.RemoveAll(x => x == "BenchmarkAssembly");
                ActorSystemSetupAnalyzer.InitializeAnalyzer(actorSystemSetup);
                ActorSystemSetupAnalyzer.PrepareInPlace(actorSystemSetup);
                ActorSystemSetupAnalyzer.MigrateInPlace(actorSystemSetup);

                //Create and connect new actor
                var newProviderSetup = actorSystemSetup.CreateActorSetup<BenchmarkActor>();
                actorSystemSetup.ReplaceActor(actorSystemSetup.GetActorSetup<DataProviderActor>(), newProviderSetup);
                actorSystemSetup.ConnectNet<SpatialDataChanged>(actorSystemSetup.GetActorSetup<DynamicEntryFilterActor>(), newProviderSetup);

                //Assign root transform to GameObjectBuilderActor and SpatialActor
                actorSystemSetup.GetActorSetup<GameObjectBuilderActor>().GetActorSettings<GameObjectBuilderActor.Settings>().Root = GameObject.Find("Root").transform;
                actorSystemSetup.GetActorSetup<SpatialActor>().GetActorSettings<SpatialActor.Settings>().Root = GameObject.Find("Root").transform;

                //Replace Asset by modified temp asset
                m_ViewerReflectBootstrapper.Asset = actorSystemSetup;

                yield return null;

                var freeFly = Camera.main.GetComponent<FreeFlyCamera>();
                if(freeFly != null)
                    freeFly.enabled = false;

                if (k_IsRunningUnderUtr)
                    m_ViewerReflectBootstrapper.StartCoroutine(disableVsync());

                yield return new WaitForSeconds(10);
            }
        }

        IEnumerator disableVsync()
        {
            while (true)
            {
                yield return new WaitForSeconds(2);
                if (QualitySettings.vSyncCount != 0)
                    QualitySettings.vSyncCount = 0;
            }
        }

        static string FindProjectsPath()
        {
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
            while (dir != null)
            {
                if (dir.Name == "reflect.viewer")
                    return Path.Combine(dir.FullName, "Internal/Model/.PerformanceTestProjects");

                dir = dir.Parent;
            }

            return "";
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            //until https://jira.unity3d.com/browse/DSTP-611 is fixed
            Debug.Log($"{DateTime.Now} TearDown Application.Quit(0)");
            if (k_IsRunningUnderUtr)
                Application.Quit(0);
        }

        //Todo Have tests for each model with specific camera path and/or position
        [UnityTest, Performance, Timeout(6_000_000)]
        [TestCase("RAC_basic_sample", new[] { 5, 2, -30 }, 60, new[] { 0, 0, 0 }, 0.02f, 30, new[] { 0, 0, 1 }, ExpectedResult = null)]
        [TestCase("High_Rise", new[] { 100, 25, -110 }, 140, new[] { 0, 0, 0 }, 0.04f, 30, new[] { 0, 0, 1 }, ExpectedResult = null)]
        [TestCase("High_Rise_Hlod", new[] { 100, 25, -110 }, 140, new[] { 0, 0, 0 }, 0.04f, 30, new[] { 0, 0, 1 }, ExpectedResult = null)]
        [TestCase("Villa_Martini", new[] { -10, 2, -30 }, 60, new[] { 0, 0, 0 }, 0.02f, 30, new[] { 0, 0, 1 }, ExpectedResult = null)]
        [TestCase("Chafiks_Model", new[] { -10, 20, -30 }, 60, new[] { 0, 0, 0 }, 0.02f, 30, new[] { 0, 0, 1 }, ExpectedResult = null)]
        [TestCase("YVR", new[] { 3939, 18, 4380 }, 270, new[] { 4039, 0, 4560 }, 0.06f, 45, new[] { 0, 0, 1 }, ExpectedResult = null)]
        [TestCase("Prodware", new[] { 48, 28, -57 }, 130, new[] { 62, 20, 18 }, 0.02f, 30, new[] { 0, 1, 0 }, ExpectedResult = null)]
        //https://jira.unity3d.com/browse/RV-1278
        //[TestCase("BAT", new[] { 1652, 85, 1219 }, 800, new[] { 2338, 17, 1370 }, 0.02f, 30, new[] { 0, 0, 1 }, ExpectedResult = null)]
        public IEnumerator FpsPerformanceTest(string modelPath, int[] cameraPosition, float travelDistance, int[] lookAt, float cameraSpeed, int loadTimeoutSeconds, int[] direction)
        {
            //Place the camera so the model can load the correct objects first
            var camera = Camera.main;
            camera.transform.rotation = Quaternion.identity;
            camera.transform.position = new Vector3(cameraPosition[0], cameraPosition[1], cameraPosition[2]);

            //Load model
            yield return LoadModel(modelPath, loadTimeoutSeconds, new Vector3(cameraPosition[0], cameraPosition[1], cameraPosition[2]));

            //Move camera around and record fps
            var lookAtVector = new Vector3(lookAt[0], lookAt[1], lookAt[2]);
            var directionVector = new Vector3(direction[0], direction[1], direction[2]);
            Vector3 endPoint = camera.transform.position + (new Vector3(direction[0], direction[1], direction[2]) * travelDistance);
            var translation = cameraSpeed * directionVector;

            camera.transform.rotation = Quaternion.identity;

            using (Measure.Frames().Scope("Time"))
            {
                while (Vector3.Distance(camera.transform.position, endPoint) > 1)
                {
                    camera.transform.position += translation;
                    camera.transform.LookAt(lookAtVector);
                    yield return null;
                }
            }
            //Todo check assert with PerformanceTest.Active.SampleGroups
        }

        [UnityTest, Performance, Timeout(6_000_000)]
        [TestCase("Hong_Kong_Airport", 60, ExpectedResult = null)]
        public IEnumerator StreamingPerformanceTest(string modelPath, int loadTimeoutSeconds)
        {
            if (k_IsRunningUnderUtr)
                yield break;

            m_ModelBounds = default;
            m_IsModelLoaded = default;

            var runWithMovement = true;

            yield return StartLoadingModel(modelPath, TimeSpan.FromSeconds(loadTimeoutSeconds));

            var camera = Camera.main;
            if (runWithMovement)
            {
                camera.transform.position = m_ModelBounds.center - new Vector3(0.0f, 0.0f, -m_ModelBounds.extents.z);
                camera.transform.rotation = Quaternion.identity;
            }
            else
            {
                var fitPosition = FreeFlyCamera.CalculateViewFitPosition(m_ModelBounds, 20.0f, 0.75f, camera.fieldOfView, camera.aspect);
                camera.transform.rotation = Quaternion.Euler(fitPosition.rotation);
                camera.transform.position = fitPosition.position;
            }

            var posFromOrigin = new Vector3(0.0f, 0.0f, -Mathf.Max(m_ModelBounds.extents.z, m_ModelBounds.extents.x));
            var yAngle = 0.0f;

            var bridge = m_ViewerReflectBootstrapper.Bridge;
            bridge.Subscribe<StreamingProgressed>(ctx =>
            {
                m_IsModelLoaded = ctx.Data.NbStreamed >= ctx.Data.Total * 0.99f;
            });

            using (Measure.Frames().Scope())
            {
                while (!m_IsModelLoaded)
                {
                    if (runWithMovement)
                    {
                        yAngle += 1.0f;

                        var pos = Quaternion.Euler(0.0f, yAngle, 0.0f) * posFromOrigin * 1.1f;
                        var finalPos = pos + m_ModelBounds.center;

                        camera.transform.position = finalPos;
                        camera.transform.LookAt(m_ModelBounds.center);
                    }

                    yield return null;
                }
            }
        }

        public IEnumerator LoadModel(string modelPath, int loadTimeoutSeconds, Vector3 camPosition)
        {
            yield return new WaitForSeconds(5);

            int nbGameObject = 0;
            DateTime lastTimeGameObjectAdded = DateTime.Now;

            DateTime startLoad = DateTime.Now;
            m_ViewerReflectBootstrapper.OpenProject(null, null, null, false, (x) =>
            {
                //Set model path
                var settings = x.GetFirstOrEmptySettings<BenchmarkActor.Settings>();
                settings.ProjectPath = Path.Combine(m_ModelFolderPath, modelPath);

                //Register to event get gameobject count
                m_ViewerReflectBootstrapper.ViewerBridge.GameObjectCreating += y =>
                {
                    nbGameObject++;
                    lastTimeGameObjectAdded = DateTime.Now;
                };
            });

            //Place the camera so the model can load the correct objects first
            var camera = Camera.main;

            yield return new WaitUntil(() =>
            {
                camera.transform.rotation = Quaternion.identity;
                camera.transform.position = camPosition;

                return nbGameObject > 0 && DateTime.Now - lastTimeGameObjectAdded > TimeSpan.FromSeconds(loadTimeoutSeconds);
            });

            Measure.Custom("Load Time milliseconds", ((DateTime.Now - startLoad) - TimeSpan.FromSeconds(loadTimeoutSeconds)).TotalMilliseconds);
        }

        IEnumerator StartLoadingModel(string modelPath, TimeSpan openingTimeout)
        {
            var startedTime = GetCurrentTime();
            m_ViewerReflectBootstrapper.OpenProject(null, null, null, false, x =>
            {
                var settings = x.GetFirstOrEmptySettings<BenchmarkActor.Settings>();
                settings.ProjectPath = Path.Combine(m_ModelFolderPath, modelPath);

                m_ViewerReflectBootstrapper.Bridge.Subscribe<GlobalBoundsUpdated>(ctx =>
                {
                    m_ModelBounds = ctx.Data.GlobalBounds;
                });
            });

            yield return null;

            yield return new WaitUntil(() => m_ModelBounds != default || GetCurrentTime() - startedTime > openingTimeout);
            if (GetCurrentTime() - startedTime > openingTimeout)
                throw new TimeoutException();
            Measure.Custom("Opening Time milliseconds", (GetCurrentTime() - startedTime).TotalMilliseconds);
            yield return null;
        }

        static TimeSpan GetCurrentTime()
        {
            return TimeSpan.FromTicks(Stopwatch.GetTimestamp());
        }


        //Extract models in zip file for tests
        void IPrebuildSetup.Setup()
        {
            m_ModelFolderPath = Directory.EnumerateDirectories(Directory.GetParent(Application.dataPath).Parent.FullName, ".PerformanceTestProjects", SearchOption.AllDirectories).FirstOrDefault();

            if (m_ModelFolderPath == null)
            {
                Debug.LogError("Unable to find Samples data. Reflect Samples require local Reflect Model data in '.PerformanceTestProjects'.");
                return;
            }

            //This is the only way I found to check if we are running tests manually or building a player to run the tests
            var playModeLauncher = Type.GetType("UnityEditor.TestTools.TestRunner.PlaymodeLauncher, UnityEditor.TestRunner");
            var playModeLauncherIsRunning = playModeLauncher?.GetField("IsRunning")?.GetValue(null);

            if (playModeLauncherIsRunning == null)
                Assert.Fail("Code is not working with new package of TestRunner! was working with 1.2.24");

            s_UtrBuild = !(bool)playModeLauncherIsRunning;

            if (!k_IsRunningUnderUtr)
                return;

            if (s_UtrBuild)
            {
                string destination = Path.Combine(Application.streamingAssetsPath, ".PerformanceTestProjects");

                if (!Directory.Exists(destination))
                    Directory.CreateDirectory(destination);

                //todo extract zip on mac
                //todo check the list of test included in the build and only extract those models
                foreach (var zipFile in Directory.EnumerateFiles(m_ModelFolderPath, "*.zip", SearchOption.AllDirectories))
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.WorkingDirectory = m_ModelFolderPath;
                    process.StartInfo.Arguments = "/C tar -xf " + zipFile;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.Start();

                    string output = process.StandardOutput.ReadToEnd();
                    string err = process.StandardError.ReadToEnd();

                    if (!string.IsNullOrEmpty(output))
                        Debug.Log($"{DateTime.Now}:{zipFile}:{output}");
                    if (!string.IsNullOrEmpty(err))
                        Debug.LogError($"{DateTime.Now}:{zipFile}:{err}");

                    process.WaitForExit();

                    Debug.Log($"{DateTime.Now} Extract done for'{zipFile}' with exit code '{process?.ExitCode}'");
                }

                foreach (var directory in Directory.GetDirectories(m_ModelFolderPath))
                    Directory.Move(directory, destination + "\\" + Path.GetFileName(directory));
            }
            else
            {
                foreach (var zipFile in Directory.EnumerateFiles(m_ModelFolderPath, "*.zip", SearchOption.AllDirectories))
                {
                    string destination = new DirectoryInfo(zipFile).Parent.ToString();

                    //todo extract zip on mac
                    if (!Directory.Exists(Path.Combine(destination, Path.GetFileNameWithoutExtension(zipFile))))
                    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
                        Process.Start("cmd.exe", "/C powershell -Command Expand-Archive -Force -Path '" + zipFile + "' -DestinationPath '" + destination + "'")?.WaitForExit();
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
                        Process.Start("unzip", "'" + zipFile + "' -d '" + destination + "'")?.WaitForExit();
#endif
                    }
                }
            }
        }

        //Remove models from StreamingAssets after utr build is done
        void IPostBuildCleanup.Cleanup()
        {
            if (s_UtrBuild && k_IsRunningUnderUtr)
            {
                string destination = Path.Combine(Application.streamingAssetsPath, ".PerformanceTestProjects");
                Debug.Log(" IPostBuildCleanup.Cleanup : " + destination);
                if (Directory.Exists(destination))
                    Directory.Delete(destination, true);
            }
        }
    }
}
