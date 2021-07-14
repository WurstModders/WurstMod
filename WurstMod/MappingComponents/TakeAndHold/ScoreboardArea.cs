using System;
using UnityEngine;
using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.MappingComponents.TakeAndHold
{
    [Obsolete]
    [AddComponentMenu("")]
    public class ScoreboardArea : ComponentProxy
    {
        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 1.5f, 0f), new Vector3(5f, 3f, 5f), Vector3.back, transform);
        }

        public override void InitializeComponent()
        {
            ObjectReferences.ResetPoint.transform.position = transform.position;
            ObjectReferences.ManagerDonor.ScoreDisplay.transform.position = transform.position + new Vector3(0f, 1.8f, 7.5f);

            Destroy(this);
        }
    }
}
