#if !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Deli.VFS;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents;
using WurstMod.MappingComponents.Generic;
using WurstMod.Shared;
using Object = UnityEngine.Object;

namespace WurstMod.Runtime
{
    /// <summary>
    /// WurstMod loading class. Responsible for loading a map.
    /// </summary>
    public static class Loader
    {
        public static bool IsLoadInProgress;

        /// <summary>
        /// This is called by the BepInEx entrypoint. It is fired when ANY scene loads, including vanilla ones.
        /// </summary>
        public static IEnumerator OnSceneLoad(Scene scene)
        {
            if (!IsLoadInProgress && LevelToLoad.HasValue)
            {
                // We're loading the base scene for a modded scene
                IsLoadInProgress = true;
                
                // Find references
                ObjectReferences.FindReferences(scene);
                
                // Load the level
                var level = LevelToLoad.Value;
                yield return LoadCustomScene(level);
            }
            
            if (IsLoadInProgress && LevelToLoad.HasValue)
            {
                IsLoadInProgress = false;
            }
        }

        /// <summary>
        /// This method is called to load a custom scene 
        /// </summary>
        private static IEnumerator LoadCustomScene(LevelInfo level)
        {
            // Step 0: Get the loaded scene and find any custom loaders and scene patchers
            _loadedScene = SceneManager.GetActiveScene();
            var sceneLoader = CustomSceneLoader.GetSceneLoaderForGamemode(level.Gamemode);
            if (sceneLoader == null)
            {
                Debug.LogError($"No SceneLoader found for the gamemode '{level.Gamemode}'! Cannot load level.");
                yield break;
            }

            // Step 1: Let the custom loaders do their PRELOAD method
            sceneLoader.PreLoad();

            // Step 2: Disable all currently active objects.
            var disabledObjects = new List<GameObject>();
            var objects = _loadedScene.GetRootGameObjects();
            foreach (var gameObject in objects)
            {
                if (!gameObject.activeSelf) continue;
                disabledObjects.Add(gameObject);
                gameObject.SetActive(false);
            }

            // Step 3: Destroy all unused objects
            foreach (var gameObject in objects.SelectMany(o => o.GetComponentsInChildren<Transform>()))
            foreach (var filter in sceneLoader.EnumerateDestroyOnLoad())
                if (gameObject.name.Contains(filter))
                    Object.Destroy(gameObject.gameObject);

            // Step 4: Load the modded scene and merge it into the loaded scene
            yield return MergeCustomScene(level);
            var loadedRoot = ObjectReferences.CustomScene;
            sceneLoader.LevelRoot = loadedRoot;

            // Step 5: Resolve component proxies and the scene loader to do what it needs to do
            foreach (var proxy in loadedRoot.GetComponentsInChildren<ComponentProxy>().OrderByDescending(x => x.ResolveOrder))
            {
                try
                {
                    proxy.InitializeComponent();
                }
                catch (Exception e)
                {
                    Debug.LogError("Error initializing component " + proxy + ":" + e.Message + "\n" + e.StackTrace);
                }
            }
            sceneLoader.Resolve();

            // Step 6: Re-enable disabled objects
            foreach (var gameObject in disabledObjects.Where(gameObject => gameObject)) gameObject.SetActive(true);

            // Step 7: Let the custom loaders do their POSTLOAD method
            sceneLoader.PostLoad();
        }

        /// <summary>
        /// This method will load the custom scene and merge it in.
        /// </summary>
        private static IEnumerator MergeCustomScene(LevelInfo level)
        {
            // If we've already loaded the bundle, take it from the dict, otherwise load it.
            AssetBundle bundle;
            if (!LoadedBundles.ContainsKey(level.AssetBundlePath))
            {
                bundle = level.AssetBundle;
                LoadedBundles.Add(level.AssetBundlePath, bundle);
            }
            else bundle = LoadedBundles[level.AssetBundlePath];

            var sceneName = Path.GetFileNameWithoutExtension(bundle.GetAllScenePaths()[0]);

            // Let Unity load the custom scene
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            yield return null;

            // Then merge the scene into the original
            SceneManager.MergeScenes(SceneManager.GetSceneByName(sceneName), SceneManager.GetActiveScene());

            // Find the level component
            ObjectReferences.CustomScene = _loadedScene.GetRootGameObjects()
                .Single(x => x.name == "[TNHLEVEL]" || x.name == "[LEVEL]")
                .GetComponent<CustomScene>();
        }

        // Reference to the currently loaded scene
        private static Scene _loadedScene;

        // Keep track of which assemblies and asset bundles we've already loaded
        private static readonly Dictionary<IFileHandle, AssetBundle> LoadedBundles = new Dictionary<IFileHandle, AssetBundle>();
        private static readonly Dictionary<string, Assembly> LoadedAssemblies = new Dictionary<string, Assembly>();

        // Public field to set which level we'll load
        public static LevelInfo? LevelToLoad = null;
    }
}
#endif