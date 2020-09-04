using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents;
using WurstMod.MappingComponents.Generic;
using WurstMod.Shared;
using Object = UnityEngine.Object;

namespace WurstMod.UnityEditor.SceneExporters
{
    public class SceneExporter
    {
        private Scene _scene;
        private CustomScene _root;
        private ExportErrors _err;
        
        /// <summary>
        /// This is called to validate the scene and ensure it can be exported.
        /// <strong>If you override this, make sure to call <code>base.Validate(scene)</code>!</strong>
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
            if (Directory.Exists(location)) Directory.Delete(location, true);

            // Create a LevelInfo object and save it
            Debug.Log("Writing level info...");
            var levelInfo = new LevelInfo
            {
                Location = location,
                SceneName = _root.SceneName,
                Author = _root.Author,
                Gamemode = _root.Gamemode,
                Description = _root.Description
            };
            levelInfo.ToFile();

            // Export the asset bundle
            Debug.Log("Creating asset bundle...");
            BuildPipeline.BuildAssetBundles(levelInfo.Location, new[] {build}, buildOptions, BuildTarget.StandaloneWindows64);

            // Delete unnecessary files.
            Debug.Log("Cleaning up...");
            var toDelete = new[]
            {
                Path.Combine(levelInfo.Location, $"{Constants.FilenameLevelData}.manifest"),
                Path.Combine(levelInfo.Location, _scene.name),
                Path.Combine(levelInfo.Location, $"{_scene.name}.manifest")
            };
            foreach (var file in toDelete)
                if (File.Exists(file)) File.Delete(file);
        }

        /// <summary>
        /// Simple helper function to check if the number of components of a certain type is between two numbers
        /// </summary>
        /// <param name="min">The minimum number</param>
        /// <param name="max">The maximum number</param>
        /// <param name="warning">If invalid, produces a warning instead of an error</param>
        /// <param name="message">Custom message to log</param>
        /// <typeparam name="T">The type of the component</typeparam>
        protected void RequiredComponents<T>(int min, int max, bool warning = false, string message = null)
        {
            var count = _root.GetComponentsInChildren<T>().Length;
            if (min <= count && count <= max) return;

            var msg = min == max ? $"{min}" : $"{min} - {max}";
            if (warning) _err.AddWarning(message ?? $"Your scene contains {count} {nameof(T)}. Recommended number is {msg}");
            else _err.AddError(message ?? $"Your scene contains {count} {nameof(T)}. Required number is {msg}");
        }
        
        /// <summary>
        /// Returns an exporter object for the provided game mode
        /// </summary>
        /// <param name="gamemode">The gamemode</param>
        /// <returns>The exporter object</returns>
        public static SceneExporter GetExporterForGamemode(string gamemode)
        {
            // Get a list of all types in the app domain that derive from SceneExporter
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    // Try to find types in all assemblies
                    types.AddRange(assembly.GetTypes().Where(t => t.IsSubclassOf(typeof(SceneExporter))));
                }
                catch (ReflectionTypeLoadException e)
                {
                    // If we hit this exception it means there were types that Unity does not like.
                    // However, we are still given a partial list of loaded types, so continue with that
                    types.AddRange(e.Types.Where(t => t != null && t.IsSubclassOf(typeof(SceneExporter))));
                }
            }

            // Magic LINQ statement to select the first type that has the
            // SceneExporterAttribute with a gamemode that matches the gamemode parameter
            return
            (
                from type in types
                let attributes = type.GetCustomAttributes(typeof(SceneExporterAttribute), false)
                where attributes.Length != 0
                where ((SceneExporterAttribute) attributes[0]).Gamemode == gamemode
                select (SceneExporter) Activator.CreateInstance(type)
            ).FirstOrDefault();
        }
    }

    public class SceneExporterAttribute : Attribute
    {
        public string Gamemode { get; }
        public SceneExporterAttribute(string gamemode) => Gamemode = gamemode;
    }
}