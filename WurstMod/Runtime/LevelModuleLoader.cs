using ADepIn;
using Deli;
using UnityEngine;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    [QuickNamedBind("Level")]
    public class LevelModuleLoader : IAssetLoader
    {
        public void LoadAsset(IServiceKernel kernel, Mod mod, string path)
        {
            // If the config has disabled loading the default included levels, return
            if (!Entrypoint.LoadDebugLevels.Value && mod.Info.Guid == "wurstmod")
                return;
            
            // Make sure it's a directory
            if (!path.EndsWith("/"))
                path += "/";
            
            // Try to make a level info from it
            var level = LevelInfo.FromFrameworkMod(mod, path);
            
            if (!level.HasValue) Debug.LogError($"Level in {mod}, {path} is not valid!");
            else
            {
                CustomLevelFinder.ArchiveLevels.Add(level.Value);
                Debug.Log($"Discovered level {level.Value.SceneName} in {mod}, {path}");
            }
        }
    }
}