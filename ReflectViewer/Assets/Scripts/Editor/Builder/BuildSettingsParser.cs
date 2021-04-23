namespace Unity.Reflect.Viewer.Builder
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using UnityEngine;
    using Unity.Reflect.Viewer.UI;

    public class BuildSettingsParser
    {
        private Dictionary<string, string> commandLineArgsByKey;
        //During batchmode, we can't rely on EditorUserBuildSettings.activeBuildSettings
        private BuildTarget activeBuildTarget;
        private string buildDirectory;
        public BuildTarget ActiveBuildTarget
        {
            get { return activeBuildTarget; }
        }

        public string BuildDirectory
        {
            get
            {
                return buildDirectory;
            }
        }

        public void StartParsing()
        {
            Initialize();
            ParseArguments();
            ActOnHandledArguments();
        }

        private void Initialize()
        {
            commandLineArgsByKey = new Dictionary<string, string>();
            activeBuildTarget = BuildTarget.NoTarget;
            buildDirectory = BuilderConstants.DEFAULT_BUILD_DIRECTORY;
        }

        private void ParseArguments()
        {
            string[] commandLinesArgs = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < commandLinesArgs.Length - 1; ++i)
            {
                string argument = commandLinesArgs[i];
                if (argument.StartsWith("-") && !commandLinesArgs[i + 1].StartsWith("-"))
                {
                    commandLineArgsByKey.Add(argument, commandLinesArgs[i + 1].ToLower());
                }
            }
        }

        private void ActOnHandledArguments()
        {
            if (commandLineArgsByKey.ContainsKey(BuilderConstants.BUILD_TARGET))
            {
                SwitchToRespectiveBuildTarget(commandLineArgsByKey[BuilderConstants.BUILD_TARGET]);
            }
            if (commandLineArgsByKey.ContainsKey(BuilderConstants.VIEWER_VERSION))
            {
                SetVersion(commandLineArgsByKey[BuilderConstants.VIEWER_VERSION]);
            }
            if (commandLineArgsByKey.ContainsKey(BuilderConstants.OUTPUT_PATH))
            {
                buildDirectory = commandLineArgsByKey[BuilderConstants.OUTPUT_PATH];
            }            
        }

        private void SwitchToRespectiveBuildTarget(string buildTarget)
        {
            switch (buildTarget)
            {
                case BuilderConstants.ANDROID_BUILD_TARGET:
                    activeBuildTarget = BuildTarget.Android;
                    break;
                case BuilderConstants.IOS_BUILD_TARGET:
                    activeBuildTarget = BuildTarget.iOS;
                    break;
                case BuilderConstants.OSX_BUILD_TARGET:
                    activeBuildTarget = BuildTarget.StandaloneOSX;
                    break;
                case BuilderConstants.WIN_BUILD_TARGET:
                    activeBuildTarget = BuildTarget.StandaloneWindows;
                    break;
                default:
                    throw new Exception("[BuildSettingsParser] BUILD FAILED. Invalid Build target: " + buildTarget);
            }
        }

        private void SetVersion(string versionArgument)
        {
            Version version;
            if (Version.TryParse(versionArgument, out version))
            {
                int bundleVersionCode = GetBundleVersionFromVersionString(versionArgument);
                PlayerSettings.bundleVersion = versionArgument;
                switch (activeBuildTarget)
                {
                    case BuildTarget.Android:
                        PlayerSettings.Android.bundleVersionCode = bundleVersionCode;
                        break;
                    case BuildTarget.iOS:
                        PlayerSettings.iOS.buildNumber = versionArgument;
                        break;
                    case BuildTarget.StandaloneOSX:
                        PlayerSettings.macOS.buildNumber = versionArgument;
                        break;
                }

            }
            else
            {
                Debug.LogWarning("[BuildSettingsParser] Could not parse a valid version. Forcing a development build.");
                EditorUserBuildSettings.development = true;
            }
        }

        private int GetBundleVersionFromVersionString(string version)
        {
            int bundleCode = int.Parse(version.Replace(".", ""));
            return bundleCode;
        }

        private bool AllDeltaDNAArgsPresent()
        {
            return commandLineArgsByKey.ContainsKey(BuilderConstants.DELTA_DNA_BASE_URL)
                && commandLineArgsByKey.ContainsKey(BuilderConstants.DELTA_DNA_DEV_URL)
                && commandLineArgsByKey.ContainsKey(BuilderConstants.DELTA_DNA_LIVE_URL);
        }
    }
}
