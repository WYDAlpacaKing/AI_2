using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.AI.Evo
{
    // An array which can be used with the evolution algorithm
    [System.Serializable]
    public struct ArrayGenome : IGenome
    {
        public float[] Params;

        public float MutationRate;

        public ArrayGenome(int n, float mutationRate = 0.1f)
        {
            Params = new float[n];
            MutationRate = mutationRate;
        }

        #region Evolution
        // Copies this genome
        public IGenome Copy()
        {
            ArrayGenome copy = new ArrayGenome();
            copy.MutationRate = MutationRate;
            copy.Params = (float[])Params.Clone();

            return copy;
        }

        // Picks a random element and mutates it
        public void Mutate()
        {

            int i = Random.Range(0, Params.Length);


            if (Random.Range(0f, 1f) >= 0.1f)
            {
                // 90% chance of small change
                float value = Params[i] + Random.Range(-MutationRate, +MutationRate);
                Params[i] = Mathf.Clamp(value, -1f, +1f);
            } else
            {
                // 10% change of completely new value
                Params[i] = Random.Range(-1f, +1f);
            }
        }

        public void InitialiseRandom()
        {
            for (int i = 0; i < Params.Length; i++)
                Params[i] = Random.Range(-1f, +1f);
        }
        #endregion

        // Calculates the root mean square error between two genomes
        public static float RMSE (ArrayGenome g1, ArrayGenome g2)
        {
            float mse =
                Enumerable.Range(0, g1.Params.Length)
                .Select(i => Mathf.Pow(g1.Params[i] - g2.Params[i], 2f))
                .Average();
            return Mathf.Sqrt(mse);
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            s.Append("[");
            for (int i = 0; i < Params.Length; i++)
            {
                s.Append(Params[i]);
                s.Append(", ");
            }
            s.Append("]");

            return s.ToString();
        }
    }
}