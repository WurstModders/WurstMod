using WurstMod.Runtime;

namespace WurstMod.SceneLoaders
{
    [CustomSceneLoader("h3vr.sandbox")]
    public class SandboxSceneLoader : CustomSceneLoader
    {
        public override string BaseScene => "ProvingGround";
    }
}