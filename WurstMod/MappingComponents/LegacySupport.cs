﻿using System;
using UnityEngine;

// We can maintain backwards compatibility like this. It's a little cheesy of course, but relatively unobtrusive.
namespace WurstMod.TNH
{
    [Obsolete] [AddComponentMenu("")] public class AICoverPoint : WurstMod.MappingComponents.Generic.AICoverPoint { }
    [Obsolete] [AddComponentMenu("")] public class AttackVector : WurstMod.MappingComponents.TakeAndHold.AttackVector { }
    [Obsolete] [AddComponentMenu("")] public class FVRHandGrabPoint : WurstMod.MappingComponents.Generic.FVRHandGrabPoint { }
    [Obsolete] [AddComponentMenu("")] public class FVRReverbEnvironment : WurstMod.MappingComponents.Generic.FVRReverbEnvironment { }
    [Obsolete] [AddComponentMenu("")] public class PMat : WurstMod.MappingComponents.Generic.PMat { }
    [Obsolete] [AddComponentMenu("")] public class ScoreboardArea : WurstMod.MappingComponents.TakeAndHold.ScoreboardArea { }
    [Obsolete] [AddComponentMenu("")] public class TNH_DestructibleBarrierPoint : WurstMod.MappingComponents.TakeAndHold.TNH_DestructibleBarrierPoint { }
    [Obsolete] [AddComponentMenu("")] public class TNH_HoldPoint : WurstMod.MappingComponents.TakeAndHold.TNH_HoldPoint { }
    [Obsolete] [AddComponentMenu("")] public class TNH_Level : WurstMod.MappingComponents.Generic.CustomScene 
    {
        public Material skybox;

        void Awake()
        {
            Skybox = skybox;
        }
    }
    [Obsolete] [AddComponentMenu("")] public class TNH_SupplyPoint : WurstMod.MappingComponents.TakeAndHold.TNH_SupplyPoint { }
}
namespace WurstMod.TNH.Extras
{
    [Obsolete] [AddComponentMenu("")] public class ForcedSpawn : WurstMod.MappingComponents.TakeAndHold.ForcedSpawn { }
}
namespace WurstMod.Generic
{
    [Obsolete] [AddComponentMenu("")] public class GenericPrefab : WurstMod.MappingComponents.Sandbox.GenericPrefab { }
    [Obsolete] [AddComponentMenu("")] public class GroundPanel : WurstMod.MappingComponents.Sandbox.GroundPanel { }
    [Obsolete] [AddComponentMenu("")] public class ItemSpawner : WurstMod.MappingComponents.Sandbox.GenericPrefab
    {
        void Awake()
        {
            objectType = MappingComponents.Sandbox.Prefab.ItemSpawner;
        }
    }
    [Obsolete] [AddComponentMenu("")] public class PointableButton : WurstMod.MappingComponents.Sandbox.PointableButton { }
    [Obsolete] [AddComponentMenu("")] public class Spawn : WurstMod.MappingComponents.Sandbox.Spawn { }
}
namespace WurstMod.Any
{
    [Obsolete] [AddComponentMenu("")] public class AICoverPoint : WurstMod.MappingComponents.Generic.AICoverPoint { }
    [Obsolete] [AddComponentMenu("")] public class AnvilPrefab : WurstMod.MappingComponents.Generic.AnvilPrefab { }
    [Obsolete] [AddComponentMenu("")] public class FVRHandGrabPoint : WurstMod.MappingComponents.Generic.FVRHandGrabPoint { }
    [Obsolete] [AddComponentMenu("")] public class FVRReverbEnvironment : WurstMod.MappingComponents.Generic.FVRReverbEnvironment { }
    [Obsolete] [AddComponentMenu("")] public class PMat : WurstMod.MappingComponents.Generic.PMat { }
    [Obsolete] [AddComponentMenu("")] public class Target : WurstMod.MappingComponents.Generic.Target { }
    [Obsolete] [AddComponentMenu("")] public class Trigger : WurstMod.MappingComponents.Generic.Trigger { }
}