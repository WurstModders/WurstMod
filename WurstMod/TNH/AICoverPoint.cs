using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    /// <summary>
    /// AI take cover here. Place on floor, rotation doesn't matter.
    /// </summary>
    public class AICoverPoint : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.0f, 0.6f, 0.0f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
        }
    }
}
