using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using System.Linq;

namespace AlanZucconi.Data
{
    public class HistogramPlot
    {
        // The data and properties used to draw it
        public HistogramPlotAttribute Attribute;
        public PlotData Data;

        // Current size (based on Inspector size)
        public float Width, Height;

        // Singleton
        static Material Material = null;



        

        public HistogramPlot(PlotData data, HistogramPlotAttribute attribute)
        {
            Data = data;
            Attribute = attribute;

            Initialise();
        }

        public void Initialise ()
        {
            Data.CalculateStatistics();
            CalculateHistogram();

            if (Material == null)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                Material = new Material(shader);
            }
        }

        public void Destroy ()
        {
            Object.DestroyImmediate(Material);
        }

        #region HistogramData
        public int[] HistogramData;
        public float MinX;
        public float MaxX;
        public float MinY;
        public float MaxY;
        public void CalculateHistogram()
        {

            // To few points to calculate histogram!
            if (Data.Data.Count < 5)
                return;


            /*
            // Outliers -------------
            // Removes top and bottom 5 percentile
            float outliersThreshold = 0.05f/2; // 5 percentile

            float outliersMin = Data.Data.Percentile(point => point.y, 0.0f + outliersThreshold);
            float outliersMax = Data.Data.Percentile(point => point.y, 1.0f - outliersThreshold);

            var points = Data.Data
                .Select(point => point.y)
                .Where(value => value >= outliersMin && value <= outliersMax);
            */
            var points = Data.Data
                .Select(point => point.y);

            MinX = points.Min();
            MaxX = points.Max();

            // All points are the same!
            if (MinX == MaxX)
                return;


            // https://www.statisticshowto.com/choose-bin-sizes-statistics/#freedman
            // https://stats.stackexchange.com/questions/798/calculating-optimal-number-of-bins-in-a-histogram

            // Uses Freedman-Diaconis rule for the number of bins

            //Freedman - Diaconis rule is very robust and works well in practice.
            // The bin-width is set to h = 2×IQR×n−1 / 3.
            // So the number of bins is (max−min)/ h,
            // where n is the number of observations,
            // max is the maximum value and min is the minimum value.
            //float IQR = Data.Quartile3.y - Data.Quartile1.y;
            //float h = 2 * IQR / Mathf.Pow(points.Count()-1, 1f / 3f);
            //int bins = Mathf.FloorToInt((MaxX- MinY)/h);

            // Sturge's Rule
            // Sturge’s rule works best for continuous data that is normally distributed and symmetrical. 
            // Not good for skewed data
            //int bins = Mathf.FloorToInt(1f + 3.322f * Mathf.Log(points.Count()));

            // Scott's Rule
            //3.49σn−1 / 3.
            //int bins = Mathf.FloorToInt((3.49f * points.StandardDeviation()) / Mathf.Pow(points.Count(), 1f / 3f));

            // Rice's Rule
            // (cube root of the number of observations) *2.
            int bins = Mathf.FloorToInt(2 * Mathf.Pow(points.Count(), 1f/3f));
            if (bins == 0)
                return;

            HistogramData = new int[bins];
            //int bins = Attribute.Bins;

            //Attribute.Bins = 15;
            //HistogramData = new int[Attribute.Bins];




            //MinX = Data.Data.Min(point => point.y);
            //MaxX = Data.Data.Max(point => point.y);

            // Loops through the points in the dataset
            //foreach (float value in Data.Data.Select(point => point.y))
            foreach (float value in points)
            {
                // Finds the bin
                // value: [MinX, MaxX]
                // bin:   [0,    bins-1]
                int bin = (int)((value - MinX) / (MaxX - MinX) * (bins - 1));
                if (bin >= HistogramData.Length)
                    continue;
                if (bin < 0)
                    continue;

                HistogramData[bin]++;
            }

            MinY = 0;
            MaxY = HistogramData.Max();

            //Debug.Log(MinX + "\t" + MaxX + "\t" + MinY + "\t" + MaxY);
        }
        #endregion

