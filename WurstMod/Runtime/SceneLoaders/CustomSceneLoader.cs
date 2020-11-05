using System;
using System.Linq;
using UnityEngine;
using WurstMod.MappingComponents.Generic;
using WurstMod.Shared;

namespace WurstMod.Runtime
{
    /// <summary>
    /// Abstract class to derive custom scene loaders for
    /// </summary>
    public abstract class CustomSceneLoader
    {
        /// <summary>
        /// This is implemented by the deriving class and tells the loader which scene to use as the base for the game mode
        /// </summary>
        public abstract string BaseScene { get; }
        
        public CustomScene LevelRoot { get; set; }

        /// <summary>
        /// This method is called by the loader class before it has done anything. The original scene will be intact
        /// and unmodified
        /// </summary>
        public virtual void PreLoad()
        {
        }

        /// <summary>
        /// This method is called by the loader class after it has finished loading the modded scene.
        /// All proxies will have been resolved and the original scene will have been stripped of any
        /// unused game objects.
        /// </summary>
        public virtual void PostLoad()
        {
        }

        /// <summary>
        /// Called when the loader is finished resolving components but just before everything is re-activated.
        /// </summary>
        public virtual void Resolve()
        {
        }

        public static CustomSceneLoader GetSceneLoaderForGamemode(string gamemode)
        {
            // Get a list of all types in the app domain that derive from CustomSceneLoader
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(CustomSceneLoader)));

            // Magic LINQ statement to select the first type that has the
            // CustomSceneLoaderAttribute with a gamemode that matches the gamemode parameter
            return
            (
                from type in types
                let attributes = type.GetCustomAttributes(typeof(CustomSceneLoaderAttribute), false)
                where attributes.Length != 0
                where ((CustomSceneLoaderAttribute) attributes[0]).Gamemode == gamemode
                select (CustomSceneLoader) Activator.CreateInstance(type)
            ).FirstOrDefault();
        }
    }

    /// <summary>
    /// An attribute for use on derived CustomSceneLoader classes. Describes which game mode they will run on. 
    /// </summary>
    public class CustomSceneLoaderAttribute : Attribute
    {
        public string Gamemode { get; }

        public CustomSceneLoaderAttribute(string gamemode) => Gamemode = gamemode;
    }
}