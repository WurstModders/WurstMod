using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    /// <summary>
    /// Place this class on any geometry you want to be able to climb, like ladders!
    /// Climbable geometry must be in the Interactable layer.
    /// </summary>
    public class FVRHandGrabPoint : MonoBehaviour
    {
        [Tooltip("A DISABLED gameObject visualizing the ability to grab this object. This is the glowy bit when your hands are near a ladder.")]
        public GameObject UXGeo_Hover;

        [Tooltip("A DISABLED gameObject visualizing you grabbing this object. This is the glowy bit when you are grabbing a ladder.")]
        public GameObject UXGeo_Held;
    }
}
