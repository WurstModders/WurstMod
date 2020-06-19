using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.Generic
{
    public class Spawn : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            Extensions.GenericGizmoSphere(new Color(0.0f, 0.8f, 0.8f, 0.5f), Vector3.zero, 0.25f, transform);
        }
    }
}
