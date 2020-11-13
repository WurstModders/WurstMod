using H3ModFramework;
using UnityEngine;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    [ModuleLoader(Name = "Level")]
    public class LevelModuleLoader : IModuleLoader
    {
        public void LoadModule(ModInfo mod, ModInfo.ModuleInfo module)
        {
            // If the config has disabled loading the default included levels, return
            if (!Entrypoint.LoadDebugLevels.Value && mod.Guid == "wurstmod")
                return;
            
            // Make sure it's a directory
            if (!module.Path.EndsWith("/"))
                module.Path += "/";
            
            // Try to make a level info from it
            var level = LevelInfo.FromFrameworkMod(mod, module);
            
            if (!level.HasValue) Debug.LogError($"Level in {mod}, {module} is not valid!");
            else
            {
                CustomLevelFinder.ArchiveLevels.Add(level.Value);
                Debug.Log($"Discovered level {level.Value.SceneName} in {mod}, {module}");
            }
        }
    }
}