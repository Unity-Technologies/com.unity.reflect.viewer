using System;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEditor.XR.Management;

namespace Unity.Reflect.Viewer.Builder
{
    internal class ReflectViewerBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -101;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (!report.summary.platform.Equals(BuildTarget.StandaloneWindows) &&
                !report.summary.platform.Equals(BuildTarget.StandaloneWindows64))
            {
                var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
                var settingManager = generalSettings.Manager;
                settingManager.loaders.Clear();
            }
        }
    }
}
