using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents;
using WurstMod.MappingComponents.Generic;
using WurstMod.MappingComponents.Sandbox;
using WurstMod.MappingComponents.TakeAndHold;
using WurstMod.Shared;
using LevelInfo = WurstMod.Shared.LevelInfo;

namespace WurstMod.UnityEditor
{
    public enum LevelType { TNH, Generic };
    public class Exporter
    {
        // TODO Prevent naming objects based on whitelisted names.

        private static CustomScene _customSceneComponent;

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
            
            // Let the proxied components know we're about to export
            foreach (var proxy in scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ComponentProxy>())) proxy.OnExport();
            
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

            

            CreateBundle(scene, type);
        }

        private static void CreateBundle(Scene scene, LevelType type)
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                // Setup build options.
                BuildAssetBundleOptions buildOptions = BuildAssetBundleOptions.UncompressedAssetBundle;
                AssetBundleBuild build = default(AssetBundleBuild);
                build.assetBundleName = "leveldata";
                build.assetNames = new string[] { scene.path };
                string prefix = type.ToString() + "-";
                string directory = "AssetBundles/" + prefix + scene.name + "/";

                // Create directory if it doesn't exist.
                Directory.CreateDirectory(directory);

                // Nuke files if already exist because asset bundles are fickle.
                if (File.Exists(directory + "leveldata")) File.Delete(directory + "leveldata");
                if (File.Exists(directory + "leveldata.MANIFEST")) File.Delete(directory + "leveldata.MANIFEST");
                if (File.Exists(directory + prefix + scene.name)) File.Delete(directory + prefix + scene.name);
                if (File.Exists(directory + prefix + scene.name + ".MANIFEST")) File.Delete(directory + prefix + scene.name + ".MANIFEST");

                // Export
                BuildPipeline.BuildAssetBundles(directory, new AssetBundleBuild[] { build }, buildOptions, BuildTarget.StandaloneWindows64);

                // Create name/author/desc file.
                // TODO: Update this to use a JSON file and include the game mode
                var sb = new StringBuilder();
                sb.AppendLine(_customSceneComponent.SceneName);
                sb.AppendLine(_customSceneComponent.Author);
                sb.AppendLine(_customSceneComponent.Description);
                File.WriteAllText(directory + "info.txt", sb.ToString());

                // Delete unnecessary files.
                if (File.Exists(directory + "leveldata.MANIFEST")) File.Delete(directory + "leveldata.MANIFEST");
                if (File.Exists(directory + prefix + scene.name)) File.Delete(directory + prefix + scene.name);
                if (File.Exists(directory + prefix + scene.name + ".MANIFEST")) File.Delete(directory + prefix + scene.name + ".MANIFEST");
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
            _customSceneComponent = roots[0].GetComponent<CustomScene>();
            if (_customSceneComponent == null)
            {
                return "You must add a TNH_Level component to [LEVEL] and set your level's name, author, and description.";
            }
            
            if (_customSceneComponent.SceneName == "" || _customSceneComponent.Author == "" || _customSceneComponent.Description == "")
            {
                string warn = "WARNING: You didn't set one of the fields on [LEVEL]! Please add your level's name, author, and description.";
                Debug.LogWarning(warn);
                warnings.Add(warn);
            }

            // levelName and levelAuthor cannot contain newlines.
            if (_customSceneComponent.SceneName.Contains('\n') || _customSceneComponent.Author.Contains('\n'))
            {
                return "Level Name and Level Author cannot contain newlines.";
            }

            // Warn empty skybox.
            if (_customSceneComponent.Skybox == null && RenderSettings.skybox != null)
            {
                string warn = "WARNING: You didn't set your skybox on [LEVEL]!";
                Debug.LogWarning(warn);
                warnings.Add(warn);
            }

            return "";
        }

        private static string CheckScoreboard(Scene scene, List<string> warnings)
        {
            // Must have exactly one scoreboard area.
            ScoreboardArea[] sb = _customSceneComponent.GetComponentsInChildren<ScoreboardArea>();
            if (sb.Length != 1)
            {
                return "You must have exactly one Scoreboard Area.";
            }
            return "";
        }

        private static string CheckHoldPoints(Scene scene, List<string> warnings)
        {
            // UNVERIFIED Must have at least 2 Hold Points.
            TNH_HoldPoint[] holds = _customSceneComponent.GetComponentsInChildren<TNH_HoldPoint>();
            if (holds.Length < 2)
            {
                return "You must have at least two Hold Points.";
            }
            // Hold points must have at least 9 defenders spawnpoints.
            // NavBlockers must be disabled.
            foreach (TNH_HoldPoint hold in holds)
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
            TNH_SupplyPoint[] supplies = _customSceneComponent.GetComponentsInChildren<TNH_SupplyPoint>();
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
            if (_customSceneComponent.GetComponentsInChildren<ForcedSpawn>().Length > 1)
            {
                return "You can only have one Supply Point with the ForcedSpawn component.";
            }
            return "";
        }

        private static string CheckSpawn(Scene scene, List<string> warnings)
        {
            if (_customSceneComponent.GetComponentsInChildren<Spawn>().Length != 1)
            {
                return "You must have exactly one Spawnpoint prefab in a generic level.";
            }
            return "";
        }
        #endregion
    }
}
