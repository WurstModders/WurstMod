using System;
using System.IO;
using System.Linq;
using Deli.VFS;
using UnityEngine;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public class LegacySupport
    {
        // I'm VERY explicit with namespaces in this file because strange things can happen if a stray using 
        // gets thrown in while working with classes of the same name like this.
        // Not required, but it feels more sane (albeit verbose.)

        // The following arrays map the old enum ordering to the new hash-based future-proof ordering.
        public static readonly int[] pMatLegacyMapping = new int[]
        {
            -1389548697,
            339799954,
            -1592592720,
            -1228112709,
            -1170414547,
            -1989712078,
            2012074650,
            1273286112,
            -1234456471,
            -1946916951,
            -725349196,
            1686857538,
            -65249002,
            -955345693,
            526415300,
            -2059344742,
            746711929,
            -858280334,
            1001966363,
            1428605659,
            -728703673,
            1178975115,
            -1261570224,
            -1261569134,
            -751390192,
            -466768857,
            1400949664,
            -158744292,
            -1788895460,
            -195832731,
            324251733,
            -1698391924,
            1984043213,
            370905090,
            -566556950,
            1287532213,
            1273826288,
            -2146462032,
            762136440,
            1505815014,
            -582586173,
            -724419178,
            255480758,
            1556236128,
            624679819,
            660109706,
            -2146336943,
            -1779147911,
            -151837501
        };
        public static readonly int[] matDefLegacyMapping = new int[]
        {
            1752877821,
            -1347286364,
            644301452,
            -1669265951,
            -871205235,
            -635266500,
            332398969,
            1273826288,
            1985703140,
            1729971855,
            -1675716207,
            1942859388,
            -1487240928,
            383750667,
            432136527,
            1211416655,
            -34193051,
            1943035998,
            -1149654923,
            -649847075,
            610581220,
            942411018,
            2144447471,
            -1896619314,
            1242992293,
            -134865591,
            1827179403,
            1461872896,
            1417815362,
            -1779147911,
            -1661987665,
            -1942452169,
            757445278,
            1483923932,
            -1690371124,
            1213917816,
            434755139,
            1335110134,
            1960591488,
            666790754,
            -1853667375,
            -321340768,
            1725752409,
            -529192027,
            1454812933,
            -213122109,
            892418936,
            -1898909600,
            -1258062406,
            -1217966965,
            761218943,
            1081916419,
            -1271050433,
            10600013,
            -1779142660
        };

        public static void EnsureLegacyFolderExists(IFileHandle legacyManifest)
        {
            var manifest = Path.Combine(Constants.LegacyLevelsDirectory, "manifest.json");
            if (File.Exists(manifest)) return;
            Directory.CreateDirectory(Constants.LegacyLevelsDirectory);
            Directory.CreateDirectory(Path.Combine(Constants.LegacyLevelsDirectory, "TakeAndHold"));
            Directory.CreateDirectory(Path.Combine(Constants.LegacyLevelsDirectory, "Other"));
            
            var stream = legacyManifest.OpenRead();
            var buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            File.WriteAllBytes(manifest, buffer);
        }
    }
}

// We can maintain backwards compatibility like this. It's a little cheesy of course, but relatively unobtrusive.
namespace WurstMod.TNH
{
    [Obsolete] [AddComponentMenu("")] public class AICoverPoint : WurstMod.MappingComponents.Generic.AICoverPoint { }
    [Obsolete] [AddComponentMenu("")] public class AttackVector : WurstMod.MappingComponents.TakeAndHold.AttackVector { }
    [Obsolete] [AddComponentMenu("")] public class FVRHandGrabPoint : WurstMod.MappingComponents.Generic.FVRHandGrabPoint { }
    [Obsolete] [AddComponentMenu("")] public class FVRReverbEnvironment : WurstMod.MappingComponents.Generic.FVRReverbEnvironment { }
    [Obsolete] [AddComponentMenu("")] public class PMat : WurstMod.MappingComponents.Generic.PMat 
    {
        void Awake()
        {
            def = (WurstMod.Shared.ResourceDefs.PMat)WurstMod.Runtime.LegacySupport.pMatLegacyMapping[(int)def];
            matDef = (WurstMod.Shared.ResourceDefs.MatDef)WurstMod.Runtime.LegacySupport.matDefLegacyMapping[(int)matDef];
        }
    }
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
    [Obsolete] [AddComponentMenu("")] public class AnvilPrefab : WurstMod.MappingComponents.Generic.AnvilPrefab 
    {
        public string Guid;
        public string Bundle;
        public string AssetName;

        void Awake()
        {
            spawnOnSceneLoad = true;

            string[] enumNames = Enum.GetNames(typeof(WurstMod.Shared.ResourceDefs.AnvilAsset));
            string correctEnumName = enumNames.Where(x => x.EndsWith(AssetName)).FirstOrDefault();
            if (!string.IsNullOrEmpty(correctEnumName))
            {
                prefab = (WurstMod.Shared.ResourceDefs.AnvilAsset)Enum.Parse(typeof(WurstMod.Shared.ResourceDefs.AnvilAsset), correctEnumName);
            }
        }
    }
    [Obsolete] [AddComponentMenu("")] public class FVRHandGrabPoint : WurstMod.MappingComponents.Generic.FVRHandGrabPoint { }
    [Obsolete] [AddComponentMenu("")] public class FVRReverbEnvironment : WurstMod.MappingComponents.Generic.FVRReverbEnvironment { }
    [Obsolete] [AddComponentMenu("")] public class PMat : WurstMod.MappingComponents.Generic.PMat
    {
        void Awake()
        {
            def = (WurstMod.Shared.ResourceDefs.PMat)WurstMod.Runtime.LegacySupport.pMatLegacyMapping[(int)def];
            matDef = (WurstMod.Shared.ResourceDefs.MatDef)WurstMod.Runtime.LegacySupport.matDefLegacyMapping[(int)matDef];
        }
    }
    [Obsolete] [AddComponentMenu("")] public class Target : WurstMod.MappingComponents.Generic.Target { }
    [Obsolete] [AddComponentMenu("")] public class Trigger : WurstMod.MappingComponents.Generic.Trigger { }
}
