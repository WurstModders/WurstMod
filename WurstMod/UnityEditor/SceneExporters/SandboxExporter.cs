#if UNITY_EDITOR
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;
using WurstMod.MappingComponents.Sandbox;
using WurstMod.Shared;

namespace WurstMod.UnityEditor.SceneExporters
{
    public class SandboxExporter : SceneExporter
    {
        public override string GamemodeId => Constants.GamemodeSandbox;

        public override void Validate(Scene scene, CustomScene root, ExportErrors err)
        {
            // Let the base validate
            base.Validate(scene, root, err);

            // Check for a spawn
            RequiredComponents<Spawn>(1, 1);
        }
    }
}
#endif