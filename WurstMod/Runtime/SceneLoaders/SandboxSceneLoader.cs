using System.Collections.Generic;
using System.Linq;
using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.SceneLoaders
{
    public class SandboxSceneLoader : CustomSceneLoader
    {
        public override string GamemodeId => Constants.GamemodeSandbox;
        public override string BaseScene => "ProvingGround";

        public override IEnumerable<string> EnumerateDestroyOnLoad()
        {
            
            return base.EnumerateDestroyOnLoad().Concat(new[]
            {
                "_Animator_Spawning_",
                "_Boards",
                "_Env",
                "_AmbientAudio",
                "AILadderTest1",
                // TODO: Should probably remove all the Anvil Prefabs, but it causes errors...
                //"__SpawnOnLoad",
            });
        }
    }
}