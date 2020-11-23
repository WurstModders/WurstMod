﻿using System.IO;
using Deli;
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
        
        public bool IsFrameworkMod;
        public Mod Mod;

        // This is a replacement for using the location of the level asset bundle as a unique identifier.
        public string Identifier => $"{SceneName}{Author}{Gamemode}{Description}".GetHashCode().ToString();
        
        public string AssetBundlePath => Path.Combine(Location, Constants.FilenameLevelData);
        public string ThumbnailPath => Path.Combine(Location, Constants.FilenameLevelThumbnail);
        public string LevelInfoPath => Path.Combine(Location, Constants.FilenameLevelInfo);

        public Texture2D Thumbnail
        {
            get
            {
                if (!IsFrameworkMod) return SpriteLoader.LoadTexture(ThumbnailPath);
                var thumb = Mod.Resources.Get<Texture2D>(ThumbnailPath);
                return thumb.IsNone ? null : thumb.Unwrap();
            }
        }

        private AssetBundle _cached;
        public AssetBundle AssetBundle
        {
            get
            {
                if (!_cached)
                    _cached = IsFrameworkMod ? Mod.Resources.Get<AssetBundle>(AssetBundlePath).Unwrap() : AssetBundle.LoadFromFile(AssetBundlePath);
                return _cached;
            }
        }


        /// <summary>
        /// Loads a LevelInfo struct from a file on disk
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <returns></returns>
        public static LevelInfo? FromFile(string path)
        {
            // If we were not given a path, assume the base game will handle it and return null;
            if (path == "")
                return null;

            // If we were not given the path to a directory, go up a level and re-create the path with the level info file
            if ((File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory)
                path = Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, Constants.FilenameLevelInfo);

            // Then return the deserialized object
            var levelInfo = JsonConvert.DeserializeObject<LevelInfo>(File.ReadAllText(path));
            levelInfo.Location = Path.GetDirectoryName(path);
            levelInfo.IsFrameworkMod = false;
            return levelInfo;
        }

        /// <summary>
        /// Creates a LevelInfo struct with the information given from a mod archive module
        /// </summary>
        /// <param name="mod">The mod the module originates from</param>
        /// <param name="module">The module</param>
        /// <returns>A LevelInfo from it</returns>
        public static LevelInfo? FromFrameworkMod(Mod mod, string path)
        {
            // Load the level info from the mod archive
            var levelInfo = JsonConvert.DeserializeObject<LevelInfo>(mod.Resources.Get<string>(path + Constants.FilenameLevelInfo).Unwrap());

            // If it doesn't exist, exit early
            if (levelInfo.Gamemode == "") return null;

            // Set some vars and return it
            levelInfo.Location = path;
            levelInfo.IsFrameworkMod = true;
            levelInfo.Mod = mod;
            return levelInfo;
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