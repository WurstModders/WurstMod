using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FistVR;
using HarmonyLib;
using UnityEngine;
using WurstMod.MappingComponents.Generic;
using WurstMod.MappingComponents.TakeAndHold;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public class Patches
    {
        static Harmony harmony;

        public static void Patch()
        {
            harmony = new Harmony("com.koba.plugins.wurstmod");

            // MUST patch GetTypes first. 
            // This might result in a double-patch for GetTypes but it won't hurt anything.
            harmony.ProcessorForAnnotatedClass(typeof(Patch_Assembly)).Patch();
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    #region Assembly GetTypes Hacking
    // Assembly.GetTypes() fails if ANY of the types cannot be loaded.
    // In practice, this means we cannot inherit from UnityEditor.
    // To fix this, we will literally patch mscorlib.dll.
    [HarmonyPatch(typeof(Assembly), "GetTypes", new Type[0])]
    public class Patch_Assembly
    {
        static Exception Finalizer(Exception __exception, ref Type[] __result)
        {
            if (__exception != null)
            {
                __result = ((ReflectionTypeLoadException)__exception).Types.Where(t => t != null).ToArray();
            }
            return null;
        }
    }

    #endregion

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
            if (Loader.LevelToLoad != null)
            {
                // Returning false on a prefix prevents the original method from running.
                Debug.Log("Ignoring high score for custom level.");
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
                if (!__instance.ReflectGet<bool>("m_isOnOffMeshLink"))
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


                    // Clean up the dictionary.
                    // Destroyed Unity objects sure have some confusing properties.
                    List<Sosig> nullCheck = new List<Sosig>();
                    foreach (var pair in genData)
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

    #region ForcedSpawn SpawnOnly Support

    /// <summary>
    /// To enforce a supply point as SpawnOnly, we need to remove it after its used
    /// This patch lets us do that safely.
    /// </summary>
    [HarmonyPatch(typeof(TNH_Manager), "InitBeginningEquipment")]
    public class Patch_TNH_Manager_InitBeginningEquipment
    {
        static void Postfix(TNH_Manager __instance)
        {
            var forcedSpawn = __instance.SupplyPoints.Select(x => x.GetComponent<ForcedSpawn>()).FirstOrDefault(x => x != null);
            if (forcedSpawn == null) return;
            __instance.SupplyPoints = __instance.SupplyPoints.Where(x => x.gameObject != forcedSpawn.gameObject).ToList();
            __instance.ReflectGet<TNH_PointSequence>("m_curPointSequence").StartSupplyPointIndex = 0;
        }
    }

    #endregion

    #region Generic Level Support

    [HarmonyPatch(typeof(MainMenuScenePointable), "OnPoint")]
    public class Patch_MainMenuScenePointable_OnPoint
    {
        static bool Prefix(MainMenuScenePointable __instance, FVRViveHand hand)
        {
            if (hand.Input.TriggerDown) Loader.LevelToLoad = __instance.name == "MODDEDSCREEN" ? LevelInfo.FromFile(__instance.Def.Name.Split('\n')[1]) : null;

            return true;
        }
    }

    #endregion

    #region Target Event Support

    [HarmonyPatch(typeof(ReactiveSteelTarget), "Damage")]
    public class Patch_ReactiveSteelTarget_Damage
    {
        static Dictionary<ReactiveSteelTarget, Target> targetComponents = new Dictionary<ReactiveSteelTarget, Target>();

        static bool Prefix(ReactiveSteelTarget __instance)
        {
            // Cache Target components.
            Target ourTarget = null;
            if (targetComponents.ContainsKey(__instance))
            {
                ourTarget = targetComponents[__instance];
            }
            else
            {
                ourTarget = __instance.GetComponent<Target>();
                targetComponents[__instance] = ourTarget;
            }

            // Clear cache as necessary, using weird Unity destroyed behaviour.
            List<ReactiveSteelTarget> nullCheck = new List<ReactiveSteelTarget>();
            foreach (var pair in targetComponents)
            {
                if (pair.Key == null) nullCheck.Add(pair.Key);
            }

            foreach (ReactiveSteelTarget destroyed in nullCheck)
            {
                targetComponents.Remove(destroyed);
            }

            // Run event logic.
            if (ourTarget != null)
            {
                if (ourTarget.shotEvent != null) ourTarget.shotEvent.Invoke();
            }

            return true;
        }
    }

    #endregion

    #region Whizzbanger Support

    /// <summary>
    /// This patched method will run on the disabled donor detonator in generic sandbox levels
    /// whenever a new banger is created by any whizzbanger, adding the new banger to all other
    /// detonators lists.
    /// 
    /// With this patch, any banger created by any whizzbanger can be detonated by any detonator in the scene.
    /// </summary>
    [HarmonyPatch(typeof(BangerDetonator), "RegisterBanger")]
    public class Patch_BangerDetonator_RegisterBanger
    {
        static List<BangerDetonator> detonators = new List<BangerDetonator>();

        static bool Prefix(BangerDetonator __instance, Banger b)
        {
            if (Loader.LevelToLoad != null)
            {
                // We are in a custom level.
                if (detonators.Count == 0 || detonators[0] == null)
                {
                    // We have loaded a new custom level, recreate the cache.
                    detonators.Clear();
                    detonators.AddRange(GameObject.FindObjectsOfType<BangerDetonator>());
                }

                // Add banger to all detonators in cache.
                foreach (BangerDetonator ii in detonators)
                {
                    if (!ii.ReflectGet<List<Banger>>("m_bangers").Contains(b))
                    {
                        ii.ReflectGet<List<Banger>>("m_bangers").Add(b);
                    }
                }

                return false;
            }

            return true;
        }
    }

    #endregion
}