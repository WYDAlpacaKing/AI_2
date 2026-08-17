using System.Collections.Generic;
using System;
using System.Linq;

namespace AlanZucconi.Core
{

    [System.Serializable]
    public struct DeterministicRandom
    {
        public enum SequenceType
        {
            Random, // A random seed for the generator
            Seeded  // An input seed for the generator
        }

        //[Header("Randomness")]
        // If true, the tetromino sequence is random
        public SequenceType Sequence;// = SequenceType.Random;
                                     // Used to randomise the sequence
        //[ShowIf("Sequence", SequenceType.Seeded)]
        public int Salt;

        public void Initialise()
        {
            // If the sequence is random,
            // we initialise the salt randomly
            // Otherwise, we skip this step
            // and leave its value unchange
            if (Sequence == SequenceType.Random)
                Salt = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        // Get the random source associated with the value "x"
        public System.Random Get(int x)
        {
            // Seed based on the current turn (so is predictable)
            int seed = x ^ Salt;
            return new System.Random(seed);
        }
    }

    public static class RandomSourceExtension
    {
        // Retrieves a random element from an iterator,
        //  using a RandomSouce to make the randomness deterministic
        public static T DeterministicRandom<T>(this IEnumerable<T> source, DeterministicRandom random, int x)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            int count = source.Count();

            if (count == 0)
                throw new InvalidOperationException("The sequence is empty.");

            int i = random.Get(x).Next(count);
            return source.ElementAt(i);
        }
    }
}