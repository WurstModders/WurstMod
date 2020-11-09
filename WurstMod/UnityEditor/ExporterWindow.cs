using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
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
            GetWindow<ExporterWindow>();
        }

        private static CustomScene GetRoot()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            return roots.Length == 1 ? roots[0].GetComponent<CustomScene>() : null;
        }

        private void OnGUI()
        {
            if (!_scene)
            {
                // Check again, scene changes and exports can cause check to fail when it shouldn't.
                _scene = GetRoot();
            }

            if (!_scene)
            {
                GUILayout.Label("Could not find Custom Scene component on root game object.", EditorStyles.boldLabel);
                return;
            }

            // Draw the meta data fields
            GUILayout.Label("Scene metadata", EditorStyles.boldLabel);
            _scene.SceneName = EditorGUILayout.TextField("Scene Name", _scene.SceneName);
            _scene.Author = EditorGUILayout.TextField("Author", _scene.Author);
            //_scene.Gamemode = EditorGUILayout.TextField("Gamemode", _scene.Gamemode);
            _scene.Gamemode = DrawGamemode(_scene.Gamemode);
            _scene.Description = EditorGUILayout.TextField("Description", _scene.Description, GUILayout.MaxHeight(75));

            if (GUILayout.Button("Export Scene"))
            {
                var scene = SceneManager.GetActiveScene();
                var err = new ExportErrors();
                Exporter.Export(scene, err);
            }
        }

        private string DrawGamemode(string current)
        {
            string[] choices = SceneExporter.GetAllExporters()
                .Select(x => Activator.CreateInstance(x) as SceneExporter)
                .Select(x => x.GamemodeId)
                .ToArray();

            int currentIndex = Array.IndexOf(choices, current);
            int newIndex;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Gamemode");
            newIndex = EditorGUILayout.Popup(currentIndex, choices);
            EditorGUILayout.EndHorizontal();

            return choices[newIndex];
        }
    }
}