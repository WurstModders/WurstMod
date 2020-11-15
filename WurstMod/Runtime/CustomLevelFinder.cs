using System.Collections.Generic;
using System.IO;
using System.Linq;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public static class CustomLevelFinder
    {
        public static readonly List<LevelInfo> DirectoryLevels = new List<LevelInfo>();
        public static readonly List<LevelInfo> ArchiveLevels = new List<LevelInfo>();
        
        /// <summary>
        /// Iterates over the custom levels found in the custom levels directory
        /// </summary>
        public static IEnumerable<LevelInfo> EnumerateLevelInfos()
        {
            foreach (var level in DirectoryLevels) yield return level;
            foreach (var level in ArchiveLevels) yield return level;
        }

        public static void DiscoverLevelsInFolder()
        {
            DirectoryLevels.Clear();
            if (!Directory.Exists(Constants.CustomLevelsDirectory)) return;
            var levels = Directory.GetFiles(Constants.CustomLevelsDirectory, Constants.FilenameLevelInfo, SearchOption.AllDirectories);
            DirectoryLevels.AddRange(levels.Select(LevelInfo.FromFile).Where(x => x.HasValue).Select(x => x.Value));
        }

        /// <summary>
        /// Returns an array of all the found level infos. Use this if you need to iterate the array multiple times
        /// </summary>
        public static LevelInfo[] GetLevelInfos() => EnumerateLevelInfos().ToArray();
    }
}