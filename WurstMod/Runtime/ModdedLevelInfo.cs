using FistVR;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public class ModdedLevelInfo : TNH_UIManager.LevelData
    {
        public ModdedLevelInfo(LevelInfo level)
        {
#if !UNITY_EDITOR
            IsModLevel = true;
            LevelAuthor = level.Author;
            LevelDescription = level.Description;
            LevelDisplayName = level.SceneName;
            LevelID = level.Identifier;
            LevelImage = level.Sprite;
            Original = level;
            LevelSceneName = "TakeAndHoldClassic";
#endif
        }
        
        public LevelInfo Original;
    }
}