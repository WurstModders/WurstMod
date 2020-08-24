using UnityEngine;

namespace WurstMod.MappingComponents.TakeAndHold
{
    public class ForcedSpawn : MonoBehaviour
    {
        [Tooltip("True if this supply point ONLY be used for spawn, or false if it can spawn again as a supply point later.")]
        public bool spawnOnly = false;
    }
}
