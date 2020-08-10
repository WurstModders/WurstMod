using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.TNH.Extras;
using FistVR;
using Valve.VR.InteractionSystem;
using UnityEngine.Rendering;
using WurstMod.Any;
using System.Reflection;

namespace WurstMod
{
    public static class Loader
    {
        private static readonly List<string> whitelistedObjectsTNH = new List<string>()
        {
            "[BullshotCamera]",
            "[CameraRig]Fixed",
            "[SceneSettings]",
            "_AIManager_TNH_Indoors",
            "_CoverPointSystem",
            "_FinalScore",
            "_GameManager",
            "_ItemSpawner",
            "_NewTAHReticle",
            "_ReverbSystem",
            "EventSystem",
            "[FXPoolManager](Clone)",
            "MuzzleFlash_AlloyLight(Clone)"
        };
        private static readonly List<string> whitelistedObjectsGeneric = new List<string>()
        {
            "[BullshotCamera]",
            "[CameraRig]Fixed",
            "[SceneSettings_IndoorRange]",
            "[SteamVR]",
            "_AIManager",
            "_AmbientAudio",
            "_CoverPointManager",
            "_FinalScore",
            "_ReverbSystem",
            "BangerDetonator",
            "Destructobin",
            "ItemSpawner",
            "SosigSpawner",
            "SosigTestingPanels",
            "WhizzBangADinger2"

        };
        private static readonly List<string> blacklistedObjectsTNH = new List<string>()
        {
            "HoldPoint_0",
            "HoldPoint_1",
            "HoldPoint_2",
            "HoldPoint_3",
            "HoldPoint_4",
            "HoldPoint_5",
            "HoldPoint_6",
            "HoldPoint_7",
            "HoldPoint_8",
            "HoldPoint_9",
            "HoldPoint_10",
            "HoldPoint_11",
            "HoldPoint_12",
            "Ladders",
            "Lighting_Greenhalls",
            "Lighting_Hallways",
            "Lighting_HoldRooms",
            "Lighting_SupplyRooms",
            "OpenArea",
            "RampHelperCubes",
            "ReflectionProbes",
            "SupplyPoint_0",
            "SupplyPoint_1",
            "SupplyPoint_2",
            "SupplyPoint_3",
            "SupplyPoint_4",
            "SupplyPoint_5",
            "SupplyPoint_6",
            "SupplyPoint_7",
            "SupplyPoint_8",
            "SupplyPoint_9",
            "SupplyPoint_10",
            "SupplyPoint_11",
            "SupplyPoint_12",
            "SupplyPoint_13",
            "Tiles"
        };
        private static readonly List<string> blacklistedObjectsGeneric = new List<string>()
        {
            "_Animator_Spawning_Red",
            "_Animator_Spawning_Blue",
            "_Boards",
            "_Env",
            "AILadderTest1",
            "AILadderTest1 (1)",
            "AILadderTest1 (2)",
            "AILadderTest1 (3)"
        };


        // Marks which level we are loading or loaded most recently. 
        // Empty string lets vanilla handle everything.
        public static string levelToLoad = "";


        static Scene currentScene;
        static FistVR.TNH_Manager manager;
        static GameObject loadedRoot;
        /// <summary>
        /// Perform the full loading and setting up of a custom TNH scene.
        /// </summary>
        public static IEnumerator HandleTAH(Scene loaded)
        {
            // Handle TAH load if we're loading a non-vanilla scene.
            if (loaded.name == "TakeAndHoldClassic" && levelToLoad != "")
            {
                Reset();
                currentScene = SceneManager.GetActiveScene();
                LevelType type = LevelType.TNH;

                // Certain objects need to be interrupted before they can initialize, otherwise everything breaks.
                // Once everything has been overwritten, we re-enable these.
                SetWhitelistedStates(false, type);

                // Destroy everything we no longer need.
                CleanByBlacklist(type);

                // Time to merge in the new scene.
                Entrypoint.self.StartCoroutine(MergeInScene());

                // Wait a few frames to make sure everything is peachy.
                yield return null;
                yield return null;
                yield return null;
                yield return null;

                // Resolve scene proxies to real TNH objects.
                ResolveAll(type);

                // Everything is set up, re-enable everything.
                SetWhitelistedStates(true, type);
            }
        }

        public static IEnumerator HandleGeneric(Scene loaded)
        {
            if (loaded.name == "ProvingGround" && levelToLoad != "")
            {
                Reset();
                currentScene = SceneManager.GetActiveScene();
                LevelType type = LevelType.Generic;

                SetWhitelistedStates(false, type);
                CleanByBlacklist(type);
                Entrypoint.self.StartCoroutine(MergeInScene());

                yield return null;
                yield return null;
                yield return null;
                yield return null;

                ResolveAll(type);
                SetWhitelistedStates(true, type);
            }
        }

