using System;
using System.Reflection;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// Adapted from u/kalin_r
// https://gist.github.com/kalineh/ad5135946f2009c36f755eea0a880998

#if UNITY_EDITOR
using UnityEditor;

public class EnumPickerWindow : EditorWindow
{
    private static GUIStyle regularStyle;
    private static GUIStyle selectedStyle;

    private string enumName;
    private List<string> valuesRaw;
    private List<string> valuesFiltered;
    private string filter;

    private EditorWindow parent;
    private Vector2 scroll;

    private System.Action<string> onSelectCallback;

    public void ShowCustom(string name, List<string> values, Rect rect, System.Action<string> onSelect)
    {
        regularStyle = new GUIStyle(EditorStyles.label);
        regularStyle.active = regularStyle.normal;

        selectedStyle = new GUIStyle(EditorStyles.label);
        selectedStyle.normal = selectedStyle.focused;
        selectedStyle.active = selectedStyle.focused;

        enumName = name;
        valuesRaw = new List<string>(values);
        valuesFiltered = new List<string>(values);
        filter = "";
        onSelectCallback = onSelect;

        parent = focusedWindow;

        var screenRect = rect;
        var screenSize = new Vector2(400, 400);

        screenRect.position = GUIUtility.GUIToScreenPoint(screenRect.position);

        ShowAsDropDown(screenRect, screenSize);
        Focus();

        GUI.FocusControl("filter");
    }

    private void OnGUI()
    {
        GUILayout.Label(string.Format("Enum Type: {0}", enumName));

        GUI.SetNextControlName("filter");
        var filterUpdate = GUILayout.TextField(filter);
        if (filterUpdate != filter)
            FilterValues(filterUpdate);

        // always focused
        GUI.FocusControl("filter");

        scroll = GUILayout.BeginScrollView(scroll);

        for (int i = 0; i < valuesFiltered.Count; ++i)
        {
            var value = valuesFiltered[i];
            var style = i == 0 ? selectedStyle : regularStyle;
            var rect = GUILayoutUtility.GetRect(new GUIContent(value), style);

            var clicked = GUI.Button(rect, value);
            if (clicked)
            {
                GUILayout.EndScrollView();

                onSelectCallback(value);
                Close();
                parent.Repaint();
                parent.Focus();

                return;
            }
        }

        GUILayout.EndScrollView();

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return)
        {
            if (valuesFiltered.Count > 0)
                onSelectCallback(valuesFiltered[0]);

            Close();
            parent.Repaint();
            parent.Focus();
        }

        if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
        {
            Close();
            parent.Repaint();
            parent.Focus();
        }
    }

    public void OnLostFocus()
    {
        Close();
    }

    private void FilterValues(string filterUpdate)
    {
        filter = filterUpdate;

        var filterLower = filter.ToLower();

        valuesFiltered.Clear();

        for (int i = 0; i < valuesRaw.Count; ++i)
        {
            var value = valuesRaw[i];
            var lower = value.ToLower();
            if (lower.Contains(filterLower))
                valuesFiltered.Add(value);
        }
    }
}

[CustomPropertyDrawer(typeof(WurstMod.Shared.ResourceDefs.AnvilAsset))]
public class EnumDrawers : EnumPicker
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        base.OnGUI(position, property, label);
    }
}

public class EnumPicker : PropertyDrawer
{
    private EnumPickerWindow window;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valuesRaw = Enum.GetValues(fieldInfo.FieldType);
        if (valuesRaw.Length <= 0)
            return;

        var valuesStr = new List<string>();
        for (int i = 0; i < valuesRaw.Length; ++i)
        {
            var raw = valuesRaw.GetValue(i);
            var str = raw.ToString();

            valuesStr.Add(str);
        }

        var enumName = fieldInfo.FieldType.Name;
        var currentName = Enum.GetName(fieldInfo.FieldType, property.intValue);

        EditorGUI.PrefixLabel(position, label);

        GUI.SetNextControlName(property.propertyPath);

        var fieldRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, position.height);

        if (GUI.Button(fieldRect, currentName, EditorStyles.popup))
        {
            window = EditorWindow.GetWindow<EnumPickerWindow>();

            System.Action<string> callback = str =>
            {
                var index = (int)Convert.ChangeType(Enum.Parse(fieldInfo.FieldType, str), fieldInfo.FieldType);

                property.serializedObject.Update();
                property.intValue = index;
                property.serializedObject.ApplyModifiedProperties();
            };

            window.ShowCustom(enumName, valuesStr, fieldRect, callback);
            window.Focus();
        }
    }
}
#endif
