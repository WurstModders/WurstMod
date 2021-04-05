#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;
using WurstMod.UnityEditor.SceneExporters;

namespace WurstMod.UnityEditor
{
    public class ExporterWindow : EditorWindow
    {
        private static CustomScene _scene;

        [MenuItem("Wurst Mod/Export Window")]
        public static void Open()
        {
            _scene = GetRoot();
            SceneExporter.RefreshLoadedSceneExporters();
            GetWindow<ExporterWindow>("WurstMod Export");
        }

        private static CustomScene GetRoot()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            return roots.Length == 1 ? roots[0].GetComponent<CustomScene>() : null;
        }

        private void OnGUI()
        {
            // If scene is null, check again. Scene changes and exports can cause check to fail when it shouldn't.
            if (!_scene) _scene = GetRoot();

            if (!_scene)
            {
                GUILayout.Label("Could not find Custom Scene component on root game object.", EditorStyles.boldLabel);
                return;
            }

            // Draw the meta data fields
            GUILayout.Label("Scene metadata", EditorStyles.boldLabel);
            _scene.SceneName = EditorGUILayout.TextField("Scene Name", _scene.SceneName);
            _scene.Author = EditorGUILayout.TextField("Author", _scene.Author);
            _scene.Description = EditorGUILayout.TextField("Description", _scene.Description, GUILayout.MaxHeight(75));

            // Game mode stuff
            _scene.Gamemode = DrawGamemode(_scene.Gamemode);
            
            if (GUILayout.Button("Export Scene"))
            {
                var scene = SceneManager.GetActiveScene();
                var err = new ExportErrors();
                Exporter.Export(scene, err);
            }
        }

        private string DrawGamemode(string current)
        {
            // If we haven't yet registered any types, do it now.
            // This will need to be done when Unity reloads assemblies, so unfortunately more often than I'd like
            if (SceneExporter.RegisteredSceneExporters == null) SceneExporter.RefreshLoadedSceneExporters();
            var exporters = (SceneExporter.RegisteredSceneExporters ?? new SceneExporter[0]).ToArray();
            var choices = exporters.Select(x => x.GamemodeId).ToArray();

            var currentIndex = Array.IndexOf(choices, current);
            
            // If the game mode isn't valid, set it to zero and let the user re-pick
            if (currentIndex == -1) currentIndex = 0;
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gamemode");
            var newIndex = EditorGUILayout.Popup(currentIndex, choices);
            EditorGUILayout.EndHorizontal();
            if (currentIndex != newIndex) _scene.ExtraData = new CustomScene.StringKeyValue[0];
            _scene.ExtraData = exporters[newIndex].OnExporterGUI(_scene.ExtraData);

            return choices[newIndex];
        }
    }
}
#endif