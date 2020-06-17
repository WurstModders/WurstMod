using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    public class TNH_Level : MonoBehaviour
    {
        // Used by exporter to generate an info file I guess?
        public string levelName;
        public string levelAuthor;

        [TextArea(15, 20)]
        public string levelDescription;

        public Material skybox;
    }
}
