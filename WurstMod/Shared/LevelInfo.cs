using System.IO;
using ADepIn;
using UnityEngine;
using Valve.Newtonsoft.Json;
using WurstMod.Runtime;

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
        
        public object Mod; // Deli.Mod, made object to fix exporter within Unity.

        // This is a replacement for using the location of the level asset bundle as a unique identifier.
        public string Identifier => $"{SceneName}{Author}{Gamemode}{Description}".GetHashCode().ToString();
        
        public string AssetBundlePath => Path.Combine(Location, Constants.FilenameLevelData);
        public string ThumbnailPath => Path.Combine(Location, Constants.FilenameLevelThumbnail);
        public string LevelInfoPath => Path.Combine(Location, Constants.FilenameLevelInfo);

        public Sprite existingSprite; // Used by TNH loader.

        public Texture2D Thumbnail
        {
            get
            {
                var thumb = ((Deli.Mod)Mod).Resources.Get<Texture2D>(ThumbnailPath);
                return thumb.IsNone ? null : thumb.Unwrap();
            }
        }

        private AssetBundle _cached;
        public AssetBundle AssetBundle
        {
            get
            {
                if (!_cached)
                    _cached = ((Deli.Mod) Mod).Resources.Get<AssetBundle>(AssetBundlePath).Unwrap();
                return _cached;
            }
        }

        /// <summary>
        /// Creates a LevelInfo struct with the information given from a mod archive module
        /// </summary>
        /// <param name="mod">The Deli.Mod the module originates from</param>
        /// <param name="module">The module</param>
        /// <returns>A LevelInfo from it</returns>
        public static LevelInfo? FromFrameworkMod(object mod, string path)
        {
            var resources = ((Deli.Mod)mod).Resources;
            // Check if this level uses a json manifest (WM >= 2)
            if (resources.Get<string>(path + Constants.FilenameLevelInfo).MatchSome(out var manifest))
            {
                // Load the level info from the mod archive
                var levelInfo = JsonConvert.DeserializeObject<LevelInfo>(manifest);

                // Set some vars and return it
                levelInfo.Location = path;
                levelInfo.Mod = mod;
                return levelInfo;
            }
            // Check if this level uses an info txt file (WM < 2)
            else if (resources.Get<string>(path + "info.txt").MatchSome(out var info))
            {
                // This supports the older format
                var lines = info.Split('\n');
                return new LevelInfo
                {
                    SceneName = lines[0],
                    Author = lines[1],
                    Gamemode = path.Contains("TakeAndHold") ? Constants.GamemodeTakeAndHold : Constants.GamemodeSandbox,
                    Location = path,
                    Mod = mod
                };
            }

            // If somehow neither of the above two options match it's an invalid thing.
            return null;
        }

        /// <summary>
        /// Creates a Levelinfo struct with the params given. This is used by the TNH loader.
        /// </summary>
        /// <param name="name">Name of the level</param>
        /// <param name="author">Author of the level</param>
        /// <param name="gamemode">Gamemode of the level</param>
        /// <param name="desc">Description of the level</param>
        /// <param name="sprite">Sprite of the level</param>
        /// <returns></returns>
        public static LevelInfo FromParams(string name, string author, string gamemode, string desc, Sprite sprite)
        {
            LevelInfo info = new LevelInfo();
            info.SceneName = name;
            info.Author = author;
            info.Gamemode = gamemode;
            info.Description = desc;
            info.existingSprite = sprite;

            return info;
        }

        /// <summary>
        /// This should really only be used inside the Unity Editor
        /// </summary>
        public void ToFile()
        {
            // Make sure the directory exists
            if (!Directory.Exists(Location)) Directory.CreateDirectory(Location);

            // Then write the serialized data
            File.WriteAllText(LevelInfoPath, JsonConvert.SerializeObject(this));
        }
    }
}