using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace AlanZucconi.Core
{
    // TODO: not used yet!
    public abstract class Game : MonoBehaviour
    {
        [Header("Game")]
        [ReadOnly]
        public bool Running = false;
        [Range(0, 5)]
        public float Delay = 1; // seconds
        [Space]
        [ReadOnly]
        public int Turn = 0;
        public int MaxTurns = 1000000;


        //[Space]
        // TODO: implement
        //public bool PauseOnDeath = false;

        

        #region GameLoop
        [Button(Editor = false)]
        public virtual void StartGame()
        {
            StartCoroutine(GameLoop_Coroutine());
        }
        public IEnumerator GameLoop_Coroutine()
        {
            // Only one running
            if (Running)
                yield break;

            // Initialises the game
            Turn = 0;
            InitialiseGame();
            Running = true;

            // Skips a frame, so if we start the game paused
            // we can see the state of the game before any more has taken place
            yield return null;



            // Game loop
            Stopwatch stopwatch = new Stopwatch();
            while (Running)
            {
                Turn++;

                // Updates the game
                // (and measures how long it took)
                stopwatch.Restart();
                UpdateGame();

                // Game over
                if (IsGameOver())
                {
                    StopGame();
                    yield break;
                }

                stopwatch.Stop();

                // Delay
                float elapsedSeconds = (float) stopwatch.Elapsed.TotalSeconds;
                float waitTime = Mathf.Max(0f, Delay - elapsedSeconds);
                if (waitTime == 0)
                    yield return null;
                else
                    yield return new WaitForSeconds(waitTime);

                // Pause
                if (!Running)
                    yield return new WaitWhile(() => !Running);
            }

            Running = false;
            stopwatch.Stop();
        }


        [Button(Editor = false)]
        public virtual void TogglePause() => Running = !Running;


        [Button(Editor = false)]
        public virtual void StopGame() => Running = false;
        // FIXME: StopGame() followed ty TogglePause() will unpause the game!
        #endregion



        public abstract void InitialiseGame();
        public abstract void UpdateGame();
        public abstract bool IsGameOver();
        public abstract float Score();
    }
}