        // https://answers.unity.com/questions/1360515/how-do-i-draw-lines-in-a-custom-inspector.html
        // From Histogram Data to Rect
        // point:  [minX, maxX]
        // vertex: [0, Width]
        private float GetX(float x)
        {
            //return (x / Data.Max.x) * Width;
            return (x - MinX) / (MaxX - MinX) * Width;
        }
        private float GetY(float y)
        {
            //return Height - (y / Data.Max.y) * Height;
            return Height - (y / MaxY) * Height;
        }
        private Vector2 GetPoint(Vector2 point)
        {
            return new Vector2(GetX(point.x), GetY(point.y));
        }


        //public void OnInspectorGUI()
        public void OnGUI(Rect rect)
        {
            if (UnityEngine.Event.current.type != EventType.Repaint)
                return;

            if (Data == null || Data.Data.Count == 0)
                return;


            // To few points to calculate histogram!
            if (Data.Data.Count < 5)
                return;


            if (Data.Dirty)
            {
                Data.CalculateStatistics(); // Not re-calculated if data is unchanged
                CalculateHistogram();
            }
            //Data.CalculateStatistics(); // Not re-calculated if data is unchanged

            if (MinX == MaxX)
                return;

            //Rect rect = GUILayoutUtility.GetRect(10, 500, Attribute.Height, Attribute.Height);
            //Width = rect.width;
            //Height = rect.height;


            Width = rect.width;
            Height = rect.height;

            // Aspect ratio
            //Data.Max.y = Data.Max.x / (Attribute.Grid.x / Attribute.Grid.y);
            //Data.Max.x = Data.Max.y * (Attribute.Grid.x / Attribute.Grid.y);
            //GUI.Box(rect, "This is a box", "x");

            GUI.BeginClip(rect);
            GL.PushMatrix();

            GL.Clear(true, false, Color.black);
            Material.SetPass(0);

            // --- Background -----------------
            GL.Begin(GL.QUADS);

            GL.Color(Attribute.BackgroundColor);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(rect.width, 0, 0);
            GL.Vertex3(rect.width, rect.height, 0);
            GL.Vertex3(0, rect.height, 0);

            GL.End();
            // -----------------

            /*
            // --- Line -----------------
            GL.Begin(GL.LINES);
            GL.Color(Attribute.GridColor.xA(0.25f));

            for (float x = 0; x <= Data.Max.x; x += Attribute.GridX)
                VerticalLine(x);
            for (float y = 0; y <= Data.Max.y; y += Attribute.GridY)
                HorizontalLine(y);

            VerticalLine(Data.Max.x);
            HorizontalLine(Data.Max.y);

            GL.End();
            // -----------------
            */


            // --- Stats -----------------
            //GL.Begin(GL.LINES);
            // Median
            //GL.Color(Color.yellow.xA(0.5f));
            //GL.Color(Attribute.MedianColour.xA(0.5f));
            //VerticalLine(Data.Quartile2.x);
            //HorizontalLine(Data.Quartile2.y);

            // IQR
            //GL.Color(new Color(1, 1, 0, 0.5f));
            //float median = 
            //VerticalLine(quartile2X);


            /*
            // IQR
            GL.Color(new Color(1, 1, 0, 0.5f));
            verticalLine(quartile1X);
            verticalLine(quartile3X);

            GL.Color(new Color(1, 1, 0, 0.5f));
            horizontalLine(quartile1Y);
            horizontalLine(quartile3Y);
            */
            //GL.End();

            // --- IQR -----------------
            //GL.Begin(GL.QUADS);
            //GL.Color(Attribute.MedianColour.xA(0.05f));
            //GLRect(Data.Quartile1.x, 0, Data.Quartile3.x, Data.Max.y);
            //GLRect(0, Data.Quartile1.y, Data.Max.x, Data.Quartile3.y);
            //GL.End();
            // -----------------


            

            // --- Data (bins) -----------------
            //if (Data.Data.Count > 1)
            if (MaxY > 1)
            {
                GL.Begin(GL.QUADS);
                //GL.Color(Color.white);
                GL.Color(Attribute.DataColour.xA(0.5f));
                //foreach (Vector2 point in Data.Data)
                for (int i = 0; i < HistogramData.Length; i++)
                {
                    // i: [0, HistogramData.Length -1]
                    // x: [MinX, MaxX]
                    float x0 = (i / (float) (HistogramData.Length-0)) * (MaxX - MinX) + MinX;

                    
                    float y0 = 0;

                    // y0:               [0,    Height]
                    //float y0 = (HistogramData[i] - MinY) / (MaxY - MinY) * Height;

                    float x1 = ((i+1) / (float)(HistogramData.Length -0)) * (MaxX - MinX) + MinX;
                    // HistogramData[i]: [MinY, MaxY]
                    float y1 = HistogramData[i];


                    

                    GLRect(x0, y0, x1, y1);

                    //Debug.Log(MinX + "\t" + MaxX + "\t" + MinY + "\t" + MaxY);
                    //Debug.Log(i + "\t" + x0 + "\t" + y0 + "\t| " + x1 + "\t" + y1);

                    

                }
                GL.End();
            }

            GL.Begin(GL.LINES);
            // Median (includes outliers)
            GL.Color(new Color(1, 1, 0, 0.5f));
            float medianScore = Data.Data.Median(point => point.y);
            VerticalLine(medianScore);
            //GL.Color(new Color(1, 1, 0, 0.25f));
            //VerticalLine(Data.Data.Average(point => point.y));

            GL.End();




            // Median message
            //GUI.contentColor = Attribute.MedianColour;
            GUI.contentColor = new Color(1, 1, 0, 1);
            //EditorGUI.LabelField(new Rect(GetX(medianScore), Height - rect.y - 25, 100, 25), "" + medianScore, EditorStyles.boldLabel);
            GUI.Label(new Rect(GetX(medianScore), Height - rect.y - 25, 100, 25), "" + medianScore, EditorStyles.boldLabel);
            GUI.contentColor = new Color(1, 1, 1, 1);
            //EditorGUI.DrawRect(new Rect(GetX(medianScore), Height - rect.y - 25, 100, 25), Color.green.xA(0.25f));


            //GL.PushMatrix();


            // Histogram labels
            GUI.contentColor = new Color(0, 0, 0, 1);
            for (int i = 0; i < HistogramData.Length; i++)
            {
                //EditorGUI.LabelField
                GUI.Label
                (
                    // i: [0, HistogramData.Length -1]
                    // x: [0, Width]
                    new Rect
                    (
                        ((float)i / (HistogramData.Length - 0)) * Width,
                        //Height/2 - rect.y + 0
                        Height / 50 + 10
                        // alternating up and down
                        - (i%2 == 0 ? 10 : 0),
                        100, 50
                    ),
                    //new Rect(rect.x + 0, rect.y + 0, 100, 50),
                    //new Rect(rect.x + 0, Height - rect.y + 0, 100, 50),
                    //"" + HistogramData[i],
                    //"" + HistogramData[i],
                    ((i / (float)(HistogramData.Length - 0)) * (MaxX - MinX) + MinX).ToString("0.0"),
                    EditorStyles.miniLabel
                );
                //Debug.Log((i / (float)(HistogramData.Length - 0)) * (MaxX - MinX) + MinX);

                /*
                // TEST
                EditorGUI.DrawRect(new Rect
                    (
                        rect.x + ((float)i / (HistogramData.Length - 0)) * Width,
                        Height - rect.y + 0
                        // alternating up and down
                        - (i % 2 == 0 ? 10 : 0),
                        100, 50
                    ), Color.green.xA(0.25f));*/
            }
            GUI.contentColor = new Color(1, 1, 1, 1);
            //GL.PopMatrix();




            /*
            // --- Data (lines) -----------------
            if (Data.Data.Count > 1)
            {
                GL.Begin(GL.LINES);
                //GL.Color(Color.white);
                GL.Color(Attribute.DataColour.xA(0.5f));
                //foreach (Vector2 point in Data.Data)
                for (int i = 0; i < Data.Data.Count - 1; i++)
                {
                    GLLine(
                        GetPoint(Data.Data[i+0]),
                        GetPoint(Data.Data[i+1])
                        );
                    //GLCross(GetPoint(point), 3);
                }
                GL.End();
            }
            // -----------------
            */
            /*
            // --- Data (crosses) -----------------
            GL.Begin(GL.LINES);
            //GL.Color(Color.white);
            GL.Color(Attribute.DataColour.xA(0.5f));
            foreach (Vector2 point in Data.Data)
                GLCross(GetPoint(point), 3);
            GL.End();
            // -----------------
            */


            GL.PopMatrix();
            GUI.EndClip();




            // --- Labels -----------------
            // Only for OnDrawInspectorGUI
            //GUI.contentColor = Color.yellow;
            //GUI.Label(new Rect(GetX(Data.Quartile2.x), 0, 100, 50), Data.Quartile2.x + " ticks", EditorStyles.boldLabel);
            //GUI.Label(new Rect(0, GetY(Data.Quartile2.y), 100, 50), Data.Quartile2.y + " points", EditorStyles.boldLabel);

            // Only for OnGUI
            //GUI.contentColor = Attribute.MedianColour;
            //if (Attribute.LabelX != null)
            //    EditorGUI.LabelField(new Rect(rect.x + GetX(Data.Quartile2.x), rect.y + 0, 100, 50), Data.Quartile2.x + " " + Attribute.LabelX, EditorStyles.boldLabel);
            //if (Attribute.LabelX != null)
            //    EditorGUI.LabelField(new Rect(rect.x + 0, rect.y + GetY(Data.Quartile2.y), 100, 50), Data.Quartile2.y + " " + Attribute.LabelY, EditorStyles.boldLabel);




            //GUISkin skin = new GUISkin();
            //skin.box.border = new RectOffset(10, 10, 10, 10);
            //GUI.Box(rect,"",skin.box);
            // -----------------
            GUI.contentColor = new Color(1, 1, 1, 1);
        }

