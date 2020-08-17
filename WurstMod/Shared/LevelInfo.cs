using System.IO;
using System.Linq;

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

        public static LevelInfo FromFile(string path)
        {
            var lines = File.ReadAllText(path).Split('\n');
            return new LevelInfo
            {
                SceneName = lines[0],
                Author = lines[1],
                Gamemode = lines[2],
                Description = string.Join("\n", lines.Skip(3).ToArray()),
                Location = Path.GetDirectoryName(path)
            };
        }

        public void ToFile(string path)
        {
            var contents = $"{SceneName}\n{Author}\n{Gamemode}\n{Description}";
            File.WriteAllText(path, contents);
        }
    }
}