using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.Data;

namespace AlanZucconi.Pacman
{
    public abstract class AgentAI : ScriptableObject
    {
        
        [Header("Student Data")]
        public string StudentLogin = "yourlogin";
        public string StudentName = "FirstName LastName";
        public string StudentEmail = "youremail@gold.ac.uk";

        /*
        // Will be initialised by the Automation tool
        [Header("Statistics")]
        [Space]
        [ReadOnly]
        public float MedianScore = 0;
        [ReadOnly]
        public float AverageScore = 0;

        [Header("Results")]
        //[LinePlot(LabelX = "test", LabelY = "points")]
        //[ScatterPlot(LabelX = "test", LabelY = "points")]
        //[HistogramPlot(Bins=15, LabelX = "points", LabelY = "count")]
        [HistogramPlot(LabelX = "points", LabelY = "count")]
        public PlotData PlotData = new PlotData();
        */

        [HideInInspector]
        public PacmanGame Game;
        [HideInInspector]
        public Agent Agent;



        public abstract Action Move();

        // Can be used to initialisation
        public virtual void Initialise() { }

        // Used for debug drag
        public virtual void Draw() { }


        // Easy access
        public Vector2Int Position
        {
            get => Agent.Position;
        }
    }
}