using FistVR;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    class DebugQuickloader
    {
        static bool startup = true;
        public static void Quickload(Scene scene)
        {
            if (scene.name == "MainMenu3" && startup)
            {
                // Ensure object references are set up.
                ObjectReferences.FindReferences(scene);

                // Read from config.
                string path = Entrypoint.configQuickload.Value;
                if (string.IsNullOrEmpty(path)) return;

                // Try to grab the info.txt.
                Loader.LevelToLoad = LevelInfo.FromFile(path.TrimEnd('\\') + "\\" + Constants.FilenameLevelInfo);
                if (!Loader.LevelToLoad.HasValue)
                {
                    Debug.LogError("Quickloader failed to find " + Constants.FilenameLevelInfo + " in specified directory: " + path);
                    return;
                }

                // Find the name of the base scene for this level and create a fake scene def.
                string sceneName = CustomSceneLoader.GetSceneLoaderForGamemode(((LevelInfo)Loader.LevelToLoad).Gamemode).BaseScene;
                MainMenuSceneDef fakeDef = ScriptableObject.CreateInstance<MainMenuSceneDef>();
                fakeDef.SceneName = sceneName;

                // Only run once.
                startup = false;

                // Feed the fake scene def to the loader and force a load.
                ObjectReferences.MainMenuControls.SetSelectedScene(fakeDef);
                ObjectReferences.MainMenuControls.LoadScene();

                Debug.Log("Quickloader found and loaded scene: \"" + ((LevelInfo)Loader.LevelToLoad).SceneName + "\" based on: \"" + sceneName + "\"");
            }
        }
    }
}
