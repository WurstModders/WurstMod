using UnityEngine;

namespace WurstMod.Generic
{
    public class Spawn : ComponentProxy
    {
        void OnDrawGizmos()
        {
            Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0.8f, 0.5f), Vector3.zero, 0.25f, transform);
        }

        public override void InitializeComponent()
        {
            ObjectReferences.CameraRig.transform.position = transform.position;
            Destroy(this);
        }
    }
}
