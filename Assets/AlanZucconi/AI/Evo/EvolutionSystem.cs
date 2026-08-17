using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using AlanZucconi.Data;
using UnityEngine.Events;

namespace AlanZucconi.AI.Evo
{

    public enum SelectionStrategy
    {
        // Takes the top k, based on fitness
        // https://en.wikipedia.org/wiki/Truncation_selection
        Truncation = 1,
        // p of being selected is based on relative fitness
        // (roulette wheel selection)
        // https://en.wikipedia.org/wiki/Fitness_proportionate_selection
        FitnessProportionate = 2,
        // https://en.wikipedia.org/wiki/Tournament_selection
        Tournament = 4,
        // p of being selected is based on relative culmulative reward (including parents)
        // https://en.wikipedia.org/wiki/Reward-based_selection
        RewardBase = 8
    }

    // An Evolution System that uses ArrayGenome
    public class EvolutionSystem : EvolutionSystem<ArrayGenome>
    {
    }

    /*
     * All worlds linked are executed in parallel, and recycled.
     * They need to be linked manually.
     */
    public abstract class EvolutionSystem<T> : MonoBehaviour
        where T : IGenome
    {
        //public IGenomeFactory<T> Factory;
        //public List<IWorld<T>> Worlds;
        //public List<T> Population;

        #region WorldData
        // A class used to store the data of each world
        //  as the evolution algorithm progresses
        [System.Serializable]
        public class WorldData
        {
            public IWorld<T> World;
            public float Score = 0; // Current score
            public T Genome; // Current genome

            // Used to test the world
            public bool IsDone = false;
            public int testsDone = 0;

            // Used to error bars
            public float Quartile1;
            public float Quartile3;

            public WorldData(IWorld<T> world) => World = world;

            // A coroutine that tests this world several times
            //  with the given genome
            //
            // A function is passed to get the result, since we cannot use a ref/out in an iterator
            public IEnumerator BatchScore(int tests)
            {
                IsDone = false;
                testsDone = 0;

                List<float> scores = new List<float>(tests);

                //for (int i = 0; i < tests; i++)
                for (testsDone = 0; testsDone < tests; testsDone++)
                {
                    // Runs the simualation
                    World.ResetSimulation();
                    World.SetGenome(Genome);
                    World.StartSimulation();

                    // Waits until it is done
                    yield return new WaitUntil(() => World.IsDone());

                    scores.Add(World.GetScore());
                }

                //Score = scores.Median();
                //Quartile1 = scores.Quartile1();
                //Quartile3 = scores.Quartile3();
                (Quartile1, Score, Quartile3) = scores.IQR(x => x);

                IsDone = true;
            }
        }

        public List<WorldData> Worlds;
        #endregion




        



        [Header("Manual initialisation")]
        public bool AddFirstGenome = false;
        // This one genome is added to the first generation
        public T FirstGenome;

        [Header("Settings")]
        [Min(1)]
        public int Generations;
        [Min(1)]
        public int TestsPerGenome = 1;


        

        [Header("Mutations")]
        [Min(1)]
        public int Mutations; // How many mutations
        [Space]
        public bool AdaptiveMutations = true;
        public int AdaptiveMutationPerGeneration = 10;
        public int MaxAdaptiveMutations = 20;



        [Header("Selection Strategy")]
        [Min(0)]
        public int TopK = 1; // elitist selection: number of top genomes that are brought forward with no mutations
        [Min(0)]
        public int RandomGenomes = 0; // adds this many random genomes back into the pool

        [Space]
        public SelectionStrategy Selection = SelectionStrategy.Truncation;

        [ShowIf("Selection", SelectionStrategy.Truncation)]
        [Range(0f, 1f)]
        public float SurvivalRate; // % of worlds that survive between generations

        [ShowIf("Selection", SelectionStrategy.Tournament)]
        public int TournamentSize = 4;
        // Larger tournament sizes generally lead to stronger selection intensity,
        // meaning that individuals with lower fitness values have a smaller chance of being selected.




        //[Header("Simulated Annealing")]
        //[Header("Adaptive Mutations")]

        [Header("Events")]
        public UnityEvent StartEvent;
        //public UnityEvent EndEvent;



        [Header("Results")]
        [ReadOnly]
        public float BestScoreSoFar = float.NegativeInfinity;
        [ReadOnly]
        public int GenerationsWithoutImprovement = 0;

        [Space]

        [LinePlot(LabelX = "Generations", LabelY = "Score")]
        public PlotData PlotData = new PlotData();


        [Header("Progress")]
        public ProgressBar GenerationBar;
        public ProgressBar TestBar;




        [Button]
        public void StartEvolution ()
        {
            StartCoroutine(StartEvolutionCoroutine());
        }

        IEnumerator StartEvolutionCoroutine ()
        {
            // ======================
            // === INITIALISATION ===
            // ======================
            StartEvent.Invoke();

            // FindObjectsOfType cannot retrieve interfaces
            // So we get all monobehaviours and filter for World<T>
            Worlds = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IWorld<T>>()
                .Select(world => new WorldData(world))
                .ToList();
            //Worlds = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            //    .OfType<IWorld<T>>()
            //    .ToList();


            // Uses the first GenomeFactory<T> to instantiate the genomes
            var genomeFactory = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .OfType<IGenomeFactory<T>>()
                .First();

            // Waits one frame to make sure Awake() and Start() have been called
            yield return null;




            // ========================
            // === FIRST POPULATION ===
            // ========================
            List<T> genomePool = new List<T>(Worlds.Count);

            if (AddFirstGenome)
            {
                // Adds the first genome
                genomePool.Add((T)FirstGenome.Copy());
                // Adds mutations of the first genome
                for (int i = 0 + 1; i < Worlds.Count; i++)
                {
                    T genome = (T) FirstGenome.Copy();
                    genome.Mutate(Random.Range(1, Mutations+1));
                    genomePool.Add(genome);
                }
            }
            else
            {
                // Initialises the random population
                for (int i = genomePool.Count(); i < Worlds.Count; i++)
                {
                    T genome = genomeFactory.Instantiate();
                    genomePool.Add(genome);
                }
            }
       

            // ======================
            // === EVOLUTION LOOP ===
            // ======================

            // Loops through the generations
            //for (int generation = 0; generation < Generations; generation++)
            foreach (int generation in GenerationBar.Loop("Generation", Generations))
            {
                // -----------------------------------------
                // [SET GENOMES]
                // Assigns the genomes from the gene pool to the worlds
                for (int i = 0; i < genomePool.Count; i++)
                    Worlds[i].Genome = genomePool[i];



                // -----------------------------------------
                // [TESTS]
                // Tests each world a numer of times
                // to make sure the score are reliable
                // Each world is reused as soon as its simulation ends,
                // so we maximises speed

                // Tests each world in parallel
                foreach (WorldData world in Worlds)
                    StartCoroutine(world.BatchScore(TestsPerGenome));

                // Waits for all tests to be done
                yield return new WaitUntil
                (
                    () =>
                    {
                        TestBar.Update
                        (
                            "Test",
                            Worlds.Sum(world => world.testsDone),   // All tests done so far
                            TestsPerGenome * Worlds.Count           // Total number of tests
                        );
                        return Worlds.All(world => world.IsDone);
                    }
                );
                //yield return new WaitUntil
                //(
                //    () => Worlds.All(world => world.IsDone)
                //);

                // -----------------------------------------
                // [FITNESS]
                // Sorts the worlds based on the genome scores
                Worlds
                    .Sort((worldA, worldB) => worldB.Score.CompareTo(worldA.Score)); // Descending order

                // Updates scores
                float maxScore = Worlds[0].Score;
                if (maxScore > BestScoreSoFar + 1e-4f) // small epsilon to avoid float imprecision
                {
                    BestScoreSoFar = maxScore;
                    GenerationsWithoutImprovement = 0;
                }
                else
                {
                    GenerationsWithoutImprovement++;
                }

                // Plot & Log
                PlotData.Add(generation, maxScore, Worlds[0].Quartile1, Worlds[0].Quartile3); // Error bars
                Debug.Log
                (
                    Worlds.Aggregate
                    (
                        $"=> Generation {generation}: {maxScore}\n",
                        (s, world) => $"{s}\tScore: {world.Score}\t{world.Genome}\n"
                    )
                );


                // -----------------------------------------
                // [SELECTION STRATEGY]
                // Genomes for the next generation
                genomePool.Clear();

                // Elitist selection: the top genomes are brought forward with no changes
                // Adds the best one back with no mutations
                for (int i = 0; i < TopK; i++)
                    genomePool.Add( (T) Worlds[i].Genome.Copy() );

                // Adds new random ones
                for (int i = 0; i < RandomGenomes; i++)
                    genomePool.Add(genomeFactory.Instantiate());


                /*
                // Fills the rest of the population
                //  picking genomes from the top % (based on Survival Rate)
                //  and mutates them
                int topGenomesToPick = (int)(Worlds.Count * SurvivalRate); // Top genomes
                for (int i = 0 + genomePool.Count(); i < Worlds.Count; i++)
                {
                    T genome = (T)Worlds[Random.Range(0, topGenomesToPick)].Genome.Copy();
                    genome.Mutate(Random.Range(1, Mutations + 1));
                    genomePool.Add(genome);
                }
                */

                // Fills in the rest of the genome pool
                //  using the chosen selection strategy
                int remainingGenomes = Worlds.Count - genomePool.Count();
                var selectedGenomes = (Selection switch
                {
                    SelectionStrategy.Truncation           => TruncationSelection       (remainingGenomes),
                    SelectionStrategy.FitnessProportionate => FitnessProportionSelection(remainingGenomes),
                    SelectionStrategy.Tournament           => TournamentSelection       (remainingGenomes),
                    _ => throw new System.Exception("Selection strategy not implemented yet!")
                }
                ).ToList();
                MutateGenomes(selectedGenomes);
                genomePool.AddRange(selectedGenomes);


                // Waits next frame before restarting
                yield return null;
            }

            //EndEvent.Invoke();
        }

        #region SelectionStrategy
        // Fills the rest of the population
        //  picking genomes from the top % (based on Survival Rate)
        // The list returned is made out of copies of the existing genomes.
        // Worlds has to be already sorted
        // https://en.wikipedia.org/wiki/Truncation_selection
        private IEnumerable<T> TruncationSelection (int count)
        {   
            int topGenomesToPick = (int)(Worlds.Count * SurvivalRate); // Top genomes
            //for (int i = 0 + genomePool.Count(); i < Worlds.Count; i++)
            for (int i = 0; i < count; i++)
                yield return (T) Worlds[Random.Range(0, topGenomesToPick)].Genome.Copy();
        }

        // https://en.wikipedia.org/wiki/Tournament_selection
        private IEnumerable<T> TournamentSelection(int count)
        {
            for (int i = 0; i < count; i++)
                yield return (T) Worlds
                    // Takes "TournamentSize" genomes at random
                    .OrderBy(_ => Random.value)
                    .Take(TournamentSize)
                    // Takes the best one from the tournament sample
                    .MaxBy(genome => genome.Score)
                    .Genome.Copy();
        }

        // https://en.wikipedia.org/wiki/Fitness_proportionate_selection
        private IEnumerable<T> FitnessProportionSelection(int count)
        {
            float scoreSum = Worlds
                .Select(world => world.Score)
                .Sum();

            for (int i = 0; i < count; i++)
                yield return (T) Worlds
                    .RandomProbability(world =>  world.Score / scoreSum)
                    .Genome.Copy();
        }

        // Mutates the genomes in a list
        // Changes the genomes in the existing list
        private void MutateGenomes (List<T> genomes)
        {
            // One extra mutation every 10 generations without improvement
            // Max 20 mutations
            int adaptiveMutations =
                AdaptiveMutations
                ? Mathf.Clamp(GenerationsWithoutImprovement / AdaptiveMutationPerGeneration, 0, MaxAdaptiveMutations)
                : 0;

            foreach (T genome in genomes)
                genome.Mutate(Random.Range(1, Mutations + adaptiveMutations + 1));
                //genome.Mutate(Random.Range(1, Mutations + 1));
        }
        #endregion
    }
}