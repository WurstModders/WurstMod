using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    public class ScoreboardArea : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            Extensions.GenericGizmoCube(new Color(0.4f, 0.4f, 0.9f, 0.5f), new Vector3(0f, 1.5f, 0f), new Vector3(5f, 3f, 5f), Vector3.back, transform);
        }
    }
}
