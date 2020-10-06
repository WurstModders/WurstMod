using UnityEngine;
using WurstMod.Shared;

namespace WurstMod.MappingComponents.Generic
{
    public class PMat : ComponentProxy
    {
        [Tooltip("Not all static colliders (anything a bullet might hit) have a def. Is this for penetrative properties? Legacy?")]
        public ResourceDefs.PMat def;
        [Tooltip("It seems like all static colliders (anything a bullet might hit) have a matDef.")]
        public ResourceDefs.MatDef matDef;


        public override void InitializeComponent()
        {
            FistVR.PMat real = gameObject.AddComponent<FistVR.PMat>();

            real.Def = ResourceDefs.PMatResources.ContainsKey(def) ? Resources.Load<FistVR.PMaterialDefinition>(ResourceDefs.PMatResources[def]) : null;
            real.MatDef = ResourceDefs.MatDefResources.ContainsKey(matDef) ? Resources.Load<FistVR.MatDef>(ResourceDefs.MatDefResources[matDef]) : null;

            Destroy(this);
        }
    }
}
