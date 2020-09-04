using System;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;
using WurstMod.MappingComponents.TakeAndHold;

namespace WurstMod.UnityEditor.SceneExporters
{
    [SceneExporter("h3vr.take_and_hold")]
    public class TakeAndHoldSceneExporter : SceneExporter
    {
        public override void Validate(Scene scene, CustomScene root, ExportErrors err)
        {
            // Base validate
            base.Validate(scene, root, err);
            
            // Check for all required components
            RequiredComponents<Scoreboard>(1, 1);
            RequiredComponents<Respawn>(1, 1);
            RequiredComponents<TNH_HoldPoint>(2, int.MaxValue);
            RequiredComponents<TNH_SupplyPoint>(3, int.MaxValue);
            RequiredComponents<ForcedSpawn>(0, 1);
        }
    }
}