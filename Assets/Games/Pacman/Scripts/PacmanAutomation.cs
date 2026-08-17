using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using AlanZucconi.Data;

namespace AlanZucconi.Pacman
{
    public class PacmanAutomation : MonoBehaviour
    {
        [Header("Settings")]
        [EditorOnly]
        public PacmanGame Game;
        [Range(0f, 1f)]
        public float Delay = 0f;


        [Header("Testing Parameters")]
        [Range(1, 1000)]
        public int TestsPerAI = 500;
        public bool Rendering = false;
        //public bool PauseOnDeath = false;
        //[EditorOnly]
        public bool ClearData = true;

        [Space]
        public List<PacmanAI> AIs;

        [Header("Progress")]
        public ProgressBar AIBar;
        public ProgressBar ProgressBar;
        //[ProgressBar(label = "ProgressLabel")]
        //public float Progress = 0;
        //[HideInInspector]
        //public string ProgressLabel;


        // Use this for initialization
        //void Start()
        [Button(Editor = false)]
        public void Run()
        {
            //Snake.DeathCallback.AddListener(SimulationDone);

            StartCoroutine(Run_Coroutine());
        }

        IEnumerator Run_Coroutine()
        {
            // Runs as fast as possible
            QualitySettings.vSyncCount = 0; // Set vSyncCount to 0 so that using .targetFrameRate is enabled.
            Application.targetFrameRate = -1; // No target framerate

            // Progress
            int totalTests = AIs.Count * TestsPerAI;
            int testsDone = 0;

            //foreach (PacmanAI ai in AIs)
            foreach (PacmanAI ai in AIBar.Loop("AI", AIs))
            {
                //Debug.Log($"Testing AI: [{ai.name}]...");

                if (ClearData)
                {
                    ai.ScoresData = new();
                    //ai.ScatterData.Data.Clear();
                    //ai.HistogramData.Data.Clear();


                    //ai.ScatterData   = new PlotData();
                    //ai.HistogramData = new PlotData();

                    //ai.EatenData = new PlotData();


                }

                // Only the ones needed to get to TestsPerAI
                testsDone += ai.ScoresData.Count;
                //for (int i = 0 + ai.ScatterData.Data.Count; i < TestsPerAI; i++)
                foreach (int i in ProgressBar.Loop($"Testing", 0 + ai.ScoresData.Count, TestsPerAI))
                {
                    // Progress
                    //ProgressBar.Value = testsDone / (float)totalTests;
                    //ProgressBar.Label = $"Progress: {testsDone} of {totalTests} ({(int)(testsDone / (float)totalTests * 100)}%)";
                    testsDone++;


                    //Debug.Log($"\tSimulation {i}\tof {TestsPerAI}...");

                    // Setup
                    Game.Delay = Delay;
                    Game.Rendering = Rendering;
                    //Tetris.PauseOnDeath = PauseOnDeath;
                    Game.PacmanAI = ai; // TODO: SetAI(ai)?

                    // Starts the game
                    Game.StartGame();
                    yield return new WaitWhile(() => Game.Running); // Wait until simulation done
                    Game.StopGame();

                    CollectStats(ai);
                }
            }



            Debug.Log("DONE!");
        }





        public void CollectStats(PacmanAI ai)
        {

            ai.ScoresData.Add(new Vector3(Game.Turn, Game.Score(), Game.Pacman.Score_ThingsEaten));

            /*
            //ai.PlotData.Add(new Vector2(0, Tetris.Turn));
            //ai.PlotData.Add(new Vector2(ai.PlotData.Data.Count, Game.Turn));
            ai.ScatterData.Add(new Vector2(Game.Turn, Game.Score()));
            ai.EatenData.Add(new Vector2(Game.Turn, Game.Pacman.Score_ThingsEaten));

            ai.HistogramData.Add(new Vector2(ai.ScatterData.Data.Count, Game.Score()));

            */
            ai.UpdatePlots();



            // Calculates the statistics
            //ai.MedianScore  = ai.ScatterData.Data.Median (point => point.y);
            //ai.AverageScore = ai.ScatterData.Data.Average(point => point.y);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(ai);
#endif
        }


    }
}