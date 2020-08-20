using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace WurstMod.Shared
{
    public struct LevelInfo
    {
        public string SceneName;
        public string Author;
        public string Gamemode;
        public string Description;

        public string Location;

        public string AssetBundlePath => Path.Combine(Location, Constants.FilenameLevelData);
        public string ThumbnailPath => Path.Combine(Location, Constants.FilenameLevelThumbnail);
        public string LevelInfoPath => Path.Combine(Location, Constants.FilenameLevelInfo);


        private static readonly List<string> InvalidLevels = new List<string>();

        public static LevelInfo? FromFile(string path)
        {
            // If we were not given the path to a directory, go up a level and re-create the path with the level info file
            if ((File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory)
                path = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Constants.FilenameLevelInfo);
            
            var lines = File.ReadAllText(path).Replace("\r", "").Split('\n');

            if (lines.Length >= 4)
                return new LevelInfo
                {
                    SceneName = lines[0],
                    Author = lines[1],
                    Gamemode = lines[2],
                    Description = string.Join("\n", lines.Skip(3).ToArray()),
                    Location = Path.GetDirectoryName(path)
                };
            Debug.LogError("Invalid level info file at " + path + "!");
            return null;

        }

        public void ToFile(string path)
        {
            var contents = $"{SceneName}\n{Author}\n{Gamemode}\n{Description}";
            File.WriteAllText(path, contents);
        }
    }
}