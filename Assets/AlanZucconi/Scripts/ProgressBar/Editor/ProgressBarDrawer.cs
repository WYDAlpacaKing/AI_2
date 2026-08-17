using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ProgressBar))]
public class ProgressBarDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var valueProp = property.FindPropertyRelative("Value");
        var labelProp = property.FindPropertyRelative("Label");

        float value = Mathf.Clamp01(valueProp.floatValue);
        string barLabel = string.IsNullOrEmpty(labelProp.stringValue) ? property.displayName : labelProp.stringValue;

        EditorGUI.ProgressBar(position, value, barLabel);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}