using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WurstMod
{
    public static class Extensions
    {
        #region LINQ
        public static IEnumerable<T> DistinctBy<T, TKey>(this IEnumerable<T> items, Func<T, TKey> property)
        {
            return items.GroupBy(property).Select(x => x.First());
        }

        public static IEnumerable<Transform> AsEnumerable(this Transform transform)
        {
            if (transform != null)
            {
                return transform.Cast<Transform>();
            }
            else
            {
                return new List<Transform>();
            }
        }
        #endregion

        #region Reflection
        // Reflection shortcuts.

        public static object ReflectInvoke<T>(this T target, string methodName, params object[] parameters) where T : UnityEngine.Object
        {
            MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            return method.Invoke(target, parameters);
        }

        public static object ReflectGet<T>(this T target, string fieldName) where T : UnityEngine.Object
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            return field.GetValue(target);
        }

        public static void ReflectSet<T>(this T target, string fieldName, object value) where T : UnityEngine.Object
        {
            FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field.SetValue(target, value);
        }

        #endregion

        #region Scene
        public static List<GameObject> GetAllGameObjectsInScene(this Scene scene)
        {
            List<GameObject> allGameObjects = new List<GameObject>();
            List<GameObject> rootGameObjects = scene.GetRootGameObjects().ToList();
            foreach (GameObject ii in rootGameObjects)
            {
                foreach (GameObject jj in ii.GetComponentsInChildren<Transform>(true).Select(x => x.gameObject))
                {
                    allGameObjects.Add(jj);
                }
            }
            return allGameObjects;
        }
        #endregion

        #region Gizmos
        public static void GenericGizmoCube(Color color, Vector3 center, Vector3 size, bool drawFacing, params Transform[] markers)
        {
            Gizmos.color = color;
            foreach (Transform ii in markers)
            {
                Gizmos.matrix = ii.localToWorldMatrix;
                Gizmos.DrawCube(center, size);
                if (drawFacing) Gizmos.DrawLine(center, center + Vector3.forward);
            }
        }

        public static void GenericGizmoCubeOutline(Color color, Vector3 center, Vector3 size, params Transform[] markers)
        {
            Gizmos.color = color;
            foreach (Transform ii in markers)
            {
                Gizmos.matrix = ii.localToWorldMatrix;
                Gizmos.DrawWireCube(center, size);
            }
        }

        public static void GenericGizmoSphere(Color color, Vector3 center, float radius, params Transform[] markers)
        {
            Gizmos.color = color;
            foreach (Transform ii in markers)
            {
                Gizmos.matrix = ii.localToWorldMatrix;
                Gizmos.DrawSphere(center, radius);
            }
        }
        #endregion
    }
}
