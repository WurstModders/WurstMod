using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;
using WurstMod.MappingComponents.Sandbox;

namespace WurstMod.UnityEditor.SceneExporters
{
    [SceneExporter("h3vr.sandbox")]
    public class SandboxSceneSceneExporter : SceneExporter
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