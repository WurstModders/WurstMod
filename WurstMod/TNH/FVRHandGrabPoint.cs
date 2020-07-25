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
    [Obsolete("This class has been moved to the WurstMod.Any namespace, use that script instead!")]
    [AddComponentMenu("")]
    public class FVRHandGrabPoint : ComponentProxy
    {
        [Tooltip("A DISABLED gameObject visualizing the ability to grab this object. This is the glowy bit when your hands are near a ladder.")]
        public GameObject UXGeo_Hover;

        [Tooltip("A DISABLED gameObject visualizing you grabbing this object. This is the glowy bit when you are grabbing a ladder.")]
        public GameObject UXGeo_Held;

        protected override bool InitializeComponent()
        {
            FistVR.FVRHandGrabPoint real = gameObject.AddComponent<FistVR.FVRHandGrabPoint>();

            real.UXGeo_Hover = UXGeo_Hover;
            real.UXGeo_Held = UXGeo_Held;
            real.PositionInterpSpeed = 1;
            real.RotationInterpSpeed = 1;

            // Messy math for interaction distance.
            Collider proxyCol = GetComponent<Collider>();
            Vector3 extents = proxyCol.bounds.extents;
            real.EndInteractionDistance = 2.5f * Mathf.Abs(Mathf.Max(extents.x, extents.y, extents.z));

            return true;
        }
    }
}
