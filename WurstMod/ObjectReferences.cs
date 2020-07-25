using System;
using System.Linq;
using System.Reflection;
using FistVR;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using WurstMod.TNH;
using Object = UnityEngine.Object;

namespace WurstMod
{
    public static class ObjectReferences
    {
        [ObjectReference]
        public static FistVR.FVRPointableButton ButtonDonor;
        [ObjectReference]
        public static FistVR.TNH_DestructibleBarrierPoint BarrierDonor;
        [ObjectReference]
        public static FistVR.TNH_Manager ManagerDonor;
        [ObjectReference]
        public static FistVR.TNH_HoldPoint HoldPointDonor;

        [ObjectReference("ItemSpawner")]
        public static GameObject ItemSpawnerDonor;
        [ObjectReference("Destructobin")]
        public static GameObject DestructobinDonor;
        [ObjectReference("SosigSpawner")]
        public static GameObject SosigSpawnerDonor;
        [ObjectReference("WhizzBangADinger2")]
        public static GameObject WhizzBangADingerDonor;
        [ObjectReference("BangerDetonator")]
        public static GameObject BangerDetonatorDonor;

        [ObjectReference] // TODO Slightly dangerous I think? Remember this and doublecheck it.
        public static TNH_Level Level;
        [ObjectReference("[CameraRig]Fixed")]
        public static GameObject CameraRig;
        [ObjectReference("_FinalScore")]
        public static GameObject FinalScore;
        [ObjectReference("[ResetPoint]")]
        public static GameObject ResetPoint;


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
            foreach (var field in Assembly.GetExecutingAssembly().GetTypes().SelectMany(x => x.GetFields()))
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
                    //if (field.GetValue(null) != null) break;

                    // If the field type is GameObject, just find the GameObject normally
                    Object found;
                    if (field.FieldType == typeof(GameObject))
                        found = gameObjects.FirstOrDefault(x => x.name.Contains(reference.NameFilter));
                    // If it isn't, we also want to query for where it has a component of the right type
                    else
                        found = gameObjects.Where(x => x.name.Contains(reference.NameFilter)).FirstOrDefault(x => x.GetComponent(field.FieldType))?.GetComponent(field.FieldType);
                    if (found != null) field.SetValue(null, found);
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
        public ObjectReferenceAttribute(string nameFilter = "")
        {
            NameFilter = nameFilter;
        }

        public string NameFilter { get; }
    }
}