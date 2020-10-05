using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.Newtonsoft.Json;

namespace WurstMod.Shared
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct LevelInfo
    {
        [JsonProperty] public string SceneName;
        [JsonProperty] public string Author;
        [JsonProperty] public string Gamemode;
        [JsonProperty] public string Description;

        // We don't want this serialized
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
            
            // Then return the deserialized object
            var levelInfo = JsonConvert.DeserializeObject<LevelInfo>(File.ReadAllText(path));
            levelInfo.Location = Path.GetDirectoryName(path);
            return levelInfo;
        }

        public void ToFile()
        {
            // Make sure the directory exists
            if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);
            
            // Then write the serialized data
            File.WriteAllText(LevelInfoPath, JsonConvert.SerializeObject(this));
        }
    }
}