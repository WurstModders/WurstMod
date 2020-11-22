using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.SceneLoaders
{
    public class SandboxSceneLoader : CustomSceneLoader
    {
        public override string GamemodeId => Constants.GamemodeSandbox;
        public override string BaseScene => "ProvingGround";

        public override string[] DestroyOnLoad => new[]
        {
            "_Animator_Spawning_",
            "_Boards",
            "_Env",
            "AILadderTest1",
            // TODO: Should probably remove all the Anvil Prefabs, but it causes errors...
            //"__SpawnOnLoad",
        };
    }
}