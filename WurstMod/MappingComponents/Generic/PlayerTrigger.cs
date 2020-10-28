using System;
using UnityEngine;
using UnityEngine.Events;

namespace WurstMod.MappingComponents.Generic
{
    public class PlayerTrigger : MonoBehaviour
    {
        public UnityEvent Enter;
        public UnityEvent Exit;

        private void OnTriggerEnter(Collider other) => Enter.Invoke();

        private void OnTriggerExit(Collider other) => Exit.Invoke();
    }
}