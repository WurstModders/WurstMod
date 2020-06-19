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
    public enum LevelType { TNH, Generic };
    public class Exporter
    {
        // TODO Prevent naming objects based on whitelisted names.

        private static TNH.TNH_Level levelComponent;

        [MenuItem("H3VR/Export TNH")]
        public static void ExportTNH()
        {
            Export(LevelType.TNH);
        }

        [MenuItem("H3VR/Export Generic")]
        public static void ExportGeneric()
        {
            Export(LevelType.Generic);
        }

        private static void Export(LevelType type)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            List<string> warnings = new List<string>();
            string error = "";
            switch(type)
            {
                case LevelType.TNH: 
                    error = ValidateTNH(scene, warnings);
                    break;
                case LevelType.Generic:
                    error = ValidateGeneric(scene, warnings);
                    break;
            }
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
            // TODO Doesn't always work for some reason.
            levelComponent.skybox = RenderSettings.skybox;

            CreateBundle(scene);
        }

        private static void CreateBundle(Scene scene)
        {
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

        private static string ValidateTNH(Scene scene, List<string> warnings)
        {
            string error = "";
            error = CheckRoot(scene, warnings);
            if (error != "") return error;
            error = CheckScoreboard(scene, warnings);
            if (error != "") return error;
            error = CheckHoldPoints(scene, warnings);
            if (error != "") return error;
            error = CheckSupplyPoints(scene, warnings);
            if (error != "") return error;
            error = CheckNavmeshAndOcclusion(scene, warnings);
            if (error != "") return error;
            error = CheckForcedSpawn(scene, warnings);
            if (error != "") return error;

            return "";
        }

        private static string ValidateGeneric(Scene scene, List<string> warnings)
        {
            string error = "";
            error = CheckRoot(scene, warnings);
            if (error != "") return error;
            error = CheckNavmeshAndOcclusion(scene, warnings);
            if (error != "") return error;
            error = CheckSpawn(scene, warnings);
            if (error != "") return error;

            return "";
        }

        #region Validation Checks
        private static string CheckRoot(Scene scene, List<string> warnings)
        {
            // Must have exactly one properly formed "[TNHLEVEL]" or "[LEVEL]" object at the root.
            GameObject[] roots = EditorSceneManager.GetActiveScene().GetRootGameObjects();
            if (roots.Length != 1 || (roots[0].name != "[TNHLEVEL]" && roots[0].name != "[LEVEL]"))
            {
                return "You must have a single object named [LEVEL] at the root of the scene. All other objects must be children of this object.";
            }
            levelComponent = roots[0].GetComponent<TNH.TNH_Level>();
            if (levelComponent == null)
            {
                return "You must add a TNH_Level component to [LEVEL] and set your level's name, author, and description.";
            }
            if (levelComponent.levelName == "" || levelComponent.levelAuthor == "" || levelComponent.levelDescription == "")
            {
                string warn = "WARNING: You didn't set one of the fields on [LEVEL]! Please add your level's name, author, and description.";
                Debug.LogWarning(warn);
                warnings.Add(warn);
            }

            // levelName and levelAuthor cannot contain newlines.
            if (levelComponent.levelName.Contains('\n') || levelComponent.levelAuthor.Contains('\n'))
            {
                return "Level Name and Level Author cannot contain newlines.";
            }

            return "";
        }

        private static string CheckScoreboard(Scene scene, List<string> warnings)
        {
            // Must have exactly one scoreboard area.
            TNH.ScoreboardArea[] sb = levelComponent.GetComponentsInChildren<TNH.ScoreboardArea>();
            if (sb.Length != 1)
            {
                return "You must have exactly one Scoreboard Area.";
            }
            return "";
        }

        private static string CheckHoldPoints(Scene scene, List<string> warnings)
        {
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
            return "";
        }

        private static string CheckSupplyPoints(Scene scene, List<string> warnings)
        {
            // UNVERIFIED Must have at least 3 Supply Points.
            TNH.TNH_SupplyPoint[] supplies = levelComponent.GetComponentsInChildren<TNH.TNH_SupplyPoint>();
            if (supplies.Length < 3)
            {
                return "You must have at least three Supply Points.";
            }
            return "";
        }

        private static string CheckNavmeshAndOcclusion(Scene scene, List<string> warnings)
        {
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

        private static string CheckForcedSpawn(Scene scene, List<string> warnings)
        {
            // Cannot have more than one ForcedSpawn component
            if (levelComponent.GetComponentsInChildren<TNH.Extras.ForcedSpawn>().Length > 1)
            {
                return "You can only have one Supply Point with the ForcedSpawn component.";
            }
            return "";
        }

        private static string CheckSpawn(Scene scene, List<string> warnings)
        {
            if (levelComponent.GetComponentsInChildren<Generic.Spawn>().Length != 1)
            {
                return "You must have exactly one Spawnpoint prefab in a generic level.";
            }
            return "";
        }
        #endregion
    }
}
