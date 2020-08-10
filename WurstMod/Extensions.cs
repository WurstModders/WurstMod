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
        // Reflection shortcuts with caching. A smidge less verbose than InvokeMember.
        // Currently doesn't work for overloaded methods.
        // Cross that bridge when we come to it, I guess.
        private struct ReflectDef 
        { 
            public Type type;
            public string name;
            public ReflectDef(Type type, string name) 
            { 
                this.type = type;
                this.name = name;
            }
        }
        private static Dictionary<ReflectDef, MethodInfo> methodCache = new Dictionary<ReflectDef, MethodInfo>();
        private static Dictionary<ReflectDef, FieldInfo> fieldCache = new Dictionary<ReflectDef, FieldInfo>();

        public static object ReflectInvoke(this UnityEngine.Object target, string methodName, params object[] parameters)
        {
            MethodInfo method;
            ReflectDef def = new ReflectDef(target.GetType(), methodName);
            if (methodCache.ContainsKey(def))
            {
                method = methodCache[def];
            }
            else
            {
                method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
                methodCache[def] = method;
            }

            return method.Invoke(target, parameters);
        }

        public static T ReflectGet<T>(this UnityEngine.Object target, string fieldName)
        {
            FieldInfo field;
            ReflectDef def = new ReflectDef(target.GetType(), fieldName);
            if (fieldCache.ContainsKey(def))
            {
                field = fieldCache[def];
            }
            else
            {
                field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                fieldCache[def] = field;
            }

            return (T)field.GetValue(target);
        }

        public static void ReflectSet(this UnityEngine.Object target, string fieldName, object value) 
        {
            FieldInfo field;
            ReflectDef def = new ReflectDef(target.GetType(), fieldName);
            if (fieldCache.ContainsKey(def))
            {
                field = fieldCache[def];
            }
            else
            {
                field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
                fieldCache[def] = field;
            }

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

        public static void RefreshShader(this Material mat)
        {
            if (mat != null)
            {
                Shader refreshed = Shader.Find(mat.shader.name);
                if (refreshed != null)
                {
                    mat.shader = Shader.Find(mat.shader.name);
                    return;
                }
                Debug.LogError("WARNING: Failed to refresh shader \"" + mat.shader.name + "\", objects with this shader may render incorrectly.");
            }
            else
            {
                mat = new Material(Shader.Find("Standard"));
                Debug.LogError("WARNING: Attempted to refresh missing material, affected objects may render incorrectly.");
            }
            
        }

        public static T GetComponentBidirectional<T>(this Component mb) where T : Component
        {
            // Easy, builtin for checking self and children.
            T found = mb.GetComponentInChildren<T>();
            if (found != null) return found;

            // Annoying and hacky, checking parents.
            Transform parent = mb.transform.parent;
            while (parent != null)
            {
                found = parent.GetComponent<T>();
                if (found != null) return found;
                parent = parent.parent;
            }
            return null;
        }
        #endregion

        #region Gizmos
        public static void GenericGizmoCube(Color color, Vector3 center, Vector3 size, Vector3 forward, params Transform[] markers)
        {
            Gizmos.color = color;
            foreach (Transform ii in markers)
            {
                Gizmos.matrix = ii.localToWorldMatrix;
                Gizmos.DrawCube(center, size);
                Gizmos.DrawLine(center, center + forward);
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
