using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.SceneLoaders
{
    public class SandboxSceneLoader : CustomSceneLoader
    {
        public override string GamemodeId => Constants.GamemodeSandbox;
        public override string BaseScene => "ProvingGround";
    }
}