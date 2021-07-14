﻿using System.IO;
using UnityEngine;
using Valve.Newtonsoft.Json;
#if !UNITY_EDITOR
using Deli;
using Deli.VFS;
using FistVR;
using Valve.VR.InteractionSystem.Sample;
#endif

namespace WurstMod.Shared
{
    [JsonObject(MemberSerialization.OptIn)]
    public struct LevelInfo
    {
        [JsonProperty] public string SceneName;
        [JsonProperty] public string Author;
        [JsonProperty] public string Gamemode;
        [JsonProperty] public string Description;
        
        // This is a replacement for using the location of the level asset bundle as a unique identifier.
        public string Identifier => $"{SceneName}{Author}{Gamemode}{Description}".GetHashCode().ToString();

#if !UNITY_EDITOR
        // We don't want this serialized
        public IDirectoryHandle Location;

        public Mod Mod;

        public IFileHandle AssetBundlePath => Location.GetFile(Constants.FilenameLevelData);
        public IFileHandle ThumbnailPath => Location.GetFile(Constants.FilenameLevelThumbnail);
        public IFileHandle LevelInfoPath => Location.GetFile(Constants.FilenameLevelInfo);

        public Texture2D Thumbnail
        {
            get
            {
                try
                {
                    var stream = ThumbnailPath.OpenRead();
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    var tex = new Texture2D(0, 0);
                    tex.LoadImage(buffer);
                    return tex;
                }
                catch
                {
                    return null;
                }
            }
        }
        
        public Sprite Sprite
        {
            get
            {
                Texture2D tex = Thumbnail;
                return tex != null ? Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.one / 2) : null;
            }
        }

        private AssetBundle _cached;

        public AssetBundle AssetBundle
        {
            get
            {
                if (!_cached)
                {
                    var stream = AssetBundlePath.OpenRead();
                    var buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    _cached = AssetBundle.LoadFromMemory(buffer);
                }

                return _cached;
            }
        }

        /// <summary>
        /// Creates a LevelInfo struct with the information given from a mod archive module
        /// </summary>
        /// <param name="mod">The Deli.Mod the module originates from</param>
        /// <param name="module">The module</param>
        /// <returns>A LevelInfo from it</returns>
        public static LevelInfo? FromFrameworkMod(object source, object handle)
        {
            var mod = (Deli.Mod) source;
            var path = (IDirectoryHandle) handle;

            // Check if this level uses a json manifest (WM >= 2)
            var manifest = path.GetFile(Constants.FilenameLevelInfo);
            if (manifest != null)
            {
                // Load the level info from the mod archive
                var bytes = new StreamReader(manifest.OpenRead()).ReadToEnd();
                var levelInfo = JsonConvert.DeserializeObject<LevelInfo>(bytes);

                // Set some vars and return it
                levelInfo.Location = path;
                levelInfo.Mod = mod;
                return levelInfo;
            }

            // Check if this level uses an info txt file (WM < 2)
            manifest = path.GetFile("info.txt");
            if (manifest != null)
            {
                // This supports the older format
                var lines = new StreamReader(manifest.OpenRead()).ReadToEnd().Split('\n');
                return new LevelInfo
                {
                    SceneName = lines[0].Trim(),
                    Author = lines[1].Trim(),
                    Gamemode = path.Path.Contains("TakeAndHold") ? Constants.GamemodeTakeAndHold : Constants.GamemodeSandbox,
                    Location = path,
                    Mod = mod
                };
            }

            // If somehow neither of the above two options match it's an invalid thing.
            if (mod.Info.Guid != "wurstmodders.wurstmod.legacy")
                mod.Logger.LogError($"Level at {path} does not contain a valid manifest!");
            return null;
        }
#endif
        /// <summary>
        /// This should really only be used inside the Unity Editor
        /// </summary>
        public void ToFile(string location)
        {
            // Make sure the directory exists
            if (!Directory.Exists(location)) Directory.CreateDirectory(location);

            // Then write the serialized data
            File.WriteAllText(Path.Combine(location, Constants.FilenameLevelInfo), JsonConvert.SerializeObject(this));
        }

    }
}