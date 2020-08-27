using UnityEngine;
using WurstMod.Runtime;

namespace WurstMod.MappingComponents.TakeAndHold
{
    class Respawn : ComponentProxy
    {
        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0.8f, 0.5f), Vector3.zero, 0.25f, transform);
        }

        public override void InitializeComponent()
        {
            ObjectReferences.ResetPoint.transform.position = transform.position;

            Destroy(this);
        }
    }
}
