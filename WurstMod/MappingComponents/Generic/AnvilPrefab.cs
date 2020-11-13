using FistVR;
using UnityEngine;
using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.MappingComponents.Generic
{
    public class AnvilPrefab : ComponentProxy
    {
        // Inspector
        // Using the WeaponStuff enum covers most of the stuff people probably want to instantiate,
        // but not all of it. TODO reconsider this implementation.
        public ResourceDefs.AnvilAsset prefab;
        public bool spawnOnSceneLoad = false;
        
        public override void InitializeComponent()
        {
            if (spawnOnSceneLoad) Spawn();
        }

        /// <summary>
        /// Spawn the object this marker describes.
        /// You can call this from a trigger!
        /// </summary>
        public void Spawn()
        {
            Debug.Log("Loading Anvil Asset: " + ResourceDefs.AnvilAssetResources[prefab]);
            FVRObject obj = Resources.Load<FVRObject>(ResourceDefs.AnvilAssetResources[prefab]);
            GameObject go = Instantiate<GameObject>(obj.GetGameObject(), transform.position, transform.rotation, ObjectReferences.CustomScene.transform);
            go.SetActive(true);
        }

    }
}