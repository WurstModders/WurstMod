using WurstMod.Runtime;

namespace WurstMod.SceneLoaders
{
    [CustomSceneLoader("Sandbox")]
    public class SandboxSceneLoader : CustomSceneLoader
    {
        public override string BaseScene => "ProvingGround";
    }
}