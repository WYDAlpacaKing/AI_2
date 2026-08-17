using System.Collections.Generic;

using UnityEngine;
using UnityEditor;

namespace AlanZucconi.Data
{
    // https://docs.unity3d.com/ScriptReference/PropertyDrawer.html
    [CustomPropertyDrawer(typeof(HistogramPlotAttribute))]
    public class HistogramPlotDrawer : PropertyDrawer
    {
        HistogramPlot histogramPlot = null;

        /*
        // Draw the property inside the given rect
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label);
            EditorGUI.PropertyField(position, property);

            if (!property.isExpanded)
                return;

            position.y += PropertyHeight;

            HistogramPlotAttribute plotAttribute = attribute as HistogramPlotAttribute;
            PlotData data = fieldInfo.GetValue(property.serializedObject.targetObject) as PlotData;

            if (histogramPlot == null)
                histogramPlot = new HistogramPlot(data, plotAttribute);

            
            histogramPlot.OnGUI(position);
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
            HistogramPlotAttribute plotAttribute = attribute as HistogramPlotAttribute;
            PlotData data = fieldInfo.GetValue(property.serializedObject.targetObject) as PlotData;

            if (histogramPlot == null)
                histogramPlot = new HistogramPlot(data, plotAttribute);

            // Rectangle for the actual scatter plot
            Rect plotRect = new Rect(
                position.x,
                position.y + foldoutHeight,
                position.width,
                plotAttribute.Height - foldoutHeight
            );

            histogramPlot.OnGUI(plotRect);
        }

        //const float PropertyHeight = 16;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            //ScatterPlotAttribute scatterPlot = attribute as ScatterPlotAttribute;
            //return property.isExpanded ? scatterPlot.Height : PropertyHeight;
            //return property.isExpanded ? 16 : PropertyHeight;

            float propertyHeight = EditorGUIUtility.singleLineHeight;

            HistogramPlotAttribute plotAttribute = attribute as HistogramPlotAttribute;

            return property.isExpanded
                ? plotAttribute.Height
                : propertyHeight
                ;
        }
        
    }
}