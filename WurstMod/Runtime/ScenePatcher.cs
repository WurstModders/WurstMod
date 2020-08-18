using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace WurstMod.Runtime
{
    public abstract class ScenePatcher
    {
        public abstract void PatchScene();

        /// <summary>
        /// Enumerates a list of scene patchers for the given scene name
        /// </summary>
        public static void PatchScene(string sceneName)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(ScenePatcher)));

            Debug.Log("Found " + types.Count() + " patchers");
            
            foreach (var patcher in
                from type in types
                let attributes = type.GetCustomAttributes(typeof(ScenePatcherAttribute), false)
                where attributes.Length != 0
                where ((ScenePatcherAttribute) attributes[0]).SceneName == sceneName
                select (ScenePatcher) Activator.CreateInstance(type)) patcher.PatchScene();
        }
    }

    /// <summary>
    /// Marks a ScenePatcher class for use with a specific scene
    /// </summary>
    public class ScenePatcherAttribute : Attribute
    {
        public string SceneName { get; }

        public ScenePatcherAttribute(string sceneName) => SceneName = sceneName;
    }
}