using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using System.Reflection;

namespace WurstMod
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

        private static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        void InitAppDomain()
        {
            AppDomain.CurrentDomain.AssemblyLoad += (sender, e) =>
            {
                assemblies[e.LoadedAssembly.FullName] = e.LoadedAssembly;
            };
            AppDomain.CurrentDomain.AssemblyResolve += (sender, e) =>
            {
                Assembly assembly = null;
                assemblies.TryGetValue(e.Name, out assembly);
                return assembly;
            };
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ObjectReferences.FindReferences(scene);
            StartCoroutine(Loader.HandleTAH(scene));
            StartCoroutine(Loader.HandleGeneric(scene));
            TNH.TNH_LevelSelector.SetupLevelSelector(scene);
            Generic_LevelPopulator.SetupLevelPopulator(scene);
        }
    }
}
