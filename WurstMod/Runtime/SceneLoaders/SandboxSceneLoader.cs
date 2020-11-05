using WurstMod.Runtime;
using WurstMod.Shared;

namespace WurstMod.SceneLoaders
{
    [CustomSceneLoader(Constants.GamemodeSandbox)]
    public class SandboxSceneLoader : CustomSceneLoader
    {
        public override string BaseScene => "ProvingGround";
    }
}