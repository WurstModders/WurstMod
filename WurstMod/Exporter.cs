using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


namespace WurstMod
{
    public class Exporter
    {
        private static TNH.TNH_Level levelComponent;

        [MenuItem("H3VR/Export TNH")]
        public static void ExportBundle()
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            List<string> warnings = new List<string>();
            string error = Validate(scene, warnings);
            if (error != "")
            {
                Debug.LogError(error);
                EditorUtility.DisplayDialog("Error during Export", error, "OK");
                return;
            }
            if (warnings.Count != 0)
            {
                bool choice = EditorUtility.DisplayDialog("Warning(s)", string.Join("\n", warnings.ToArray()) + "\n\nDo you want to continue anyway?", "Yes", "No");
                if (!choice) return;
            }

            // Pre-save, grab the skybox.
            levelComponent.skybox = RenderSettings.skybox;

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // Setup build options.
                BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
                AssetBundleBuild build = default(AssetBundleBuild);
                build.assetBundleName = "leveldata";
                build.assetNames = new string[] { scene.path };
                string directory = "AssetBundles/" + scene.name + "/";

                // Create directory if it doesn't exist.
                Directory.CreateDirectory(directory);

                // Nuke files if already exist because asset bundles are fickle.
                if (File.Exists(directory + "leveldata")) File.Delete(directory + "leveldata");
                if (File.Exists(directory + "leveldata.MANIFEST")) File.Delete(directory + "leveldata.MANIFEST");
                if (File.Exists(directory + scene.name)) File.Delete(directory + scene.name);
                if (File.Exists(directory + scene.name + ".MANIFEST")) File.Delete(directory + scene.name + ".MANIFEST");

                // Export
                BuildPipeline.BuildAssetBundles(directory, new AssetBundleBuild[] { build }, buildOptions, BuildTarget.StandaloneWindows64);

                // Create name/author/desc file.
                StringBuilder strb = new StringBuilder();
                strb.AppendLine(levelComponent.levelName);
                strb.AppendLine(levelComponent.levelAuthor);
                strb.AppendLine(levelComponent.levelDescription);
                File.WriteAllText(directory + "info.txt", strb.ToString());

                // Delete unnecessary files.
                if (File.Exists(directory + "leveldata.MANIFEST")) File.Delete(directory + "leveldata.MANIFEST");
                if (File.Exists(directory + scene.name)) File.Delete(directory + scene.name);
                if (File.Exists(directory + scene.name + ".MANIFEST")) File.Delete(directory + scene.name + ".MANIFEST");
            }
        }

        public static string Validate(Scene scene, List<string> warnings)
        {
            // Must have exactly one properly formed "[TNHLEVEL]" object at the root.
            GameObject[] roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            if (roots.Length != 1 || roots[0].name != "[TNHLEVEL]")
            {
                return "You must have a single object named [TNHLEVEL] at the root of the scene. All other objects must be children of this object.";
            }
            levelComponent = roots[0].GetComponent<TNH.TNH_Level>();
            if (levelComponent == null)
            {
                return "You must add a TNH_Level component to [TNHLEVEL] and set your level's name, author, and description.";
            }
            if (levelComponent.levelName == "" || levelComponent.levelAuthor == "" || levelComponent.levelDescription == "")
            {
                string warn = "WARNING: You didn't set one of the fields on [TNHLEVEL]! Please add your level's name, author, and description.";
                Debug.LogWarning(warn);
                warnings.Add(warn);
            }

            // levelName and levelAuthor cannot contain newlines.
            if (levelComponent.levelName.Contains('\n') || levelComponent.levelAuthor.Contains('\n'))
            {
                return "Level Name and Level Author cannot contain newlines.";
            }

            // Must have exactly one scoreboard area.
            TNH.ScoreboardArea[] sb = levelComponent.GetComponentsInChildren<TNH.ScoreboardArea>();
            if (sb.Length != 1)
            {
                return "You must have exactly one Scoreboard Area.";
            }

            // UNVERIFIED Must have at least 2 Hold Points.
            TNH.TNH_HoldPoint[] holds = levelComponent.GetComponentsInChildren<TNH.TNH_HoldPoint>();
            if (holds.Length < 2)
            {
                return "You must have at least two Hold Points.";
            }
            // Hold points must have at least 9 defenders spawnpoints.
            // NavBlockers must be disabled.
            foreach (TNH.TNH_HoldPoint hold in holds)
            {
                if (hold.SpawnPoints_Sosigs_Defense.AsEnumerable().Count() < 9)
                    return "All holds must have at least 9 entries in SpawnPoints_Sosigs_Defense.";
                if (hold.NavBlockers.activeSelf)
                    return "All NavBlockers must be disabled before exporting.";
            }

            // UNVERIFIED Must have at least 3 Supply Points.
            TNH.TNH_SupplyPoint[] supplies = levelComponent.GetComponentsInChildren<TNH.TNH_SupplyPoint>();
            if (supplies.Length < 3)
            {
                return "You must have at least three Supply Points.";
            }

            // Check for Navmesh and Occlusion data.
            if (!File.Exists(Path.GetDirectoryName(scene.path) + "/" + scene.name + "/NavMesh.asset"))
            {
                string warn = "WARNING: Scene does not contain Navmesh data!";
                Debug.LogWarning(warn);
                warnings.Add(warn);
            }
            if (!File.Exists(Path.GetDirectoryName(scene.path) + "/" + scene.name + "/OcclusionCullingData.asset"))
            {
                string warn = "WARNING: Scene does not contain Occlusion culling data!";
                Debug.LogWarning(warn);
                warnings.Add(warn);
            }

            return "";
        }
    }
}
