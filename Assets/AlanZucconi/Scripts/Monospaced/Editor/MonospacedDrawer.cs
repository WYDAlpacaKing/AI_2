using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(MonospacedAttribute))]
public class MonospacedDrawer : PropertyDrawer
{
    private GUIStyle _monoStyle;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        MonospacedAttribute monoAttr = (MonospacedAttribute)attribute;

        int lines = Mathf.Clamp(monoAttr.minLines, 1, monoAttr.maxLines);
        return EditorGUIUtility.singleLineHeight * lines + 10;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.String)
        {
            EditorGUI.LabelField(position, label.text, "Use [Monospaced] with strings.");
            return;
        }

        if (_monoStyle == null)
        {
            _monoStyle = new GUIStyle(EditorStyles.textArea)
            {
                wordWrap = true,
                font = Font.CreateDynamicFontFromOSFont("Courier New", 12),
                fontSize = 12,
                richText = false
            };
        }

        EditorGUI.BeginProperty(position, label, property);

        Rect labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, label);

        Rect textRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, position.height - EditorGUIUtility.singleLineHeight - 2);
        property.stringValue = EditorGUI.TextArea(textRect, property.stringValue, _monoStyle);

        EditorGUI.EndProperty();
    }
}