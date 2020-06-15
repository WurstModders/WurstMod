using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;


namespace WurstMod
{
    [BepInPlugin("com.koba.plugins.wurstmod", "WurstMod", "1.0.0.0")]
    public class Entrypoint : BaseUnityPlugin
    {
        public static BaseUnityPlugin self;
        void Awake()
        {
            self = this;
            RegisterListeners();
            InitDetours();
        }

        void RegisterListeners()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        void InitDetours()
        {
            Patches.Patch();
        }

        private void SceneManager_sceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(Loader.HandleTAH(scene));
            TNH.LevelSelector.SetupLevelSelector(scene);
        }
    }
}
