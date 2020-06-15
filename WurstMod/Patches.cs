using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using ErosionBrushPlugin;
using FistVR;
using HarmonyLib;

namespace WurstMod
{
    public class Patches
    {
        static Harmony harmony;
        public static void Patch()
        {
            harmony = new Harmony("com.koba.plugins.wurstmod");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    /// <summary>
    /// This patch prevents high scores from being submitted online when playing
    /// a custom level, preserving the legitimacy of the leaderboards.
    /// </summary>
    [HarmonyPatch(typeof(TNH_ScoreDisplay))]
    [HarmonyPatch("ProcessHighScore")]
    public class Patch_TNH_ScoreDisplay
    {
        static bool Prefix(int score)
        {
            if (Loader.levelToLoad != "")
            {
                // Returning false on a prefix prevents the original method from running.
                UnityEngine.Debug.Log("Ignoring high score for custom level.");
                return false;
            }
            return true;
        }
    }
}
