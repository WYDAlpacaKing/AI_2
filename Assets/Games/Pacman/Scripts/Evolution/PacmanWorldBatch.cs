using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static AlanZucconi.Pacman.Evo.PacmanWorld;

namespace AlanZucconi.Pacman.Evo
{
    // Instantiates several instances of a PacmanWorld
    // and tests them at the same time
    public class PacmanWorldBatch : MonoBehaviour
    {
        public PacmanWorld WorldPrefab;
        [ReadOnly]
        public List<PacmanWorld> Worlds;

        public Vector2Int Size; // How many to create on each axis
        public Vector2 Offset;

        public ScoreType Type = ScoreType.Score;

        public PacmanAIEvo AI;

        void Awake()
        {
            if (AI != null && Worlds.IsEmpty())
                InstantiateWorlds();
        }

        public void InstantiateWorlds ()
        {
            // Already created?
            if (Worlds.Any())
                return;

            for (int x =  0; x < Size.x; x ++)
            {
                for (int y = 0; y < Size.y; y++)
                {
                    PacmanWorld world = Instantiate(WorldPrefab, new Vector3(x * Offset.x, y * Offset.y), Quaternion.identity, transform);
                    world.AI = AI;
                    world.Type = Type;
                    Worlds.Add(world);
                }
            }
        }

        public int GetWeightsSize() => AI.GetWeightsSize();

        // Only used in AdamOptimizer
        // Not actually implement IWorld, otherwise this would be captured by the Evolution System
        #region ArrayGenomeWorld
        public void ResetSimulation()
        {
            // If worlds have not been created before, it does so now
            // It will only happen once
            //if (Worlds.IsEmpty())
            //    InstantiateWorlds();

            foreach (PacmanWorld world in Worlds)
                world.ResetSimulation();
        }

        //public int GetGenomeSize() => Worlds[0].GetGenomeSize(); // size of the first one
        public float GetMutationRate() => Worlds[0].MutationRate;

        //public void SetGenome(ArrayGenome genome)
        public void SetWeights(float[] weights)
        {
            foreach (PacmanWorld world in Worlds)
                world.SetWeights(weights);
            //world.SetGenome(genome);
        }

        //public ArrayGenome GetGenome() => Worlds[0].GetGenome();
        public void StartSimulation()
        {
            foreach (PacmanWorld world in Worlds)
                world.StartSimulation();
        }
        public bool IsDone() => Worlds.All(world => ! world.Game.Running);

        public float GetScore()
            => Worlds
            .Select(world => world.GetScore())
            .Median();
        //.Average();

        // Like GetScore(), but with IQR
        public (float q1, float q2, float q3) GetIQR()
            => Worlds
            .IQR(world => world.GetScore());
        #endregion
    }
}