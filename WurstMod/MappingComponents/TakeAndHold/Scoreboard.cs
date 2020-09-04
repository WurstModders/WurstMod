using UnityEngine;
using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.MappingComponents.TakeAndHold
{
    class Scoreboard : ComponentProxy
    {
        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 0f, 0f), new Vector3(2f, 2f, 0.1f), Vector3.back, transform);
            transform.Rotate(0, -45, 0);
            Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(-1.771f, 0f, 0.743f), new Vector3(2f, 2f, 0.1f), Vector3.back, transform);
            transform.Rotate(0, 45, 0);
        }

        public override void InitializeComponent()
        {
            ObjectReferences.FinalScore.transform.position = transform.position;

            Destroy(this);
        }


    }
}
