using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.Generic
{
    [Obsolete("This class has been replaced with GenericPrefab, a much more extensible system with more options.")]
    class ItemSpawner : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 1.5f, 0.25f), new Vector3(2.3f, 1.2f, 0.5f), Vector3.forward, transform);
        }
    }
}
