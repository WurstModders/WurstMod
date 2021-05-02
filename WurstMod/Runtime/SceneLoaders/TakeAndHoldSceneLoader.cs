using System.Collections.Generic;
using System.Linq;
using FistVR;
using UnityEngine;
using WurstMod.MappingComponents.TakeAndHold;
using WurstMod.Shared;
using TNH_HoldPoint = FistVR.TNH_HoldPoint;
using TNH_SupplyPoint = FistVR.TNH_SupplyPoint;

namespace WurstMod.Runtime.SceneLoaders
{
    public class TakeAndHoldSceneLoader : CustomSceneLoader
    {
        private TNH_Manager _tnhManager;

        public override string GamemodeId => Constants.GamemodeTakeAndHold;
        public override string BaseScene => "TakeAndHoldClassic";

        public override IEnumerable<string> EnumerateDestroyOnLoad()
        {
            return base.EnumerateDestroyOnLoad().Concat(new[]
            {
                // Take and Hold objects
                "HoldPoint_",
                "Ladders",
                "Lighting",
                "OpenArea",
                "RampHelperCubes",
                "ReflectionProbes",
                "SupplyPoint_",
                "Tiles"
            });
        }

        /// <summary>
        /// Base function for setting up the TNH Manager object to handle a custom level.
        /// </summary>
        public override void Resolve()
        {
            _tnhManager = ObjectReferences.ManagerDonor;

            // Hold points need to be set.
            _tnhManager.HoldPoints = LevelRoot.GetComponentsInChildren<TNH_HoldPoint>(true).ToList();

            // Supply points need to be set.
            _tnhManager.SupplyPoints = LevelRoot.GetComponentsInChildren<TNH_SupplyPoint>(true).ToList();

            // Possible Sequences need to be generated at random.
            if (LevelRoot.ExtraData == null || LevelRoot.ExtraData[0].Value == "") _tnhManager.PossibleSequnces = GenerateRandomPointSequences(1);

            // Safe Pos Matrix needs to be set. Diagonal for now.
            TNH_SafePositionMatrix maxMatrix = GenerateTestMatrix();
            _tnhManager.SafePosMatrix = maxMatrix;
        }

        /// <summary>
        /// Regular gamemode uses a preset list of possible hold orders. This creates a bunch randomly.
        /// </summary>
        private List<TNH_PointSequence> GenerateRandomPointSequences(int count)
        {
            List<TNH_PointSequence> sequences = new List<TNH_PointSequence>();
            for (int ii = 0; ii < count; ii++)
            {
                TNH_PointSequence sequence = ScriptableObject.CreateInstance<TNH_PointSequence>();

                // Logic for forced spawn location.
                ForcedSpawn forcedSpawn = LevelRoot.GetComponentInChildren<ForcedSpawn>();
                if (forcedSpawn != null)
                {
                    sequence.StartSupplyPointIndex = _tnhManager.SupplyPoints.IndexOf(_tnhManager.SupplyPoints.First(x => x.gameObject == forcedSpawn.gameObject));
                }
                else
                {
                    sequence.StartSupplyPointIndex = Random.Range(0, _tnhManager.SupplyPoints.Count);
                }

                // If the mapper hasn't set a custom hold order
                if (LevelRoot.ExtraData == null || LevelRoot.ExtraData[0].Value == "")
                    sequence.HoldPoints = new List<int>
                    {
                        Random.Range(0, _tnhManager.HoldPoints.Count),
                        Random.Range(0, _tnhManager.HoldPoints.Count),
                        Random.Range(0, _tnhManager.HoldPoints.Count),
                        Random.Range(0, _tnhManager.HoldPoints.Count),
                        Random.Range(0, _tnhManager.HoldPoints.Count)
                    };
                else
                {
                    var namedOrder = LevelRoot.ExtraData[0].Value.Split(',');
                    sequence.HoldPoints = namedOrder.Select(x => _tnhManager.HoldPoints.FindIndex(y => y.name == x)).ToList();
                }

                // Fix sequence, because they may generate the same point after the current point, IE {1,4,4)
                // This would break things.
                for (int jj = 0; jj < sequence.HoldPoints.Count - 1; jj++)
                {
                    if (sequence.HoldPoints[jj] == sequence.HoldPoints[jj + 1])
                    {
                        sequence.HoldPoints[jj + 1] = (sequence.HoldPoints[jj + 1] + 1) % _tnhManager.HoldPoints.Count;
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
        private TNH_SafePositionMatrix GenerateTestMatrix()
        {
            TNH_SafePositionMatrix maxMatrix = ScriptableObject.CreateInstance<TNH_SafePositionMatrix>();
            maxMatrix.Entries_HoldPoints = new List<TNH_SafePositionMatrix.PositionEntry>();
            maxMatrix.Entries_SupplyPoints = new List<TNH_SafePositionMatrix.PositionEntry>();

            int effectiveHoldCount = _tnhManager.HoldPoints.Count;
            int effectiveSupplyCount = _tnhManager.SupplyPoints.Where(x => x.GetComponent<ForcedSpawn>() == null || x.GetComponent<ForcedSpawn>().spawnOnly == false).Count();
            //int effectiveSupplyCount = _tnhManager.SupplyPoints.Count;

            for (int ii = 0; ii < effectiveHoldCount; ii++)
            {
                TNH_SafePositionMatrix.PositionEntry entry = new TNH_SafePositionMatrix.PositionEntry();
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
                TNH_SafePositionMatrix.PositionEntry entry = new TNH_SafePositionMatrix.PositionEntry();
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
    }
}