using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WurstMod.Runtime;

namespace WurstMod.MappingComponents.TakeAndHold
{
    public class TNH_DestructibleBarrierPoint : ComponentProxy
    {
        [Tooltip("All cover points which become active when this barrier exists.")]
        public List<AICoverPoint> CoverPoints;

        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoCube(new Color(0.0f, 0.6f, 0.0f, 0.5f), new Vector3(0, 1, 0), new Vector3(1, 2, 0.1f), Vector3.zero, transform);
        }

        public override int ResolveOrder => 2;

        public override void InitializeComponent()
        {
            FistVR.TNH_DestructibleBarrierPoint real = gameObject.AddComponent<FistVR.TNH_DestructibleBarrierPoint>();

            real.Obstacle = gameObject.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            real.CoverPoints = real.GetComponentsInChildren<global::AICoverPoint>(true).ToList();
            real.BarrierDataSets = new List<FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet>();

            for (int ii = 0; ii < 2; ii++)
            {
                FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet barrierSet = new FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet();
                barrierSet.BarrierPrefab = ObjectReferences.BarrierDonor.BarrierDataSets[ii].BarrierPrefab;
                barrierSet.Points = new List<FistVR.TNH_DestructibleBarrierPoint.BarrierDataSet.SavedCoverPointData>();

                real.BarrierDataSets.Add(barrierSet);
            }

            // This should only be run in the editor, but OH WELL.
            real.BakePoints();

            Destroy(this);
        }
    }
}
