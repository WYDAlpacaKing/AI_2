using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AlanZucconi.Pacman.GhostAI;

namespace AlanZucconi.Pacman
{
    // Used to decide 
    public class GhostTimer : MonoBehaviour
    {
        [System.Serializable]
        public struct Interval
        {
            public float Duration;
            public GhostState State;
        }

        [Header("Scatter Times")]
        // https://www.gamedeveloper.com/design/the-pac-man-dossier
        // The list of time frames after which the AI switches to scatter mode
        // 7 20 7 20 5 20 5 forever
        // S C  S C  S C  S C
        public List<Interval> Intervals;
        public bool LoopTime = true;



        private IntervalList<GhostState> Timer;

        public GhostState this[float time]
        {
            get => Timer[time];
        }


        // Start is called before the first frame update
        void Start()
        {

            // TODO make this a game object we can refer to!

            const int FPS = 10; // frame per second

            Timer = new();
            Timer.LoopTime = true;

            foreach (Interval interval in Intervals)
                Timer.Append(interval.Duration * FPS, interval.State);
        }


        
    }
}