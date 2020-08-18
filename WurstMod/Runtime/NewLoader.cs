using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
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
    public static class NewLoader
    {
        /// <summary>
        /// This is called by the BepInEx entrypoint. It is fired when ANY scene loads, including vanilla ones.
        /// </summary>
        public static void OnSceneLoad(Scene scene)
        {
            // If we're trying to load a custom scene
            var sceneName = scene.name;
            if (LevelToLoad.HasValue)
            {
                var level = LevelToLoad.Value;
                LoadCustomAssemblies(level);
                LoadCustomScene(level);
                sceneName = level.SceneName;
            }

            // Let the scene patchers for the scene do their thing
            
        }

        /// <summary>
        /// This method loads custom assemblies packed beside the custom scene.
        /// </summary>
        private static void LoadCustomAssemblies(LevelInfo level)
        {
            foreach (var assembly in Directory.GetFiles(level.Location, "*.dll"))
            {
                if (LoadedAssemblies.Contains(assembly)) continue;
                var loadedAsm = Assembly.LoadFile(assembly);
                AppDomain.CurrentDomain.Load(loadedAsm.GetName());
                Debug.Log("LOADED TYPES: " + string.Join(", ", loadedAsm.GetTypes().Select(x => x.Name).ToArray()));
                LoadedAssemblies.Add(assembly);
            }
        }
        
        /// <summary>
        /// This method is called to load a custom scene 
        /// </summary>
        private static void LoadCustomScene(LevelInfo level)
        {
            // Step 0: Get the loaded scene and find any custom loaders and scene patchers
            _loadedScene = SceneManager.GetActiveScene();
            var sceneLoader = CustomSceneLoader.GetSceneLoaderForGamemode(level.Gamemode);
            if (sceneLoader == null)
            {
                Debug.LogError($"No SceneLoader found for the gamemode '{level.Gamemode}'! Cannot load level.");
                return;
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
            foreach (var filter in DestroyOnLoad)
                if (gameObject.name.Contains(filter))
                    Object.Destroy(gameObject.gameObject);
            
            // Step 4: Load the modded scene and merge it into the loaded scene
            var loadedRoot = MergeCustomScene(level);
            
            // Step 5: Resolve component proxies
            foreach (var proxy in loadedRoot.GetComponentsInChildren<ComponentProxy>().OrderByDescending(x => x.ResolveOrder)) proxy.InitializeComponent();
            
            // Step 6: Re-enable disabled objects
            foreach (var gameObject in disabledObjects) gameObject.SetActive(true);
            
            // Step 7: Let the custom loaders do their POSTLOAD method
            sceneLoader.PostLoad();

        }

        /// <summary>
        /// This method will load the custom scene and merge it in.
        /// </summary>
        /// <returns>The root object of the modded scene</returns>
        private static CustomScene MergeCustomScene(LevelInfo level)
        {
            // If we've already loaded the bundle, take it from the dict, otherwise load it.
            AssetBundle bundle;
            if (!LoadedBundles.ContainsKey(level.AssetBundlePath))
            {
                bundle = AssetBundle.LoadFromFile(level.AssetBundlePath);
                LoadedBundles.Add(level.AssetBundlePath, bundle);
            }
            else bundle = LoadedBundles[level.AssetBundlePath];
            var sceneName = Path.GetFileNameWithoutExtension(bundle.GetAllScenePaths()[0]);
            
            // Let Unity load the custom scene and merge it into the current scene
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
            SceneManager.MergeScenes(SceneManager.GetSceneAt(SceneManager.sceneCount - 1), SceneManager.GetActiveScene());
            
            // Find the level component
            ObjectReferences.CustomScene = _loadedScene.GetRootGameObjects()
                .Single(x => x.name == "[TNHLEVEL]" || x.name == "[LEVEL]")
                .GetComponent<CustomScene>();

            return ObjectReferences.CustomScene;
        }

        // Reference to the currently loaded scene
        private static Scene _loadedScene;
        
        // Keep track of which assemblies and asset bundles we've already loaded
        private static readonly Dictionary<string, AssetBundle> LoadedBundles = new Dictionary<string, AssetBundle>();
        private static readonly List<string> LoadedAssemblies = new List<string>();
        
        // Public field to set which level we'll load
        public static LevelInfo? LevelToLoad = null;
        
        // List of GameObjects we want to destroy on load.
        private static readonly string[] DestroyOnLoad = {
            ""
        };
    }
}