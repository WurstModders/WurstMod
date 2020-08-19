using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WurstMod.Runtime
{
    public abstract class ScenePatcher
    {
        public abstract void PatchScene(Scene scene);

        /// <summary>
        /// Enumerates a list of scene patchers for the given scene name
        /// </summary>
        public static void RunPatches(Scene scene)
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(ScenePatcher)));

            Debug.Log("Found " + types.Count() + " patchers");

            foreach (var patcher in
                from type in types
                let attributes = type.GetCustomAttributes(typeof(ScenePatcherAttribute), false)
                where attributes.Length != 0
                where ((ScenePatcherAttribute) attributes[0]).SceneName == scene.name
                select (ScenePatcher) Activator.CreateInstance(type)) patcher.PatchScene(scene);
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