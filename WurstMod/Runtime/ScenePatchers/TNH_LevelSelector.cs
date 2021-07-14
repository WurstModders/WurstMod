#if !UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FistVR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WurstMod.Shared;

namespace WurstMod.Runtime.ScenePatchers
{
    /// <summary>
    /// This class is responsible for modifying the level selector panel to actually function.
    /// </summary>
    public static class TNH_LevelSelector
    {
        /// <summary>
        /// Performs all actions required to setup the level selector in the TNH Lobby.
        /// </summary>
        public static void SetupLevelSelector(Scene loaded)
        {
            if (loaded.name == "TakeAndHold_Lobby_2")
            {
                var uiManager = Object.FindObjectOfType<TNH_UIManager>();
                foreach (LevelInfo level in CustomLevelFinder.ArchiveLevels.Where(x => x.Gamemode == Constants.GamemodeTakeAndHold))
                    uiManager.Levels.Add(new ModdedLevelInfo(level));
            }
        }
    }
}
#endif