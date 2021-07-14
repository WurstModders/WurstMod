using FistVR;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public class ModdedLevelInfo : TNH_UIManager.LevelData
    {
        public ModdedLevelInfo(LevelInfo level)
        {
            IsModLevel = true;
            LevelAuthor = level.Author;
            LevelDescription = level.Description;
            LevelDisplayName = level.SceneName;
            LevelID = level.SceneName.Replace(" ", "").Truncate(16);
            LevelImage = level.Sprite;
            Original = level;
            LevelSceneName = "TakeAndHoldClassic";
        }
        
        public LevelInfo Original;
    }
}