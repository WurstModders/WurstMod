using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using WurstMod.UnityEditor;
#endif
namespace WurstMod.MappingComponents.Generic
{
    [RequireComponent(typeof(Collider))]
    public class PlayerTrigger : ComponentProxy
    {
        public UnityEvent Enter;
        public UnityEvent Exit;

        private int _inTrigger;

        private void OnTriggerEnter(Collider other)
        {
            if (_inTrigger != 0) return;
            Enter.Invoke();
            _inTrigger++;
        }

        private void OnTriggerExit(Collider other)
        {
            _inTrigger--;
            if (_inTrigger != 0) return;
            Exit.Invoke();
        }

#if UNITY_EDITOR
        public override void OnExport(ExportErrors err)
        {
            // We needs this to be a trigger on the ColOnlyHead layer.
            GetComponent<Collider>().isTrigger = true;
            gameObject.layer = LayerMask.NameToLayer("ColOnlyHead");
        }
#endif
    }
}