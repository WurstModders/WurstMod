using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class Extensions 
{
    public static Type[] GetTypesSafe(this Assembly asm)
    {
        Type[] retval;
        try
        {
            retval = asm.GetTypes();
        }
        catch (ReflectionTypeLoadException e)
        {
            return e.Types.Where(t => t != null).ToArray();
        }
        return retval;
    }


#if UNITY_EDITOR
    public static List<SerializedProperty> GetProperties(this Editor e)
    {
        List<SerializedProperty> result = new List<SerializedProperty>();
        SerializedProperty iter = e.serializedObject.GetIterator();
        if (iter.NextVisible(true))
        {
            do
            {
                result.Add(iter.Copy());
            }
            while (iter.NextVisible(false));
        }

        return result;
    }
#endif
}