        private static void Reset()
        {
            originalStates.Clear();
        }

        static Dictionary<string, bool> originalStates = new Dictionary<string, bool>();
        /// <summary>
        /// When state == false, Prevent whitelisted objects from initializing by quickly disabling them.
        /// When state == true, whitelisted objects will be set back to their original state.
        /// </summary>
        private static void SetWhitelistedStates(bool state, LevelType type)
        {
            // Decide which list to use
            List<string> whitelist = null;
            switch (type)
            {
                case LevelType.TNH:
                    whitelist = whitelistedObjectsTNH;
                    break;
                case LevelType.Generic:
                    whitelist = whitelistedObjectsGeneric;
                    break;
            }


            List<GameObject> whitelisted = currentScene.GetAllGameObjectsInScene().Where(x => whitelist.Contains(x.name)).ToList();

            // If we're running this for the first time, record initial state.
            // We do this because some whitelisted objects are disabled already.
            if (originalStates.Count == 0)
            {
                whitelisted.ForEach(x => originalStates[x.name] = x.activeInHierarchy);
            }

            foreach (GameObject ii in whitelisted)
            {
                if (originalStates.ContainsKey(ii.name) && originalStates[ii.name] == false) continue;
                else ii.SetActive(state);
            }
        }

        /// <summary>
        /// Nukes most objects in the TNH scene by blacklist.
        /// </summary>
        private static void CleanByBlacklist(LevelType type)
        {
            // Decide which blacklist to use.
            List<string> blacklist = null;
            switch (type)
            {
                case LevelType.TNH:
                    blacklist = blacklistedObjectsTNH;
                    break;
                case LevelType.Generic:
                    blacklist = blacklistedObjectsGeneric;
                    break;
            }

            // It just so happens the whole blacklist exists on the root. 
            // This will break if we need blacklist a non-root object!
            foreach (GameObject ii in currentScene.GetRootGameObjects())
            {
                if (blacklist.Contains(ii.name)) GameObject.Destroy(ii);
            }
        }

        private static Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
        /// <summary>
        /// Merges the custom level scene into the TNH scene.
        /// </summary>
        private static IEnumerator MergeInScene()
        {
            // First, make sure the bundle in question isn't already loaded.
            AssetBundle bundle;
            if (loadedBundles.ContainsKey(levelToLoad))
            {
                bundle = loadedBundles[levelToLoad];
            }
            else
            {
                bundle = AssetBundle.LoadFromFile(levelToLoad);
                loadedBundles[levelToLoad] = bundle;

                // Also load the assembly if it exists.
                string[] dlls = Directory.GetFiles(Path.GetDirectoryName(levelToLoad), "*.dll").ToArray();
                foreach (string dll in dlls)
                {
                    Debug.Log("LOADING ASSEMBLY: " + dll);
                    Assembly loadedAsm = Assembly.LoadFile(dll);
                    AppDomain.CurrentDomain.Load(loadedAsm.GetName());
                    yield return null;
                    Debug.Log("LOADED TYPES: " + string.Join(", ", loadedAsm.GetTypes().Select(x => x.Name).ToArray()));
                }
            }

            // Get the scene from the bundle and load it.
            // Things have to happen in a pretty specific order and I got tired of
            // fighting with async so this will have to be a synchronous load.
            string scenePath = bundle.GetAllScenePaths()[0];
            SceneManager.LoadScene(Path.GetFileNameWithoutExtension(scenePath), LoadSceneMode.Additive);

            // Need to wait an extra frame for the scene to actually be active.
            yield return null;

            // Merge this newly loaded scene. 
            //! It *should* always be at the final index, but this might be imperfect.
            // Merge must happen in this direction. Otherwise, restart scene will break (among other things.)
            SceneManager.MergeScenes(SceneManager.GetSceneAt(SceneManager.sceneCount - 1), SceneManager.GetActiveScene());

            // Grab a few objects we'll need later.
            loadedRoot = currentScene.GetRootGameObjects().Single(x => x.name == "[TNHLEVEL]" || x.name == "[LEVEL]");

            GameObject managerObj = currentScene.GetRootGameObjects().Where(x => x.name == "_GameManager").FirstOrDefault();
            if (managerObj != null)
            {
                manager = managerObj.GetComponent<FistVR.TNH_Manager>();
            }
        }

        /// <summary>
        /// Resolve all proxies into valid TNH components.
        /// </summary>
        private static void ResolveAll(LevelType type)
        {
            // Resolve component proxies
            foreach (var proxy in loadedRoot.GetComponentsInChildren<ComponentProxy>().OrderByDescending(x => x.ResolveOrder)) proxy.InitializeComponent();

            if (type == LevelType.TNH) Fix_TNH_Manager();
        }

