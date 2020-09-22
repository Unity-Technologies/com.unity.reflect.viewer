namespace Unity.Reflect.Viewer.Builder
{
    using System.IO;
    using System.Text;
    using UnityEditor;
    using UnityEditor.Build.Reporting;
    using UnityEngine;

    public class Builder
    {

        private static void BuildViewer()
        {
            BuildSettingsParser buildSettings = new BuildSettingsParser();
            buildSettings.StartParsing();
            BuildPlayer(buildSettings.ActiveBuildTarget, buildSettings.BuildDirectory);
        }

        private static void BuildPlayer(BuildTarget target, string buildDirectory)
        {
            string[] scenePaths = GetScenePaths();
            string relativePath = AssembleName(target, buildDirectory);
            BuildReport buildReport = BuildPipeline.BuildPlayer(scenePaths, relativePath, target, BuildOptions.None);

            ParseBuildReport(buildReport);
        }

        private static string AssembleName(BuildTarget buildTarget, string buildDirectory)
        {
            string fileName = GetFilenameFromBuildTarget(buildTarget);
            StringBuilder stringBuilder = new StringBuilder();
            if (string.IsNullOrEmpty(fileName))
            {
                stringBuilder.AppendFormat("{0}{1}{2}", buildDirectory, Path.DirectorySeparatorChar.ToString(), buildTarget.ToString());
            }
            else
            {
                stringBuilder.AppendFormat("{0}{1}{2}{1}{3}", buildDirectory, Path.DirectorySeparatorChar.ToString(), buildTarget.ToString(), fileName);
            }
            return stringBuilder.ToString();
        }

        private static string GetFilenameFromBuildTarget(BuildTarget buildTarget)
        {
            string fileName = "";
            switch (buildTarget)
            {
                case BuildTarget.Android:
                    fileName = "ViewerInstaller.apk";
                    break;
                //iOS doesn't need the name set. XCode will do these for us. 
                case BuildTarget.iOS:
                    break;
                case BuildTarget.StandaloneOSX:
                    fileName = "ViewerInstaller.app";
                    break;
                case BuildTarget.StandaloneWindows:
                case BuildTarget.StandaloneWindows64:
                    fileName = "ViewerInstaller.exe";
                    break;
            }

            return fileName;
        }

        private static string[] GetScenePaths()
        {
            string[] scenes = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < scenes.Length; i++)
            {
                scenes[i] = EditorBuildSettings.scenes[i].path;
            }

            return scenes;
        }

        private static void ParseBuildReport(BuildReport buildReport)
        {
            if (buildReport.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError("[Builder] Build failed with result of " + buildReport.summary.result);
                Debug.LogError("[Builder] Number of errors found " + buildReport.summary.totalErrors);

                EditorApplication.Exit(1);
            }

            if(buildReport.summary.totalErrors > 0)
            {
                Debug.LogError("[ ~~ TESTING IF THIS MATCHES ~~ ]" + buildReport.summary.totalErrors);
            }
        }

        #region EditorDropdowns
        [MenuItem("Builder/Build Targets/Build Android")]
        private static void BuildForAndroid()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            BuildPlayer(BuildTarget.Android, BuilderConstants.DEFAULT_BUILD_DIRECTORY);
        }

        [MenuItem("Builder/Build Targets/Build iOS")]
        private static void BuildForIOS()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
            BuildPlayer(BuildTarget.iOS, BuilderConstants.DEFAULT_BUILD_DIRECTORY);
        }

        [MenuItem("Builder/Build Targets/Build OSX")]
        private static void BuildForOSX()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
            BuildPlayer(BuildTarget.StandaloneOSX, BuilderConstants.DEFAULT_BUILD_DIRECTORY);
        }

        [MenuItem("Builder/Build Targets/Build Win64")]
        private static void BuildForWin64()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
            BuildPlayer(BuildTarget.StandaloneWindows64, BuilderConstants.DEFAULT_BUILD_DIRECTORY);
        }
        #endregion
    }
}
