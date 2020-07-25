using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.Any
{
    /// <summary>
    /// AI take cover here. Place on floor, rotation doesn't matter.
    /// </summary>
    public class AICoverPoint : ComponentProxy
    {
        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.0f, 0.6f, 0.0f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
        }

        public override int GetImportance() => 3;

        protected override bool InitializeComponent()
        {
            global::AICoverPoint real = gameObject.AddComponent<global::AICoverPoint>();

            // These seem to be constant, and Calc and CalcNew are an enigma.
            real.Heights = new float[] { 3f, 0.5f, 1.1f, 1.5f };
            real.Calc();
            real.CalcNew();

            return true;
        }
    }
}
