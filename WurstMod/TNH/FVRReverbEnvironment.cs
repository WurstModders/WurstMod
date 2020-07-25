using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.TNH
{
    /// <summary>
    /// Placed on an object with a non-trigger box collider, on the NoCol layer.
    /// </summary>
    [Obsolete("This class has been moved to the WurstMod.Any namespace, use that script instead!")]
    [AddComponentMenu("")]
    public class FVRReverbEnvironment : ComponentProxy
    {
        public enum FVRSoundEnvironment
        {
            None = 0,
            Forest = 1,
            InsideNarrow = 10,
            InsideSmall = 11,
            InsideWarehouse = 12,
            InsideNarrowSmall = 13,
            InsideLarge = 14,
            InsideWarehouseSmall = 15,
            InsideMedium = 16,
            InsideLargeHighCeiling = 17,
            OutsideOpen = 20,
            OutsideEnclosed = 21,
            OutsideEnclosedNarrow = 22,
            SniperRange = 30,
            ShootingRange = 31,
        }

        public FVRSoundEnvironment Environment;
        public int Priority;

        protected override bool InitializeComponent()
        {
            FistVR.FVRReverbEnvironment real = gameObject.AddComponent<FistVR.FVRReverbEnvironment>();

            real.Environment = (FistVR.FVRSoundEnvironment)Environment;
            real.Priority = Priority;

            return true;
        }
    }
}
