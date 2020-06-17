using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FistVR;
using HarmonyLib;
using UnityEngine;
using UnityEngine.AI;

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

    #region Scoreboard Disabling
    /// <summary>
    /// This patch prevents high scores from being submitted online when playing
    /// a custom level, preserving the legitimacy of the leaderboards.
    /// </summary>
    [HarmonyPatch(typeof(TNH_ScoreDisplay), "ProcessHighScore")]
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
    #endregion

    #region Auto-Generated Off-Mesh Link Support
    /// <summary>
    /// Sosig's don't actually support automatically generated off-mesh links.
    /// This patch prevents some console spam and maybe a crash possibility?
    /// </summary>
    [HarmonyPatch(typeof(Sosig), "LegsUpdate_MoveToPoint")]
    public class Patch_LegsUpdate_MoveToPoint
    {
        // Hold onto Sosigs and their local NavMeshLinkExtension. 
        static Dictionary<Sosig, NavMeshLinkExtension> genData = new Dictionary<Sosig, NavMeshLinkExtension>();

        static bool Prefix(Sosig __instance)
        {
            if (__instance.Agent.isOnOffMeshLink && __instance.Agent.currentOffMeshLinkData.offMeshLink == null)
            {
                if (!(bool)__instance.ReflectGet("m_isOnOffMeshLink"))
                {
                    // Setup fake link, which is added as a component to the Sosig.
                    NavMeshLinkExtension fakeLink;
                    if (genData.ContainsKey(__instance))
                    {
                        fakeLink = genData[__instance];
                    }
                    else
                    {
                        fakeLink = __instance.gameObject.AddComponent<NavMeshLinkExtension>();
                        genData[__instance] = fakeLink;
                    }


                    // Force fields on the fake link to match the type of jump.
                    Vector3 jump = __instance.Agent.currentOffMeshLinkData.endPos - __instance.Agent.currentOffMeshLinkData.startPos;
                    float horzMag = new Vector3(jump.x, 0, jump.z).magnitude;
                    float vertMag = Mathf.Abs(jump.y);
                    bool jumpDown = jump.y < 0;

                    if (!jumpDown && vertMag > (2 * horzMag)) fakeLink.Type = NavMeshLinkExtension.NavMeshLinkType.Climb;
                    else if (jumpDown && vertMag > (2 * horzMag)) fakeLink.Type = NavMeshLinkExtension.NavMeshLinkType.Drop;
                    else fakeLink.Type = NavMeshLinkExtension.NavMeshLinkType.LateralJump;

                    // Arbitrary number.
                    fakeLink.ReflectSet("m_xySpeed", 0.5f);
                    

                    // Clean up the dictionary. Keep an eye on the counts for this...
                    // Destroyed Unity objects sure have some confusing properties.
                    List<Sosig> nullCheck = new List<Sosig>();
                    foreach(var pair in genData)
                    {
                        if (pair.Key == null) nullCheck.Add(pair.Key);
                    }
                    foreach (Sosig destroyed in nullCheck)
                    {
                        genData.Remove(destroyed);
                    }
                    

                    __instance.ReflectSet("m_isOnOffMeshLink", true);
                    __instance.ReflectInvoke("InitiateLink", fakeLink);
                }
            }
            return true;
        }
    }

    /// <summary>
    /// The Start function of NavMeshLinkExtension also relies on existence of an OffMeshLink.
    /// So we create our own second case. Might be hard to avoid GetComponent here, but ah well.
    /// </summary>
    [HarmonyPatch(typeof(NavMeshLinkExtension), "Start")]
    public class Patch_NavMeshLinkExtension_Start
    {
        static bool Prefix(NavMeshLinkExtension __instance)
        {
            // Skip the Start method if this gameObject is a Sosig.
            return __instance.gameObject.GetComponent<Sosig>() == null;
        }
    }
    #endregion
}
