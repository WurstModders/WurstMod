using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using Deli;
using UnityEngine.SceneManagement;
using WurstMod.Runtime.ScenePatchers;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public class Entrypoint : DeliBehaviour
    {
        public static Entrypoint Instance;
        
        void Awake()
        {
            Instance = this;
            RegisterListeners();
            InitDetours();
            InitAppDomain();
            InitConfig();
        }

        void RegisterListeners()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        void InitDetours()
        {
            Patches.Patch();
        }

        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();

        void InitAppDomain()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => { Assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly; };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                Assemblies.TryGetValue(e.Name, out var assembly);
                return assembly;
            };
        }

        public static ConfigEntry<string> ConfigQuickload;
        public static ConfigEntry<bool> LoadDebugLevels;
        public static ConfigEntry<bool> UseLegacyLoadingMethod;
        void InitConfig()
        {
            LoadDebugLevels = Config.Bind("Debug", "LoadDebugLevels", true, "True if you want the included default levels to be loaded");
            UseLegacyLoadingMethod = Config.Bind("Debug", "UseLegacyLoadingMethod", true, $"True if you want to support loading legacy v1 or standalone levels from the {Constants.LegacyLevelsDirectory} folder");
            
            if (UseLegacyLoadingMethod.Value)
                LegacySupport.EnsureLegacyFolderExists();
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TNH_LevelSelector.SetupLevelSelector(scene);
            Generic_LevelPopulator.SetupLevelPopulator(scene);
            
            StartCoroutine(Loader.OnSceneLoad(scene));
        }

        public IResourceIO ResourceIO => Resources;
    }
}