using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using WurstMod.UnityEditor;
#endif

namespace WurstMod.MappingComponents.Generic
{
    [RequireComponent(typeof(Collider))]
    public class HandTrigger : ComponentProxy
    {
        public UnityEvent Triggered;

        public void OnTriggerEnter(Collider other)
        {
            Triggered.Invoke();
        }

#if UNITY_EDITOR
        public override void OnExport(ExportErrors err)
        {
            // This needs to be a trigger on the interactable layer
            GetComponent<Collider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
#endif
    }
}