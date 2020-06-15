using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    public class TNH_DestructibleBarrierPoint : MonoBehaviour
    {
        [Tooltip("All cover points which become active when this barrier exists.")]
        public List<AICoverPoint> CoverPoints;

        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoCube(new Color(0.0f, 0.6f, 0.0f, 0.5f), new Vector3(0, 1, 0), new Vector3(1, 2, 0.1f), false, transform);
        }
    }
}
