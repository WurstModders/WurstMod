using System.Collections;
using FistVR;
using UnityEngine;
using WurstMod.Runtime;

namespace WurstMod.MappingComponents.Generic
{
    public class ItemSpawner : ComponentProxy
    {
        public string ObjectId;
        public bool SpawnOnLoad;

        public override void InitializeComponent()
        {
            if (SpawnOnLoad) Spawn();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.0f, 0.0f, 0.6f, 0.5f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawSphere(Vector3.zero, 0.1f);
        }

        /// <summary>
        /// Spawn the object this marker describes.
        /// You can call this from a trigger!
        /// </summary>
        public void Spawn()
        {
            StartCoroutine(SpawnAsync());
        }

        private IEnumerator SpawnAsync()
        {
#if !UNITY_EDITOR
            FVRObject obj = IM.OD[ObjectId];
            var callback = obj.GetGameObjectAsync();
            yield return callback;
            GameObject go = Instantiate(callback.Result, transform.position, transform.rotation,
                ObjectReferences.CustomScene.transform);
            go.SetActive(true);
#else
            yield return null;
#endif
        }
    }
}