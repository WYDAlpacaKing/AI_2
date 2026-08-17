using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlanZucconi.Data;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    public abstract class PacmanAI : AgentAI
    {
        // Assumes PacmanAI is attached to a Pacman agent
        public Pacman Pacman
        {
            get => (Pacman) Agent;
        }

        #region Data
        // Used to store data about all tests
        //  and to later populate the plots
        // x: time
        // y: score
        // z: things eaten
        [SerializeField]
        [HideInInspector]
        public List<Vector3> ScoresData = new();


        [Header("Results")]

        [ScatterPlot(LabelX = "frames", LabelY = "things eaten", GridX = 10, GridY = 10)]
        //[SerializeField]
        public PlotData ThingsEatenPlot = new PlotData();
        
        [HistogramPlot(LabelX = "things eaten", LabelY = "count")]
        //[SerializeField]
        public PlotData ThingsEatenHistogram = new PlotData();

        [Space]

        [ScatterPlot(LabelX = "frames", LabelY = "points", GridX = 10, GridY = 1000)]
        //[SerializeField]
        public PlotData ScorePlot = new PlotData();
        //public PlotData ScatterData = new PlotData();

        [HistogramPlot(LabelX = "points", LabelY = "count")]
        //[SerializeField]
        public PlotData ScoreHistogram = new PlotData();



        //[Button(Editor=true)]
        // Uses the data stored in ScoresData to initialise the plots
        // Forces a plot update, regardless if the data has changed
        public void UpdatePlots()
        {
            ThingsEatenPlot.Data = ScoresData.Select(scores => scores.XZ()).ToList();
            ScorePlot      .Data = ScoresData.Select(scores => scores.XY()).ToList();
            
            ThingsEatenHistogram.Data = ScoresData.Select((scores, i) => new Vector2(i, scores.z)).ToList();
            ScoreHistogram      .Data = ScoresData.Select((scores, i) => new Vector2(i, scores.y)).ToList();

            ThingsEatenPlot     .Dirty = true;
            ScorePlot           .Dirty = true;
            ThingsEatenHistogram.Dirty = true;
            ScoreHistogram      .Dirty = true;

            ThingsEatenPlot     .CalculateStatistics();
            ScorePlot           .CalculateStatistics();

            ThingsEatenHistogram.CalculateStatistics();
            ScoreHistogram      .CalculateStatistics();


#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }



        #endregion
    }
}