        #region TNH
        /// <summary>
        /// Base function for setting up the TNH Manager object to handle a custom level.
        /// </summary>
        private static void Fix_TNH_Manager()
        {
            // Hold points need to be set.
            manager.HoldPoints = loadedRoot.GetComponentsInChildren<FistVR.TNH_HoldPoint>(true).ToList();

            // Supply points need to be set.
            manager.SupplyPoints = loadedRoot.GetComponentsInChildren<FistVR.TNH_SupplyPoint>(true).ToList();

            // Possible Sequences need to be generated at random.
            manager.PossibleSequnces = GenerateRandomPointSequences(10);

            // Safe Pos Matrix needs to be set. Diagonal for now.
            FistVR.TNH_SafePositionMatrix maxMatrix = GenerateTestMatrix();
            manager.SafePosMatrix = maxMatrix;
        }

        /// <summary>
        /// Regular gamemode uses a preset list of possible hold orders. This creates a bunch randomly.
        /// </summary>
        private static List<FistVR.TNH_PointSequence> GenerateRandomPointSequences(int count)
        {
            List<FistVR.TNH_PointSequence> sequences = new List<FistVR.TNH_PointSequence>();
            for (int ii = 0; ii < count; ii++)
            {
                FistVR.TNH_PointSequence sequence = ScriptableObject.CreateInstance<FistVR.TNH_PointSequence>();

                // Logic for forced spawn location.
                ForcedSpawn forcedSpawn = loadedRoot.GetComponentInChildren<ForcedSpawn>();
                if (forcedSpawn != null)
                {
                    sequence.StartSupplyPointIndex = manager.SupplyPoints.IndexOf(manager.SupplyPoints.First(x => x.gameObject == forcedSpawn.gameObject));
                }
                else
                {
                    sequence.StartSupplyPointIndex = UnityEngine.Random.Range(0, manager.SupplyPoints.Count);
                }

                sequence.HoldPoints = new List<int>()
                {
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count),
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count),
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count),
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count),
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count)
                };

                // Fix sequence, because they may generate the same point after the current point, IE {1,4,4)
                // This would break things.
                for(int jj = 0; jj < sequence.HoldPoints.Count - 1; jj++)
                {
                    if (sequence.HoldPoints[jj] == sequence.HoldPoints[jj + 1])
                    {
                        sequence.HoldPoints[jj + 1] = (sequence.HoldPoints[jj + 1] + 1) % manager.HoldPoints.Count;
                    }
                }

                sequences.Add(sequence);
            }
            return sequences;
        }

        /// <summary>
        /// Creates a matrix of valid Hold Points and Supply Points (I think) only used for endless?
        /// By default, just generates a matrix that is false on diagonals.
        /// </summary>
        private static FistVR.TNH_SafePositionMatrix GenerateTestMatrix()
        {
            FistVR.TNH_SafePositionMatrix maxMatrix = ScriptableObject.CreateInstance<FistVR.TNH_SafePositionMatrix>();
            maxMatrix.Entries_HoldPoints = new List<FistVR.TNH_SafePositionMatrix.PositionEntry>();
            maxMatrix.Entries_SupplyPoints = new List<FistVR.TNH_SafePositionMatrix.PositionEntry>();

            int effectiveHoldCount = manager.HoldPoints.Count;
            int effectiveSupplyCount = manager.SupplyPoints.Where(x => x.GetComponent<ForcedSpawn>() == null).Count();

            for (int ii = 0; ii < effectiveHoldCount; ii++)
            {
                FistVR.TNH_SafePositionMatrix.PositionEntry entry = new FistVR.TNH_SafePositionMatrix.PositionEntry();
                entry.SafePositions_HoldPoints = new List<bool>();
                for (int jj = 0; jj < effectiveHoldCount; jj++)
                {
                    entry.SafePositions_HoldPoints.Add(ii != jj);
                }

                entry.SafePositions_SupplyPoints = new List<bool>();
                for (int jj = 0; jj < effectiveSupplyCount; jj++)
                {
                    entry.SafePositions_SupplyPoints.Add(true);
                }

                maxMatrix.Entries_HoldPoints.Add(entry);
            }

            for (int ii = 0; ii < effectiveSupplyCount; ii++)
            {
                FistVR.TNH_SafePositionMatrix.PositionEntry entry = new FistVR.TNH_SafePositionMatrix.PositionEntry();
                entry.SafePositions_HoldPoints = new List<bool>();
                for (int jj = 0; jj < effectiveHoldCount; jj++)
                {
                    entry.SafePositions_HoldPoints.Add(true);
                }

                entry.SafePositions_SupplyPoints = new List<bool>();
                for (int jj = 0; jj < effectiveSupplyCount; jj++)
                {
                    entry.SafePositions_SupplyPoints.Add(ii != jj);
                }

                maxMatrix.Entries_SupplyPoints.Add(entry);
            }

            return maxMatrix;
        }
        #endregion
    }
}
