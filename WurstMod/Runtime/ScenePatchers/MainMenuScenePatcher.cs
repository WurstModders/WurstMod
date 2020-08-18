using UnityEngine;

namespace WurstMod.Runtime.ScenePatchers
{
    [ScenePatcher("MainMenu3")]
    public class MainMenuScenePatcher : ScenePatcher
    {
        public override void PatchScene()
        {
            // TODO: Create new menu!
            Debug.Log("Scene was patched!");
        }
    }
}