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
        void Awake()
        {
            RegisterListeners();
            InitDetours();
            InitAppDomain();
            InitConfig();

            // Only support legacy formats if opted in
            if (UseLegacyLoadingMethod.Value)
            {
                LegacySupport.Init();
                CustomLevelFinder.DiscoverLevelsInFolder();
            }
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
            ConfigQuickload = Config.Bind("Debug", "QuickloadPath", "", "Set this to a folder containing the scene you would like to load as soon as H3VR boots. This is good for quickly testing scenes you are developing.");
            LoadDebugLevels = Config.Bind("Debug", "LoadDebugLevels", true, "True if you want the included default levels to be loaded");
            UseLegacyLoadingMethod = Config.Bind("Debug", "UseLegacyLoadingMethod", true, $"True if you want to support loading legacy v1 or standalone levels from the {Constants.CustomLevelsDirectory} folder");
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TNH_LevelSelector.SetupLevelSelector(scene);
            Generic_LevelPopulator.SetupLevelPopulator(scene);
            
            StartCoroutine(Loader.OnSceneLoad(scene));
            DebugQuickloader.Quickload(scene); // Must occur after regular loader.
        }
    }
}