using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents;
using WurstMod.MappingComponents.Generic;
using WurstMod.Shared;
using WurstMod.UnityEditor.SceneExporters;

namespace WurstMod.UnityEditor
{
    public static class Exporter
    {
        /// <summary>
        /// In exporting we have the following steps:
        /// Run ComponentProxy OnExport()
        /// Find & call the SceneExporter for the given game mode
        /// If no errors were returned, create the bundle
        /// </summary>
        public static void Export(Scene scene, ExportErrors err)
        {
            // Get the root objects in the scene
            var roots = scene.GetRootGameObjects();

            // Make sure there's only one and it's name is correct
            if (roots.Length != 1 || roots[0].name != Constants.RootObjectLevelName)
            {
                err.AddError($"You must only have one root object in your scene and it must be named '{Constants.RootObjectLevelName}'");
                return;
            }

            // Make sure it has the CustomScene component. If not, error out
            var sceneRoot = roots[0].GetComponent<CustomScene>();
            if (!sceneRoot)
            {
                err.AddError($"Your root object must have the {nameof(CustomScene)} component on it!");
                return;
            }

            // Find the exporter class. If none is found, error out
            SceneExporter.RefreshLoadedSceneExporters();
            var exporter = SceneExporter.GetExporterForGamemode(sceneRoot.Gamemode);
            if (exporter == null)
            {
                err.AddError($"Could not find an exporter class for the gamemode '{sceneRoot.Gamemode}'. Are you missing an assembly?");
                return;
            }

            // We have an exporter class, so let it handle the rest of validating the scene
            exporter.Validate(scene, sceneRoot, err);

            // Check for errors after validating
            if (err.HasErrors)
            {
                EditorUtility.DisplayDialog("Validation failed.", "There were errors while validating the scene. Check console for more details.", "Oops");
                return;
            }
            
            // Check for warnings, and give the option to continue
            if (err.HasWarnings)
            {
                var result = EditorUtility.DisplayDialog("Validation succeeded with warnings", "There were warnings while validating the scene. This will not prevent you from continuing, but it is recommended you cancel and correct them", "Continue", "Cancel");
                if (!result) return;
            }

            // Now the scene is validated we can export.
            exporter.Export();
            
            Debug.Log("Scene exported!");
        }
    }

    public class ExportErrors
    {
        // List fields to store the messages
        public readonly List<string> Debug = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> Errors = new List<string>();

        // Some boolean properties for easy checking
        public bool HasDebug => Debug.Count != 0;
        public bool HasErrors => Warnings.Count != 0;
        public bool HasWarnings => Errors.Count != 0;

        // Methods to add to the lists
        public void AddDebug(string message, Object context = null)
        {
            Debug.Add(message);
            UnityEngine.Debug.Log(message, context);
        }

        public void AddWarning(string message, Object context = null)
        {
            Warnings.Add(message);
            UnityEngine.Debug.LogWarning(message, context);
        }

        public void AddError(string message, Object context = null)
        {
            Errors.Add(message);
            UnityEngine.Debug.LogError(message, context);
        }
    }
}