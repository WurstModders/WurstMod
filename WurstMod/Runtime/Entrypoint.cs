#if !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using Deli;
using Deli.Setup;
using Deli.VFS;
using Steamworks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;
using WurstMod.Runtime.ScenePatchers;
using WurstMod.Shared;
using Constants = WurstMod.Shared.Constants;

namespace WurstMod.Runtime
{
    public class Entrypoint : DeliBehaviour
    {
        private static readonly Dictionary<string, Assembly> Assemblies = new Dictionary<string, Assembly>();
        private static ConfigEntry<bool> loadDebugLevels;
        private static ConfigEntry<bool> useLegacyLoadingMethod;

        public Entrypoint()
        {
            int buildId = -1;
            try
            {
                SteamAPI.Init();
                buildId = SteamApps.GetAppBuildId();
            }
            catch (InvalidOperationException)
            {
                // Ignored
            }

            if (buildId < 7024234)
                throw new Exception("This version of the game is not supported. Please update your game.");

            // Patches
            Patches.Patch();
            
            // Add a number of callbacks
            Stages.Setup += StagesOnSetup;
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) => { Assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly; };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                Assemblies.TryGetValue(e.Name, out var assembly);
                return assembly;
            };

            // Config values
            loadDebugLevels = Config.Bind("Debug", "LoadDebugLevels", false, "True if you want the included default levels to be loaded");
            useLegacyLoadingMethod = Config.Bind("Debug", "UseLegacyLoadingMethod", false, $"True if you want to support loading legacy v1 or standalone levels from the {Constants.LegacyLevelsDirectory} folder");

            // Legacy support
            if (useLegacyLoadingMethod.Value)
                LegacySupport.EnsureLegacyFolderExists(Resources.GetFile("legacyManifest.json"));
        }

        private void StagesOnSetup(SetupStage stage)
        {
            stage.SharedAssetLoaders[Source, "level"] = LevelLoader;
        }

        private void LevelLoader(Stage stage, Mod mod, IHandle handle)
        {
            // If the config has disabled loading the default included levels, return
            if (!loadDebugLevels.Value && mod.Info.Guid == "wurstmodders.wurstmod")
                return;
            
            // If this is the legacy stuff and we have that disabled, return
            if (!useLegacyLoadingMethod.Value && mod.Info.Guid == "wurstmodders.wurstmod.legacy")
                return;

            // Try to make a level info from it
            var level = LevelInfo.FromFrameworkMod(mod, handle);

            if (!level.HasValue)
            {
                // Skip an error here because this will throw some errors on folders we know are invalid.
                if (mod.Info.Guid != "wurstmodders.wurstmod.legacy")
                    Debug.LogError($"Level in {mod}, {handle} is not valid!");
            }
            else
            {
                CustomLevelFinder.ArchiveLevels.Add(level.Value);
                Debug.Log($"Discovered level {level.Value.SceneName} in {mod}, {handle}");
            }
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TNH_LevelSelector.SetupLevelSelector(scene);
            Generic_LevelPopulator.SetupLevelPopulator(scene);
            StartCoroutine(Loader.OnSceneLoad(scene));
        }
    }
}
#endif