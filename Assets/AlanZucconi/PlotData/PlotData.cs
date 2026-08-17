using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Unity.Profiling;

namespace AlanZucconi.Data
{
    [Serializable]
    public class PlotData
    {
        //[NonSerialized]
        //public List<Vector2> Data = new List<Vector2>();
        
        public List<Vector2> Data = null;


        // Standard constructor 
        // Data is populated manually
        public PlotData () => Data = new List<Vector2>();



        /*
        private Func<List<Vector2>> DataSource = null;

        // This constructor is used to feed the Data from an external source
        // In this case, the Add methods should not be used!
        // Data will be initialised when data is recalculated
        public PlotData (Func<List<Vector2>> dataSource) => DataSource = dataSource;
        */


        public void Add(Vector2 point)
        {
            Data.Add(point);
            Dirty = true;
        }
        // Simple access
        public void Add(float x, float y)
            => Add(new Vector2(x, y));




        #region Bars
        // For each point in Data, indicates the min/max values of the bar
        // If null, there are no bars!
        public List<Vector2> BarsData = null;
        public void AddBars(Vector2 bars)
        {
            if (BarsData == null)
                BarsData = new();

            BarsData.Add(bars);
        }

        // Simple access
        public void Add(float x, float y, float minBar, float maxBar)
        {
            Add(new Vector2(x, y));
            AddBars(new Vector2(minBar, maxBar));
        }
        #endregion  

        public Vector2 this[int i]
        {
            get
            {
                return Data[i];
            }
        }



        // Statistics
        [HideInInspector] public bool Dirty = true; // Statistics needs to be recalculated
        [HideInInspector] public Vector2 Min;
        [HideInInspector] public Vector2 Max;
        [HideInInspector] public Vector2 Quartile1; // 25%
        [HideInInspector] public Vector2 Quartile2; // Median
        [HideInInspector] public Vector2 Quartile3; // 75%

        public void CalculateStatistics()
        {
            if (!Dirty)
                return;

            // Retrieves data from DataSource
            //if (DataSource != null)
            //    Data = DataSource();

            if (Data == null)
                return;

            if (Data.Count == 0)
                return;

            Min = new Vector2
            (
                Data.Min(point => point.x),
                Data.Min(point => point.y)
            );

            Max = new Vector2
            (
                Data.Max(point => point.x),
                Data.Max(point => point.y)
            );

            // Interquartile range
            (float q1x, float q2x, float q3x) = Data.IQR(point => point.x);
            (float q1y, float q2y, float q3y) = Data.IQR(point => point.y);

            Quartile1 = new Vector2(q1x, q1y);
            Quartile2 = new Vector2(q2x, q2y); // Median
            Quartile3 = new Vector2(q2x, q3y);

            Dirty = false;
        }

        public void Clear()
        {
            if (Data != null)
                Data.Clear();

            if (BarsData != null )
                BarsData.Clear();

            Dirty = true;
        }
    }
}