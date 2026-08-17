using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Data
{
    public class ScatterPlotAttribute : PropertyAttribute
    {
        public float Height = 250;

        public float GridX = 100;
        public float GridY = 10;

        //public Vector2 Grid = new Vector2(100, 10);
        public Vector2 Grid
        {
            get => new Vector2(GridX, GridY);
        }

        public bool KeepAspectRatio = false;

        public Color DataColour = new Color(1f, 1f, 1f);
        public Color MedianColour = new Color(1f, 1f, 0f);
        public Color BackgroundColor = new Color(0.254902f, 0.254902f, 0.254902f);
        public Color GridColor = new Color(1, 1, 1);

        public string LabelX = null;
        public string LabelY = null;
    }
}