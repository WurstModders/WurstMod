using FistVR;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR.InteractionSystem;
using WurstMod.MappingComponents;
using WurstMod.MappingComponents.Generic;
using WurstMod.Shared;

namespace WurstMod.UnityEditor.SceneExporters
{
    public abstract class SceneExporter
    {
        private Scene _scene;
        private CustomScene _root;
        private ExportErrors _err;

        /// <summary>
        /// This is implemented by the deriving class and is a unique identifier for a game mode.
        /// </summary>
        public abstract string GamemodeId { get; }

        /// <summary>
        /// This is called to validate the scene and ensure it can be exported.
        /// <strong>If you override this, make sure to call <code>base.Validate(scene, root, err)</code>! (Preferably before your code)</strong>
        /// Also, do NOT return early from this method unless absolutely necessary. Validate as many thing as you can.
        /// </summary>
        /// <param name="scene">The scene to export</param>
        /// <param name="root">The root game object</param>
        /// <param name="err">An object used to inform the exporter what's happened</param>
        public virtual void Validate(Scene scene, CustomScene root, ExportErrors err)
        {
            // Save these for later.
            _scene = scene;
            _root = root;
            _err = err;

            // Check for NavMesh and Occlusion data
            // These aren't *required* so they will only be warnings
            if (!File.Exists(Path.GetDirectoryName(scene.path) + "/" + scene.name + "/NavMesh.asset")) err.AddWarning("Scene is missing NavMesh data!");
            if (!File.Exists(Path.GetDirectoryName(scene.path) + "/" + scene.name + "/OcclusionCullingData.asset")) err.AddWarning("Scene is missing Occlusion Culling Data!");

            // Let the proxied components know we're about to export
            foreach (var proxy in scene.GetRootGameObjects().SelectMany(x => x.GetComponentsInChildren<ComponentProxy>())) proxy.OnExport(err);
        }

        /// <summary>
        /// This is called by the exporter window to let the game mode include custom data
        /// IMPORTANT: You should probably check that the array is of correct length.
        /// </summary>
        public virtual CustomScene.StringKeyValue[] OnExporterGUI(CustomScene.StringKeyValue[] customData)
        {
            if (customData == null || customData.Length != 0) customData = new CustomScene.StringKeyValue[0];
            return customData;
        }
        
        /// <summary>
        /// This method is called after validate and will actually export the scene to files.
        /// </summary>
        public virtual void Export()
        {
            // Make sure the user saves their scene first
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Configure the build
            var buildOptions = BuildAssetBundleOptions.ChunkBasedCompression;
            var build = default(AssetBundleBuild);
            build.assetBundleName = Constants.FilenameLevelData;
            build.assetNames = new[] {_scene.path};

            // Create a temporary folder to hold the build so if it fails nothing gets overwritten
            var location = $"AssetBundles/{_scene.name}/";

            // Delete the folder if it exists already
            Debug.Log("Removing previous export...");
            if (Directory.Exists(location))
            {
                foreach (string path in Directory.GetFiles(location, "*.*", SearchOption.AllDirectories))
                {
                    File.Delete(path);
                }
            }

            // Create a LevelInfo object and save it
            Debug.Log("Writing level info...");
            var levelInfo = new LevelInfo
            {
                SceneName = _root.SceneName,
                Author = _root.Author,
                Gamemode = _root.Gamemode,
                Description = _root.Description
            };
            levelInfo.ToFile(location);

            // Export the asset bundle
            Debug.Log("Creating asset bundle...");
            BuildPipeline.BuildAssetBundles(location, new[] {build}, buildOptions, BuildTarget.StandaloneWindows64);

            // Delete unnecessary files.
            Debug.Log("Cleaning up...");
            var toDelete = new[]
            {
                Path.Combine(location, $"{Constants.FilenameLevelData}.manifest"),
                Path.Combine(location, _scene.name),
                Path.Combine(location, $"{_scene.name}.manifest")
            };
            foreach (var file in toDelete)
                if (File.Exists(file))
                    File.Delete(file);
        }

        /// <summary>
        /// Simple helper function to check if the number of components of a certain type is between two numbers
        /// </summary>
        /// <param name="min">The minimum number</param>
        /// <param name="max">The maximum number</param>
        /// <param name="warning">If invalid, produces a warning instead of an error</param>
        /// <param name="message">Custom message to log</param>
        /// <typeparam name="T">The type of the component</typeparam>
        protected void RequiredComponents<T>(int min, int max = int.MaxValue, bool warning = false, string message = null)
        {
            var count = _root.GetComponentsInChildren<T>().Length;
            if (min <= count && count <= max) return;

            var msg = min == max ? $"{min}" : max == int.MaxValue ? $"at least {min}" : $"{min} - {max}";
            if (warning) _err.AddWarning(message ?? $"Your scene contains {count} {typeof(T).Name}. Recommended number is {msg}");
            else _err.AddError(message ?? $"Your scene contains {count} {typeof(T).Name}. Required number is {msg}");
        }


        // Array of currently registered scene exporters
        public static SceneExporter[] RegisteredSceneExporters;

        /// <summary>
        /// Simple method to re-discover and instantiate scene exporters.
        /// </summary>
        public static void RefreshLoadedSceneExporters() => RegisteredSceneExporters = EnumerateExporterTypes().Select(x => Activator.CreateInstance(x) as SceneExporter).ToArray();

        /// <summary>
        /// Enumerates all scene exporters across all loaded assemblies. You should probably use the
        /// RegisteredSceneLoaders variable instead of this as it's expensive.
        /// </summary>
        /// <returns>All scene exporters across all loaded assemblies</returns>
        public static IEnumerable<Type> EnumerateExporterTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypesSafe())
                .Where(x => x.IsSubclassOf(typeof(SceneExporter)));
        }

        /// <summary>
        /// Returns an exporter object for the provided game mode
        /// </summary>
        /// <param name="gamemode">The gamemode</param>
        /// <returns>The exporter object</returns>
        public static SceneExporter GetExporterForGamemode(string gamemode) => RegisteredSceneExporters.FirstOrDefault(x => x.GamemodeId == gamemode);
    }
}