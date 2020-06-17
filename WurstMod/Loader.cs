using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WurstMod
{
    public static class Loader
    {
        private static readonly List<string> whitelistedObjects = new List<string>()
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
        private static readonly List<string> blacklistedObjects = new List<string>()
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
                currentScene = SceneManager.GetActiveScene();

                // Certain objects need to be interrupted before they can initialize, otherwise everything breaks.
                // Once everything has been overwritten, we re-enable these.
                SetWhitelistedStates(false);

                // There are a handful of vars that are pretty difficult to get our hands on.
                // So let's just steal them off existing objects before we delete them.
                CollectRequiredObjects();

                // Destroy everything we no longer need.
                CleanByBlacklist();

                // Time to merge in the new scene.
                Entrypoint.self.StartCoroutine(MergeInScene());

                // Wait a few frames to make sure everything is peachy.
                yield return null;
                yield return null;
                yield return null;
                yield return null;

                // Resolve scene proxies to real TNH objects.
                ResolveAll();

                // Everything is set up, re-enable everything.
                SetWhitelistedStates(true);
            }
        }

        static Dictionary<string, bool> originalStates = new Dictionary<string, bool>();
        /// <summary>
        /// When state == false, Prevent whitelisted objects from initializing by quickly disabling them.
        /// When state == true, whitelisted objects will be set back to their original state.
        /// </summary>
        private static void SetWhitelistedStates(bool state)
        {
            // Gather up whitelisted objects.
            List<GameObject> whitelisted = currentScene.GetAllGameObjectsInScene().Where(x => whitelistedObjects.Contains(x.name)).ToList();

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

        static FistVR.AudioEvent wave;
        static FistVR.AudioEvent success;
        static FistVR.AudioEvent failure;
        static GameObject vfx;
        static GameObject[] barrierPrefabs = new GameObject[2];
        /// <summary>
        /// A wide variety of existing objects are needed for importing a new TNH scene.
        /// This function grabs all of them.
        /// </summary>
        private static void CollectRequiredObjects()
        {
            // We need HoldPoint AudioEvents.
            FistVR.TNH_HoldPoint sourceHoldPoint = currentScene.GetRootGameObjects().Where(x => x.name == "HoldPoint_0").First().GetComponent<FistVR.TNH_HoldPoint>();

            wave = new FistVR.AudioEvent();
            wave.Clips = sourceHoldPoint.AUDEvent_HoldWave.Clips.ToList();
            wave.VolumeRange = new Vector2(0.4f, 0.4f);
            wave.PitchRange = new Vector2(1, 1);
            wave.ClipLengthRange = new Vector2(1, 1);

            success = new FistVR.AudioEvent();
            success.Clips = sourceHoldPoint.AUDEvent_Success.Clips.ToList();
            success.VolumeRange = new Vector2(0.4f, 0.4f);
            success.PitchRange = new Vector2(1, 1);
            success.ClipLengthRange = new Vector2(1, 1);

            failure = new FistVR.AudioEvent();
            failure.Clips = sourceHoldPoint.AUDEvent_Failure.Clips.ToList();
            failure.VolumeRange = new Vector2(0.4f, 0.4f);
            failure.PitchRange = new Vector2(1, 1);
            failure.ClipLengthRange = new Vector2(1, 1);

            // We need VFX_HoldWave prefab.
            vfx = sourceHoldPoint.VFX_HoldWave;

            // We need barrier prefabs.
            FistVR.TNH_DestructibleBarrierPoint barrier = currentScene.GetAllGameObjectsInScene().Where(x => x.name == "Barrier_SpawnPoint").First().GetComponent<FistVR.TNH_DestructibleBarrierPoint>();
            barrierPrefabs[0] = barrier.BarrierDataSets[0].BarrierPrefab;
            barrierPrefabs[1] = barrier.BarrierDataSets[1].BarrierPrefab;
        }

        /// <summary>
        /// Nukes most objects in the TNH scene by blacklist.
        /// </summary>
        private static void CleanByBlacklist()
        {
            //! It just so happens the whole blacklist exists on the root. 
            //! This will break if we need blacklist a non-root object!
            foreach (GameObject ii in currentScene.GetRootGameObjects())
            {
                if (blacklistedObjects.Contains(ii.name)) GameObject.Destroy(ii);
            }
        }

        static Dictionary<string, AssetBundle> loadedBundles = new Dictionary<string, AssetBundle>();
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
                //bundle = AssetBundle.LoadFromFile(levelToLoad);
                AssetBundleCreateRequest bundleReq = AssetBundle.LoadFromFileAsync(levelToLoad);
                yield return bundleReq;
                bundle = bundleReq.assetBundle;
                loadedBundles[levelToLoad] = bundle;
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
            //TODO Merging in other direction might allow lighting/skybox settings to be preserved?
            SceneManager.MergeScenes(SceneManager.GetSceneAt(SceneManager.sceneCount - 1), SceneManager.GetActiveScene());

            // Grab a few objects we'll need later.
            loadedRoot = currentScene.GetRootGameObjects().Single(x => x.name == "[TNHLEVEL]");
            manager = currentScene.GetRootGameObjects().Single(x => x.name == "_GameManager").GetComponent<FistVR.TNH_Manager>();
        }

        /// <summary>
        /// Resolve all proxies into valid TNH components.
        /// </summary>
        private static void ResolveAll()
        {
            Resolve_Skybox();
            Resolve_Shaders();
            Resolve_PMats();
            Resolve_FVRReverbEnvironments();
            Resolve_FVRHandGrabPoints();
            Resolve_AICoverPoints();
            Resolve_TNH_DestructibleBarrierPoints();
            Resolve_TNH_SupplyPoints();
            Resolve_TNH_HoldPoints();
            Resolve_ScoreboardArea();

            Fix_TNH_Manager();
        }

        #region Resolves
        /// <summary>
        /// Use the skybox of the imported level.
        /// Requires GI Update to fix lighting.
        /// </summary>
        private static void Resolve_Skybox()
        {
            TNH.TNH_Level levelComponent = loadedRoot.GetComponent<TNH.TNH_Level>();
            if (levelComponent.skybox != null)
            {
                RenderSettings.skybox = levelComponent.skybox;
                RenderSettings.skybox.RefreshShader();
                DynamicGI.UpdateEnvironment();
            }
        }


        /// <summary>
        /// Shaders, when imported from an assetbundle, become garbage.
        /// Set them to themselves and bam, it works.
        /// Unity 5 bugs sure were something.
        /// </summary>
        private static void Resolve_Shaders()
        {
            foreach (MeshRenderer ii in loadedRoot.GetComponentsInChildren<MeshRenderer>(true))
            {
                foreach (Material jj in ii.materials)
                {
                    jj.RefreshShader();
                }
            }
        }

        /// <summary>
        /// Creates valid PMats from proxies.
        /// </summary>
        private static void Resolve_PMats()
        {
            WurstMod.TNH.PMat[] pMatProxies = loadedRoot.GetComponentsInChildren<WurstMod.TNH.PMat>();
            foreach (var proxy in pMatProxies)
            {
                GameObject owner = proxy.gameObject;
                FistVR.PMat real = owner.AddComponent<FistVR.PMat>();

                if (proxy.def == WurstMod.TNH.PMat.Def.None) real.Def = null;
                else real.Def = Resources.Load<FistVR.PMaterialDefinition>("pmaterialdefinitions/" + proxy.GetDef((int)proxy.def));

                real.MatDef = Resources.Load<FistVR.MatDef>("matdefs/" + proxy.GetMatDef((int)proxy.matDef));
            }
        }

        /// <summary>
        /// Creates valid VRReverbEnvironments from proxies.
        /// </summary>
        private static void Resolve_FVRReverbEnvironments()
        {
            WurstMod.TNH.FVRReverbEnvironment[] reverbProxies = loadedRoot.GetComponentsInChildren<WurstMod.TNH.FVRReverbEnvironment>();
            foreach (var proxy in reverbProxies)
            {
                GameObject owner = proxy.gameObject;
                FistVR.FVRReverbEnvironment real = owner.AddComponent<FistVR.FVRReverbEnvironment>();

                real.Environment = (FistVR.FVRSoundEnvironment)proxy.Environment;
                real.Priority = proxy.Priority;
            }
        }

        /// <summary>
        /// Creates valid FVRHandGrabPoints from proxies.
        /// </summary>
        private static void Resolve_FVRHandGrabPoints()
        {
            TNH.FVRHandGrabPoint[] grabProxies = loadedRoot.GetComponentsInChildren<TNH.FVRHandGrabPoint>();
            foreach (var proxy in grabProxies)
            {
                GameObject owner = proxy.gameObject;
                FistVR.FVRHandGrabPoint real = owner.AddComponent<FistVR.FVRHandGrabPoint>();

                real.UXGeo_Hover = proxy.UXGeo_Hover;
                real.UXGeo_Held = proxy.UXGeo_Held;
                real.PositionInterpSpeed = 1;
                real.RotationInterpSpeed = 1;

                // Messy math for interaction distance.
                Collider proxyCol = proxy.GetComponent<Collider>();
                Vector3 extents = proxyCol.bounds.extents;
                real.EndInteractionDistance = 2.5f * Mathf.Abs(Mathf.Max(extents.x, extents.y, extents.z));
            }
        }

        /// <summary>
        /// Creates valid AICoverPoints from proxies.
        /// </summary>
        private static void Resolve_AICoverPoints()
        {
            // NOTE: AICoverPoint currently isn't in the FistVR namespace.
            TNH.AICoverPoint[] coverProxies = loadedRoot.GetComponentsInChildren<TNH.AICoverPoint>();
            foreach (var proxy in coverProxies)
            {
                GameObject owner = proxy.gameObject;
                AICoverPoint real = owner.AddComponent<AICoverPoint>();

                // These seem to be constant, and Calc and CalcNew are an enigma.
                real.Heights = new float[] { 3f, 0.5f, 1.1f, 1.5f };
                real.Calc();
                real.CalcNew();
            }
        }

        /// <summary>
        /// Creates valid TNH_DestructibleBarrierPoints from proxies.
        /// </summary>
        private static void Resolve_TNH_DestructibleBarrierPoints()
        {
            TNH.TNH_DestructibleBarrierPoint[] coverProxies = loadedRoot.GetComponentsInChildren<TNH.TNH_DestructibleBarrierPoint>();
            foreach (var proxy in coverProxies)
            {
                GameObject owner = proxy.gameObject;
                FistVR.TNH_DestructibleBarrierPoint real = owner.AddComponent<FistVR.TNH_DestructibleBarrierPoint>();

                real.Obstacle = owner.GetComponent<UnityEngine.AI.NavMeshObstacle>();
                real.CoverPoints = real.GetComponentsInChildren<AICoverPoint>().ToList();
                real.BarrierDataSets = new List<FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet>();

                for (int ii = 0; ii < 2; ii++)
                {
                    FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet barrierSet = new FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet();
                    barrierSet.BarrierPrefab = barrierPrefabs[ii];
                    barrierSet.Points = new List<FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet.SavedCoverPointData>();

                    real.BarrierDataSets.Add(barrierSet);
                }

                // This should only be run in the editor, but OH WELL.
                real.BakePoints();
            }
        }

        /// <summary>
        /// Creates valid TNH_SupplyPoints from proxies.
        /// </summary>
        private static void Resolve_TNH_SupplyPoints()
        {
            TNH.TNH_SupplyPoint[] supplyProxies = loadedRoot.GetComponentsInChildren<TNH.TNH_SupplyPoint>();
            foreach (var proxy in supplyProxies)
            {
                GameObject owner = proxy.gameObject;
                FistVR.TNH_SupplyPoint real = owner.AddComponent<FistVR.TNH_SupplyPoint>();

                real.M = manager;
                real.Bounds = proxy.Bounds;
                real.CoverPoints = proxy.CoverPoints.AsEnumerable().Select(x => x.gameObject.GetComponent<AICoverPoint>()).ToList();
                real.SpawnPoint_PlayerSpawn = proxy.SpawnPoint_PlayerSpawn;
                real.SpawnPoints_Sosigs_Defense = proxy.SpawnPoints_Sosigs_Defense.AsEnumerable().ToList();
                real.SpawnPoints_Turrets = proxy.SpawnPoints_Turrets.AsEnumerable().ToList();
                real.SpawnPoints_Panels = proxy.SpawnPoints_Panels.AsEnumerable().ToList();
                real.SpawnPoints_Boxes = proxy.SpawnPoints_Boxes.AsEnumerable().ToList();
                real.SpawnPoint_Tables = proxy.SpawnPoint_Tables.AsEnumerable().ToList();
                real.SpawnPoint_CaseLarge = proxy.SpawnPoint_CaseLarge;
                real.SpawnPoint_CaseSmall = proxy.SpawnPoint_CaseSmall;
                real.SpawnPoint_Melee = proxy.SpawnPoint_Melee;
                real.SpawnPoints_SmallItem = proxy.SpawnPoints_SmallItem.AsEnumerable().ToList();
                real.SpawnPoint_Shield = proxy.SpawnPoint_Shield;
            }
        }

        /// <summary>
        /// Creates valid TNH_HoldPoints from proxies.
        /// </summary>
        private static void Resolve_TNH_HoldPoints()
        {
            TNH.TNH_HoldPoint[] holdProxies = loadedRoot.GetComponentsInChildren<TNH.TNH_HoldPoint>();
            foreach (var proxy in holdProxies)
            {
                GameObject owner = proxy.gameObject;
                FistVR.TNH_HoldPoint real = owner.AddComponent<FistVR.TNH_HoldPoint>();

                real.M = manager;
                real.Bounds = proxy.Bounds;
                real.NavBlockers = proxy.NavBlockers;
                real.BarrierPoints = proxy.BarrierPoints.AsEnumerable().Select(x => x.gameObject.GetComponent<FistVR.TNH_DestructibleBarrierPoint>()).ToList();
                real.CoverPoints = proxy.CoverPoints.AsEnumerable().Select(x => x.gameObject.GetComponent<AICoverPoint>()).ToList();
                real.SpawnPoint_SystemNode = proxy.SpawnPoint_SystemNode;
                real.SpawnPoints_Targets = proxy.SpawnPoints_Targets.AsEnumerable().ToList();
                real.SpawnPoints_Turrets = proxy.SpawnPoints_Turrets.AsEnumerable().ToList();
                real.AttackVectors = proxy.AttackVectors.AsEnumerable().Select(x => Resolve_AttackVector(x.GetComponent<TNH.AttackVector>())).ToList();
                real.SpawnPoints_Sosigs_Defense = proxy.SpawnPoints_Sosigs_Defense.AsEnumerable().ToList();

                real.AUDEvent_HoldWave = wave;
                real.AUDEvent_Success = success;
                real.AUDEvent_Failure = failure;
                real.VFX_HoldWave = vfx;
            }
        }

        /// <summary>
        /// Creates a valid H3VR AttackVector from a proxy.
        /// </summary>
        private static FistVR.TNH_HoldPoint.AttackVector Resolve_AttackVector(TNH.AttackVector proxy)
        {
            GameObject owner = proxy.gameObject;
            FistVR.TNH_HoldPoint.AttackVector real = new FistVR.TNH_HoldPoint.AttackVector();

            real.SpawnPoints_Sosigs_Attack = proxy.SpawnPoints_Sosigs_Attack;
            real.GrenadeVector = proxy.GrenadeVector;
            real.GrenadeRandAngle = proxy.GrenadeRandAngle;
            real.GrenadeVelRange = proxy.GrenadeVelRange;

            return real;
        }

        /// <summary>
        /// Places the H3VR scoreboard at the correct position.
        /// </summary>
        private static void Resolve_ScoreboardArea()
        {
            GameObject proxy = loadedRoot.GetComponentInChildren<TNH.ScoreboardArea>().gameObject;
            GameObject finalScore = currentScene.GetAllGameObjectsInScene().Where(x => x.name == "_FinalScore").First();
            GameObject resetPoint = currentScene.GetAllGameObjectsInScene().Where(x => x.name == "[ResetPoint]").First();

            resetPoint.transform.position = proxy.transform.position;
            finalScore.transform.position = proxy.transform.position + new Vector3(0f, 1.8f, 7.5f);

        }

        /// <summary>
        /// Base function for setting up the TNH Manager object to handle a custom level.
        /// </summary>
        private static void Fix_TNH_Manager()
        {
            // Hold points need to be set.
            manager.HoldPoints = loadedRoot.GetComponentsInChildren<FistVR.TNH_HoldPoint>().ToList();

            // Supply points need to be set.
            manager.SupplyPoints = loadedRoot.GetComponentsInChildren<FistVR.TNH_SupplyPoint>().ToList();

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

                sequence.StartSupplyPointIndex = UnityEngine.Random.Range(0, manager.SupplyPoints.Count);
                sequence.HoldPoints = new List<int>()
                {
                    //TODO This should only be 5, but I got an outofrange after the 4th hold so let's just add more... Maybe that'll fix it...
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count),
                    UnityEngine.Random.Range(0, manager.HoldPoints.Count),
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

            for (int ii = 0; ii < manager.HoldPoints.Count; ii++)
            {
                FistVR.TNH_SafePositionMatrix.PositionEntry entry = new FistVR.TNH_SafePositionMatrix.PositionEntry();
                entry.SafePositions_HoldPoints = new List<bool>();
                for (int jj = 0; jj < manager.HoldPoints.Count; jj++)
                {
                    entry.SafePositions_HoldPoints.Add(ii != jj);
                }

                entry.SafePositions_SupplyPoints = new List<bool>();
                for (int jj = 0; jj < manager.SupplyPoints.Count; jj++)
                {
                    entry.SafePositions_SupplyPoints.Add(ii != jj);
                }

                maxMatrix.Entries_HoldPoints.Add(entry);
            }

            for (int ii = 0; ii < manager.SupplyPoints.Count; ii++)
            {
                FistVR.TNH_SafePositionMatrix.PositionEntry entry = new FistVR.TNH_SafePositionMatrix.PositionEntry();
                entry.SafePositions_HoldPoints = new List<bool>();
                for (int jj = 0; jj < manager.HoldPoints.Count; jj++)
                {
                    entry.SafePositions_HoldPoints.Add(ii != jj);
                }

                entry.SafePositions_SupplyPoints = new List<bool>();
                for (int jj = 0; jj < manager.SupplyPoints.Count; jj++)
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
