using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;
using WurstMod.MappingComponents.Sandbox;
using WurstMod.Shared;

namespace WurstMod.UnityEditor.SceneExporters
{
    [SceneExporter(Constants.GamemodeSandbox)]
    public class SandboxExporter : SceneExporter
    {
        public override void Validate(Scene scene, CustomScene root, ExportErrors err)
        {
            // Let the base validate
            base.Validate(scene, root, err);

            // Check for a spawn
            RequiredComponents<Spawn>(1, 1);
        }
    }
}