using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace AlanZucconi.Data
{
    // https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
    [CustomPropertyDrawer(typeof(LinePlotAttribute))]
    public class LinePlotDrawer : PropertyDrawer
    {
        LinePlot linePlot = null;

        /*
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            EditorGUI.PropertyField(position, property);

            if (!property.isExpanded)
                return;

            position.y += PropertyHeight;

            LinePlotAttribute plotAttribute = attribute as LinePlotAttribute;
            PlotData data = fieldInfo.GetValue(property.serializedObject.targetObject) as PlotData;

            if (linePlot == null)
                linePlot = new LinePlot(data, plotAttribute);

            
            linePlot.OnGUI(position);
            //scatterPlot.OnInspectorGUI(position);
        }
        */
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Height of the foldout line
            float foldoutHeight = EditorGUIUtility.singleLineHeight;

            // Draw foldout label at the top
            Rect foldoutRect = new Rect(position.x, position.y, position.width, foldoutHeight);
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);

            if (!property.isExpanded)
                return;

            // Get attribute and data
            LinePlotAttribute plotAttribute = attribute as LinePlotAttribute;
            PlotData data = fieldInfo.GetValue(property.serializedObject.targetObject) as PlotData;

            if (linePlot == null)
                linePlot = new LinePlot(data, plotAttribute);                    


            // Rectangle for the actual scatter plot
            Rect plotRect = new Rect(
                position.x,
                position.y + foldoutHeight,
                position.width,
                plotAttribute.Height - foldoutHeight
            );

            linePlot.OnGUI(plotRect);
        }

        //const float PropertyHeight = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //ScatterPlotAttribute scatterPlot = attribute as ScatterPlotAttribute;
            //return property.isExpanded ? scatterPlot.Height : PropertyHeight;
            //return property.isExpanded ? 16 : PropertyHeight;

            //LinePlotAttribute plotAttribute = attribute as LinePlotAttribute;
            //return PropertyHeight + plotAttribute.Height;

            float propertyHeight = EditorGUIUtility.singleLineHeight;

            LinePlotAttribute plotAttribute = attribute as LinePlotAttribute;

            return property.isExpanded
                ? plotAttribute.Height
                : propertyHeight
                ;
        }
        
    }
}