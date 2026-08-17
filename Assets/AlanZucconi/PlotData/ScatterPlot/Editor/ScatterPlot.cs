using UnityEngine;
using UnityEditor;

using System.Linq;
//using System.Drawing;

namespace AlanZucconi.Data
{
    public class ScatterPlot
    {
        // The data and properties used to draw it
        public ScatterPlotAttribute Attribute;
        public PlotData Data;

        // Current size (based on Inspector size)
        public float Width, Height;

        // Singleton
        static Material Material = null;



        

        public ScatterPlot(PlotData data, ScatterPlotAttribute attribute)
        {
            Data = data;
            Attribute = attribute;

            Initialise();
        }

        public void Initialise ()
        {
            Data.CalculateStatistics();

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

        

        // https://answers.unity.com/questions/1360515/how-do-i-draw-lines-in-a-custom-inspector.html
        // From Data to Rect
        // point:  [0, maxX]
        // vertex: [0, rect.width]
        private float GetX(float x)
        {
            return (x / Data.Max.x) * Width;
        }
        private float GetY(float y)
        {
            return Height - (y / Data.Max.y) * Height;
        }
        private Vector2 GetPoint(Vector2 point)
        {
            return new Vector2(GetX(point.x), GetY(point.y));
        }


        //public void OnInspectorGUI()
        public void OnGUI(Rect rect)
        {
            // FIXME: when expanded, OnGUI somehow stops fields below it from being selected
            //  If those 2 lines are removed, fields are selectable,
            //  But the scatterplot is also drawn where it shouldn't.
            if (UnityEngine.Event.current.type != EventType.Repaint)
                return;

            if (Data == null || Data.Data.Count == 0)
                return;

            Data.CalculateStatistics(); // Not re-calculated if data is unchanged

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


            // --- Line -----------------
            GL.Begin(GL.LINES);
            GL.Color(Attribute.GridColor.xA(0.25f));

            for (float x = 0; x <= Data.Max.x; x += Attribute.Grid.x)
                VerticalLine(x);
            // FIXME: this equation is not right!
            float gridYstep =
                (Data.Max.y / Attribute.Grid.y >= Attribute.Height / 5f)
                ? Data.Max.y /  (Attribute.Grid.y*5f)       // lines are too close: does 1 every 2 pixel
                : Attribute.Grid.y;     // a horitzontal line every Grid.y points
            ///for (float y = 0; y <= Data.Max.y; y += Attribute.Grid.y)
            for (float y = 0; y <= Data.Max.y; y += gridYstep)
                HorizontalLine(y);

            VerticalLine(Data.Max.x);
            HorizontalLine(Data.Max.y);

            GL.End();
            // -----------------



            // --- Stats -----------------
            GL.Begin(GL.LINES);
            // Median
            //GL.Color(Color.yellow.xA(0.5f));
            GL.Color(Attribute.MedianColour.xA(0.5f));
            VerticalLine(Data.Quartile2.x);
            HorizontalLine(Data.Quartile2.y);

            /*
            // IQR
            GL.Color(new Color(1, 1, 0, 0.5f));
            verticalLine(quartile1X);
            verticalLine(quartile3X);

            GL.Color(new Color(1, 1, 0, 0.5f));
            horizontalLine(quartile1Y);
            horizontalLine(quartile3Y);
            */
            GL.End();

            // --- IQR -----------------
            GL.Begin(GL.QUADS);
            GL.Color(Attribute.MedianColour.xA(0.05f));
            GLRect(Data.Quartile1.x, 0, Data.Quartile3.x, Data.Max.y);
            GLRect(0, Data.Quartile1.y, Data.Max.x, Data.Quartile3.y);
            GL.End();
            // -----------------



            // --- Data -----------------
            GL.Begin(GL.LINES);
            //GL.Color(Color.white);
            GL.Color(Attribute.DataColour.xA(0.5f));
            foreach (Vector2 point in Data.Data)
                GLCross(GetPoint(point), 3);

            // Last point is drawn in red, so we can see the progress
            //GL.Color(Color.red);
            //GLCross(GetPoint(Data.Data.Last()), 3);

            GL.End();
            GL.PopMatrix();
            // -----------------

            // --- Labels -----------------
            int fontSize = EditorStyles.boldLabel.fontSize;
            GUI.contentColor = Color.yellow;
            GUI.Label(new Rect(GetX(Data.Quartile2.x), 0, 100, fontSize), $"{Data.Quartile2.x} {Attribute.LabelX}", EditorStyles.boldLabel);
            GUI.Label(new Rect(0, GetY(Data.Quartile2.y), 100, fontSize), $"{Data.Quartile2.y} {Attribute.LabelY}", EditorStyles.boldLabel);




            GUI.EndClip();



            // --- Labels -----------------
            // Only for OnDrawInspectorGUI
            //GUI.contentColor = Color.yellow;
            //GUI.Label(new Rect(GetX(Data.Quartile2.x), 0, 100, 50), Data.Quartile2.x + " ticks", EditorStyles.boldLabel);
            //GUI.Label(new Rect(0, GetY(Data.Quartile2.y), 100, 50), Data.Quartile2.y + " points", EditorStyles.boldLabel);

            /*
            // Only for OnGUI
            GUI.contentColor = Attribute.MedianColour;
            if (Attribute.LabelX != null)
                EditorGUI.LabelField(new Rect(rect.x + GetX(Data.Quartile2.x), rect.y + 0, 100, 50), Data.Quartile2.x + " " + Attribute.LabelX, EditorStyles.boldLabel);
            if (Attribute.LabelX != null)
                EditorGUI.LabelField(new Rect(rect.x + 0, rect.y + GetY(Data.Quartile2.y), 100, 50), Data.Quartile2.y + " " + Attribute.LabelY, EditorStyles.boldLabel);
            */
            

            


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


        void GLCross(Vector3 vertex, int radius)
        {
            GL.Vertex3(vertex.x, vertex.y - radius, 0);
            GL.Vertex3(vertex.x, vertex.y + radius, 0);

            GL.Vertex3(vertex.x - radius, vertex.y, 0);
            GL.Vertex3(vertex.x + radius, vertex.y, 0);
        }
    }
}