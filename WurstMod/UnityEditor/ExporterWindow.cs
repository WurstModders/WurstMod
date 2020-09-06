using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;

namespace WurstMod.UnityEditor
{
    public class ExporterWindow : EditorWindow
    {
        [MenuItem("Wurst Mod/Export Window")]
        public static void Open()
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            _scene = roots.Length == 1 ? roots[0].GetComponent<CustomScene>() : null;
            GetWindow<ExporterWindow>();
        }

        private static CustomScene _scene;

        private void OnGUI()
        {
            if (!_scene)
            {
                GUILayout.Label("Could not find Custom Scene component on root game object.", EditorStyles.boldLabel);
                return;
            }

            // Draw the meta data fields
            GUILayout.Label("Scene metadata", EditorStyles.boldLabel);
            _scene.SceneName = EditorGUILayout.TextField("Scene Name", _scene.SceneName);
            _scene.Author = EditorGUILayout.TextField("Author", _scene.Author);
            _scene.Gamemode = EditorGUILayout.TextField("Gamemode", _scene.Gamemode);
            _scene.Description = EditorGUILayout.TextField("Description", _scene.Description, GUILayout.MaxHeight(75));

            if (GUILayout.Button("Export Scene"))
            {
                var scene = SceneManager.GetActiveScene();
                var err = new ExportErrors();
                Exporter.Export(scene, err);
            }
        }
    }
}