using System.Collections.Generic;
using System.IO;
using System.Linq;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    public static class CustomLevelFinder
    {
        /// <summary>
        /// Iterates over the custom levels found in the custom levels directory
        /// </summary>
        public static IEnumerable<LevelInfo> EnumerateLevelInfos()
        {
            var levels = Directory.GetFiles(Constants.CustomLevelsDirectory, Constants.FilenameLevelInfo, SearchOption.AllDirectories);
            return levels.Select(LevelInfo.FromFile).Where(x => x.HasValue).Select(x => x.Value);
        }

        /// <summary>
        /// Returns an array of all the found level infos. Use this if you need to iterate the array multiple times
        /// </summary>
        public static LevelInfo[] GetLevelInfos() => EnumerateLevelInfos().ToArray();
    }
}