        // Draws line
        private void VerticalLine(float x)
        {
            GL.Vertex3(GetX(x), 0, 0);
            GL.Vertex3(GetX(x), Height, 0);
        }
        private void HorizontalLine(float y)
        {
            GL.Vertex3(0, GetY(y), 0);
            GL.Vertex3(Width, GetY(y), 0);
        }

        // A box in GL
        // bottomLeft, topRight
        private void GLRect(float x0, float y0, float x1, float y1)
        {
            float px0 = GetX(x0);
            float py0 = GetY(y0);

            float px1 = GetX(x1);
            float py1 = GetY(y1);

            GL.Vertex3(px0, py0, 0);
            GL.Vertex3(px1, py0, 0);
            GL.Vertex3(px1, py1, 0);
            GL.Vertex3(px0, py1, 0);
        }

        /*
        void GLCross(Vector3 vertex, int radius)
        {
            GL.Vertex3(vertex.x, vertex.y - radius, 0);
            GL.Vertex3(vertex.x, vertex.y + radius, 0);

            GL.Vertex3(vertex.x - radius, vertex.y, 0);
            GL.Vertex3(vertex.x + radius, vertex.y, 0);
        }
        */

        void GLLine(Vector3 pointA, Vector3 pointB)
        {
            GL.Vertex3(pointA.x, pointA.y, 0);
            GL.Vertex3(pointB.x, pointB.y, 0);
        }
    }
}