using System;
using System.Collections.Generic;
using System.Reflection;
using BepInEx;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.TakeAndHold;
using WurstMod.Runtime.ScenePatchers;

namespace WurstMod.Runtime
{
    [BepInPlugin("com.koba.plugins.wurstmod", "WurstMod", "1.3.0.0")]
    public class Entrypoint : BaseUnityPlugin
    {
        public static BaseUnityPlugin self;
        void Awake()
        {
            self = this;
            RegisterListeners();
            InitDetours();
            InitAppDomain();
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
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
            {
                Assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                Assemblies.TryGetValue(e.Name, out var assembly);
                return assembly;
            };
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            //StartCoroutine(Loader.HandleTAH(scene));
            //StartCoroutine(Loader.HandleGeneric(scene));
            TNH_LevelSelector.SetupLevelSelector(scene);
            Generic_LevelPopulator.SetupLevelPopulator(scene);
            NewLoader.OnSceneLoad(scene);
        }
    }
}
