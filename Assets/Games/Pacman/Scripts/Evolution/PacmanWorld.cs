using AlanZucconi;
using AlanZucconi.AI.Evo;
using AlanZucconi.Pacman;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.Pacman.Evo
{
    public class PacmanWorld : GenomeWorld
    {

        

        public PacmanGame Game;
        public PacmanAIEvo AI;
        [ReadOnly]
        public PacmanAIEvo AICopy = null;

        [Range(0f, 1f)]
        public float MutationRate = 0.01f;

        // Indicates which type of score to return
        public enum ScoreType
        {
            Time,
            Score,
            ThingsEaten,
            Mix,
            Mix30x70
        }

        public ScoreType Type = ScoreType.Score;

        /*
        // Clones the ghosts, so the world has its own private copies
        // This way all ghost AIs can run independently
        void Start()
        {
            foreach (Ghost ghost in Game.Ghosts)
                ghost.AI = Instantiate(ghost.AI); // Clones the values (shallow)
                //ghost.AI = ScriptableObject.CreateInstance(ghost.AI.GetType()) as GhostAI; 
        }
        */


        #region ArrayGenomeWorld
        public override void ResetSimulation()
        {
            // Creates the copy that will be used to change value
            // (PacmanGame will create its own further copy in StartGame)
            if (AICopy == null)
            {
                AICopy = Instantiate(AI);

                // To make thigns lighter, it clears the plot data
                //  so it doesn't get copied every time
                AICopy.ScoresData          ?.Clear();
                AICopy.ThingsEatenPlot     ?.Clear();
                AICopy.ThingsEatenHistogram?.Clear();
                AICopy.ScorePlot           ?.Clear();
                AICopy.ScoreHistogram      ?.Clear();
            }
        }

        //private ArrayGenome Genome;
        public override int GetGenomeSize() => AI.GetWeightsSize();
        public override float GetMutationRate() => MutationRate;

        public override void SetGenome(ArrayGenome genome) => SetWeights(genome.Params);
        public void SetWeights(float[] weights)
        {
            //Genome = genome;

            // Relative weights
            //float[] array = genome.Params
            //    .Select(x => x * 0.5f + 0.5f)
            //    .ToArray(); // [-1,+1] -> [0,1]
            //float sum = array.Sum();

            // Copies the weights
            AICopy.SetWeights(weights);

            // Sets the AI
            // (StartGame will make a futher copy)
            Game.PacmanAI = AICopy;
        }

        //public override ArrayGenome GetGenome() => Genome;
        public override void StartSimulation() => Game.StartGame(); // Will make a privat copy of Game.PacmanAI
        public override bool IsDone() => !Game.Running;
        //public override float GetScore() => Game.Score();
        //public override float GetScore() => Game.Score();
        public override float GetScore() =>
            Type switch
            {
                ScoreType.Time        => Game.Turn,
                ScoreType.Score       => Game.Pacman.Score,
                ScoreType.ThingsEaten => Game.Pacman.Score_ThingsEaten,
                ScoreType.Mix         => 0.5f * (Game.Pacman.Score_ThingsEaten / 260f) + 0.5f * (Game.Pacman.Score / 14600f),
                ScoreType.Mix30x70    => 0.3f * (Game.Pacman.Score_ThingsEaten / 260f) + 0.7f * (Game.Pacman.Score / 14600f),
                _                     => 0f // should not happen
            };

        #endregion


        private void OnDrawGizmos()
        {
            if (Game == null) return;
            if (Game.Level == null) return;
            if (Game.Level.Data == null) return;

            Vector3 size = new Vector3(Game.Level.Width, Game.Level.Height);
            DebugDraw.Rectangle(transform.position + size / 2f, size.x, size.y, Color.white);

            // Game over?
            if (! Game.Running)
            {
                Debug.DrawLine(transform.position, transform.position + new Vector3(Game.Level.Width, Game.Level.Height), Color.red);
            }
        }
    }
}