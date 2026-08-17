using UnityEngine;

namespace AlanZucconi.Pacman.Evo
{
    public abstract class PacmanAIEvo : PacmanAI
    {
        [Header("Evolution")]
        public float[] Weights;

        public virtual int GetWeightsSize()
            => Weights.Length;

        //public virtual void SetGenome(ArrayGenome genome)
        public virtual void SetWeights(params float [] weights)
        {
            for (int i = 0; i < Weights.Length; i++)
                Weights[i] = weights[i];
        }
    }
}