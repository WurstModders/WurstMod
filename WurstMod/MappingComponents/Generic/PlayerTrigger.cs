using UnityEngine;
using UnityEngine.Events;
using WurstMod.UnityEditor;

namespace WurstMod.MappingComponents.Generic
{
    [RequireComponent(typeof(Collider))]
    public class PlayerTrigger : ComponentProxy
    {
        public UnityEvent Enter;
        public UnityEvent Exit;

        private void OnTriggerEnter(Collider other) => Enter.Invoke();

        private void OnTriggerExit(Collider other) => Exit.Invoke();

        public override void OnExport(ExportErrors err)
        {
            // We needs this to be a trigger on the ColOnlyHead layer.
            GetComponent<Collider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("ColOnlyHead");
        }
    }
}