using System;
using System.Linq;
using System.Reflection;
using FistVR;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.MappingComponents.Generic;
using WurstMod.Shared;
using Object = UnityEngine.Object;

// ReSharper disable UnassignedField.Global

namespace WurstMod.Runtime
{
    public static class ObjectReferences
    {
        #region Auto-set fields

        [ObjectReference] public static FVRPointableButton ButtonDonor;
        [ObjectReference] public static TNH_DestructibleBarrierPoint BarrierDonor;
        [ObjectReference] public static TNH_Manager ManagerDonor;
        [ObjectReference] public static TNH_HoldPoint HoldPointDonor;
        [ObjectReference] public static SosigTestingPanel1 GroundPanel;
        [ObjectReference] public static FVRReverbSystem ReverbSystem;
        [ObjectReference] public static FVRSceneSettings FVRSceneSettings;
        [ObjectReference] public static MainMenuScreen MainMenuControls;
        [ObjectReference] public static AIManager AIManager;

        // These get marked as Don't Destroy On Load because we kind of need them to exist after a reload :/
        [ObjectReference("ItemSpawner", true)] public static GameObject ItemSpawnerDonor;
        [ObjectReference("Destructobin", true)] public static GameObject DestructobinDonor;
        [ObjectReference("SosigSpawner", true)] public static GameObject SosigSpawnerDonor;
        [ObjectReference("WhizzBangADinger2", true)] public static GameObject WhizzBangADingerDonor;
        [ObjectReference("BangerDetonator", true)] public static GameObject BangerDetonatorDonor;

        [ObjectReference("[CameraRig]Fixed")] public static GameObject CameraRig;
        [ObjectReference("[ResetPoint]")] public static GameObject ResetPoint;

        #endregion

        #region Manually-set fields

        /// <summary>
        /// This is set just after the custom scene is loading. It will always be set before custom code is ran.
        /// </summary>
        public static CustomScene CustomScene;

        #endregion


        /// <summary>
        ///     Tries to find the appropriate object reference for the attached field.
        ///     This should be called on scene load <b>before</b> anything messes with
        ///     it to ensure the objects aren't destroyed.
        /// </summary>
        public static void FindReferences(Scene scene)
        {
            // A list of every GameObject in the scene
            var gameObjects = scene.GetAllGameObjectsInScene();

            // First we find every field that uses this attribute
            foreach (var field in AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).SelectMany(x => x.GetFields()))
            {
                // Find our attributes on that field and continue if there aren't any
                var attributes = field.GetCustomAttributes(typeof(ObjectReferenceAttribute), true).Cast<ObjectReferenceAttribute>().ToArray();
                if (attributes.Length == 0) continue;

                // Reset the field and go over each found attribute
                //field.SetValue(null, null);
                foreach (var reference in attributes)
                {
                    // If the field's value is already set, break.
                    // NOTE: Objects are "deleted" on scene load, but reflection will not necessarily return null immediately.
                    // This method is only run on scene load anyway, so the duplicate checking isn't super necessary.
                    if (field.GetValue(null) as Object) break;

                    // If the field type is GameObject, just find the GameObject normally
                    Object found;
                    if (field.FieldType == typeof(GameObject))
                        found = gameObjects.FirstOrDefault(x => x.name.Contains(reference.NameFilter));
                    // If it isn't, we also want to query for where it has a component of the right type
                    else
                        found = gameObjects.Where(x => x.name.Contains(reference.NameFilter)).FirstOrDefault(x => x.GetComponent(field.FieldType))?.GetComponent(field.FieldType);
                    
                    if (found == null) continue;
                    field.SetValue(null, found);
                    
                    if (!reference.DontDestroyOnLoad) continue;
                    if (found is GameObject go)
                    {
                        go.transform.parent = null;
                        go.transform.position = Vector3.down * 1000;
                    }
                    else
                    {
                        ((Component) found).transform.parent = null;
                        ((Component) found).transform.position = Vector3.down * 1000;
                    }
                    Object.DontDestroyOnLoad(found);
                }
            }
        }
    }

    /// <summary>Attribute to represent a field that is auto-filled with an object reference when a scene is loaded.</summary>
    /// <example>
    ///     <code>
    /// // Provides a reference to the scene's Camera Rig.
    /// [ObjectReference("[CameraRig]Fixed")]
    /// public static GameObject CameraRig;
    /// </code>
    /// </example>
    [AttributeUsage(AttributeTargets.Field)]
    public class ObjectReferenceAttribute : Attribute
    {
        public ObjectReferenceAttribute(string nameFilter = "", bool dontDestroyOnLoad = false)
        {
            NameFilter = nameFilter;
            DontDestroyOnLoad = dontDestroyOnLoad;
        }

        public string NameFilter { get; }
        public bool DontDestroyOnLoad { get; }
    }
}