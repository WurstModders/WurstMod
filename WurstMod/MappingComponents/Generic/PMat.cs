using UnityEngine;
using WurstMod.Shared;

namespace WurstMod.MappingComponents.Generic
{
    public class PMat : ComponentProxy
    {
        [Tooltip("Not all static colliders (anything a bullet might hit) have a def. Is this for penetrative properties? Legacy?")]
        public ResourceDefs.PMatAsset Def;
        [Tooltip("It seems like all static colliders (anything a bullet might hit) have a matDef.")]
        public ResourceDefs.MatDefAsset MatDef;


        public override void InitializeComponent()
        {
            FistVR.PMat real = gameObject.AddComponent<FistVR.PMat>();

            real.Def = ResourceDefs.PMatAssetResources.ContainsKey(Def) ? Resources.Load<FistVR.PMaterialDefinition>(ResourceDefs.PMatAssetResources[Def]) : null;
            real.MatDef = ResourceDefs.MatDefAssetResources.ContainsKey(MatDef) ? Resources.Load<FistVR.MatDef>(ResourceDefs.MatDefAssetResources[MatDef]) : null;

            Destroy(this);
        }
    }
}
