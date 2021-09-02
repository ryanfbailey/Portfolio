using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace RFB.EditorUtility
{
    public static class BuildEditorUtility
    {
        // Auto build project
        [MenuItem("Tools/RFB/Build")]
        public static void Build()
        {
            // Get platform directory
            string buildDirectory = Application.dataPath.Replace("\\", "/").Replace("/Assets", "/Builds") + "/";
            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }

            // Get platform target
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;

            // File name
            string buildNameShort = Application.productName.Replace(" ", "").Replace("-", "").Replace(",", "");
            string buildName = buildNameShort;
            buildName += "_v" + PlayerSettings.bundleVersion;
            if (buildTarget == BuildTarget.StandaloneOSX)
            {
                buildName += ".app";
            }
            else if (buildTarget == BuildTarget.Android)
            {
                buildName += ".apk";
            }
            else if (buildTarget == BuildTarget.StandaloneWindows || buildTarget == BuildTarget.StandaloneWindows64)
            {
                buildName += "_Win/" + buildNameShort + ".exe";
            }
            else if (buildTarget == BuildTarget.StandaloneLinux64)
            {
                buildName += "_Lin/" + buildNameShort + ".x86_64";
            }
            else if (buildTarget == BuildTarget.iOS)
            {
                buildName += "_iOS/";
            }
            else if (buildTarget == BuildTarget.WebGL)
            {
                buildName += "_WebGL/";
            }

            // Build & Run if Possible
            string buildPath = buildDirectory + buildName;
            Debug.Log("Build Path\nPath: " + buildPath);
            BuildPipeline.BuildPlayer(EditorBuildSettings.scenes, buildPath, buildTarget, BuildOptions.None);
        }

        // Callback
        [PostProcessBuildAttribute(1)]
        public static void OnBuildComplete(BuildTarget target, string pathToBuiltProject)
        {
            // Get app version
            string appVersion = PlayerSettings.bundleVersion;
            // Increment
            appVersion = IncrementVersion(appVersion);
            // Set app version
            PlayerSettings.bundleVersion = appVersion;
            // Save
            AssetDatabase.SaveAssets();
        }

        // Return
        public static string IncrementVersion(string oldVersion, bool increment = true)
        {
            // Result
            string result = oldVersion;

            // Get last period
            int lastPeriod = oldVersion.LastIndexOf(".");
            if (lastPeriod != -1)
            {
                int last;
                string lastStr = result.Substring(lastPeriod + 1);
                if (int.TryParse(lastStr, out last))
                {
                    // Increment/Decrement
                    last += increment ? 1 : -1;

                    // Set
                    result = oldVersion.Substring(0, lastPeriod) + "." + last.ToString();
                    Debug.Log("Iterate\nFrom: " + oldVersion + "\nTo: " + result);
                }
            }

            // Return
            return result;
        }
    }
}
