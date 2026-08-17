using System.Collections;
using System.Collections.Generic;
using AlanZucconi.AI.Evo;
using UnityEngine;

namespace AlanZucconi.AI.Evo
{
    /* A world specifically designed to work with ArrayGenome genomes. */
    public abstract class GenomeWorld : MonoBehaviour,
        IWorld<ArrayGenome>,
        IGenomeFactory<ArrayGenome>
    {
        #region IWorld
        public abstract void ResetSimulation();
        public abstract void SetGenome(ArrayGenome genome);
        public abstract void StartSimulation();
        public abstract bool IsDone();
        public abstract float GetScore();
        //public abstract ArrayGenome GetGenome();
        #endregion

        #region IGenomeFactory
        public abstract int GetGenomeSize();
        public abstract float GetMutationRate();

        public ArrayGenome Instantiate()
        {
            ArrayGenome genome = new ArrayGenome(GetGenomeSize(), GetMutationRate());
            genome.InitialiseRandom();
            return genome;
        }
        #endregion
    }
}