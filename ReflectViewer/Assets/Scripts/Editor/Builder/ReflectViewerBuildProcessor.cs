using System;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Unity.Reflect.Viewer.Builder
{
    internal class ReflectViewerBuildProcessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => -2;

        public void OnPreprocessBuild(BuildReport report)
        {
            Debug.Log("ReflectViewerBuildProcessor:OnPreprocessBuild");
            if (report.summary.platform.Equals(BuildTarget.StandaloneOSX))
            {
                Debug.Log("OSXBuildPreProcess - removing unsupported Oculus Loader");
                var XRSettingsRelativePath = "XR/XRGeneralSettings.asset";
                var XRSettings = $"{Application.dataPath}/{XRSettingsRelativePath}";

                // Read XR Settings
                var XRSettingsYaml = System.IO.File.ReadAllText(XRSettings);

                var oculusFileReference1 = "- {fileID: 11400000, guid: c6a8f50e61834ef4ab895401f15d2678, type: 2}\n";
                var oculusFileReference2 = "- {fileID: 11400000, guid: 529013ebd787a4d488dc738f97cea387, type: 2}\n";
                var settingSeparator = "MonoBehaviour:";
                var XRStandaloneProviderKeyName = "m_Name: Standalone Providers";

                // If oculus file reference are found, remove them
                if (XRSettingsYaml.IndexOf(oculusFileReference1, StringComparison.InvariantCulture) != -1)
                {
                    // Look for Standalone Providers item
                    var splitSettings = XRSettingsYaml.Split(new string[] {settingSeparator}, StringSplitOptions.None);
                    var newSettings = new StringBuilder();
                    for (var i = 0; i < splitSettings.Length; i++)
                    {
                        var setting = splitSettings[i];
                        if (setting.IndexOf(XRStandaloneProviderKeyName, StringComparison.InvariantCulture) != -1)
                        {
                            setting = setting.Replace("m_Loaders:\n", "m_Loaders: []\n");
                            setting = setting.Replace(oculusFileReference1, "");
                            setting = setting.Replace(oculusFileReference2, "");
                        }

                        if (i > 0)
                        {
                            newSettings.Append(settingSeparator);
                        }

                        newSettings.Append(setting);
                    }
                    System.IO.File.WriteAllText(XRSettings, newSettings.ToString());
                    AssetDatabase.Refresh();
                }
            }

            // Execute this code before Any other builds except Windows for removing VR Packages.
            if (!report.summary.platform.Equals(BuildTarget.StandaloneWindows) &&
                !report.summary.platform.Equals(BuildTarget.StandaloneWindows64))
            {
                string[] packagesToRemove = {"com.unity.xr.oculus", "com.valvesoftware.unity.openvr"};
                foreach (var packageToRemove in packagesToRemove)
                {
                    var removeRequest = Client.Remove(packageToRemove);

                    while (!removeRequest.IsCompleted)
                    {
                    }

                    if (removeRequest.Status == StatusCode.Success)
                    {
                        Debug.Log($"remove status : {removeRequest.Status} , removed package {removeRequest.PackageIdOrName}");
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
}
