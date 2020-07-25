using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.Generic
{
    public enum Prefab { ItemSpawner, Destructobin, SosigSpawner, WhizzBangADinger, WhizzBangADingerDetonator}
    class GenericPrefab : ComponentProxy
    {
        /// <summary>
        /// Select what kind of object this is, the ghost will update to match.
        /// </summary>
        public Prefab objectType;

        void OnDrawGizmos()
        {
            switch (objectType)
            {
                case Prefab.ItemSpawner: 
                    Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 0.7f, 0.25f), new Vector3(2.3f, 1.2f, 0.5f), Vector3.forward, transform);
                    break;
                case Prefab.Destructobin:
                    Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 0.525f, 0f), new Vector3(0.65f, 1.05f, 0.65f), Vector3.left, transform);
                    break;
                case Prefab.SosigSpawner:
                    Extensions.GenericGizmoCube(new Color(0.9f, 0.4f, 0.4f, 0.5f), new Vector3(0f, 0.11f, 0f), new Vector3(0.3f, 0.43f, 0.05f), Vector3.back, transform);
                    break;
                case Prefab.WhizzBangADinger:
                    Extensions.GenericGizmoCube(new Color(0.9f, 0.9f, 0.4f, 0.5f), new Vector3(0f, 0.425f, -0.05f), new Vector3(0.4f, 0.85f, 0.5f), Vector3.forward, transform);
                    break;
                case Prefab.WhizzBangADingerDetonator:
                    Extensions.GenericGizmoCube(new Color(0.9f, 0.9f, 0.4f, 0.5f), new Vector3(0f, 0f, 0.1f), new Vector3(0.05f, 0.05f, 0.33f), Vector3.forward, transform);
                    break;
            }
            
        }


        private static Dictionary<Generic.Prefab, GameObject> baseObjects = new Dictionary<Generic.Prefab, GameObject>();
        protected override bool InitializeComponent()
        {
            if (baseObjects.Count == 0)
            {
                // Initialize dictionary of baseObjects.
                void Add(Generic.Prefab type, GameObject obj)
                {
                    baseObjects[type] = obj;
                    baseObjects[type].SetActive(false);
                }

                Add(Generic.Prefab.ItemSpawner, ObjectReferences.ItemSpawnerDonor);
                Add(Generic.Prefab.Destructobin, ObjectReferences.DestructobinDonor);
                Add(Generic.Prefab.SosigSpawner, ObjectReferences.SosigSpawnerDonor);
                Add(Generic.Prefab.WhizzBangADinger, ObjectReferences.WhizzBangADingerDonor);
                Add(Generic.Prefab.WhizzBangADingerDetonator, ObjectReferences.BangerDetonatorDonor);
            }
            GameObject copy = GameObject.Instantiate(baseObjects[objectType], ObjectReferences.Level.transform);
            copy.transform.position = transform.position;
            copy.transform.localEulerAngles = transform.localEulerAngles;
            copy.SetActive(true);

            return true;
        }
    }
}
