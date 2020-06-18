using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH.Extras
{
    class ForcedSpawn : MonoBehaviour
    {
        [Tooltip("True if this supply point ONLY be used for spawn, or false if it can spawn again as a supply point later.")]
        public bool spawnOnly = false;
    }
}
