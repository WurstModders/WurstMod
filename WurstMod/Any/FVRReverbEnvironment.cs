using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace WurstMod.Any
{
    /// <summary>
    /// Placed on an object with a non-trigger box collider, on the NoCol layer.
    /// </summary>
    public class FVRReverbEnvironment : MonoBehaviour
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

    }
}
