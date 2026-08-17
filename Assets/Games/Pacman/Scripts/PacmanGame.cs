using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using AlanZucconi.Core;
using System.Data;
using Unity.Profiling;

namespace AlanZucconi.Pacman
{


    public class PacmanGame : Game
    {


        [Header("Level")]
        public PacmanLevel Level;
        public bool Rendering = true;




        [Header("Agents")]
        public bool MakeClonesOfAgentAIs = true;
        [Space]
        public List<Ghost> Ghosts;

        // These are the AI sto be used for the ghosts
        // When "MakeClonesOfAgentAIs=true",
        //  these are the onesthat will be cloned
        // They are copied only once, when GhostAIs is null
        private List<GhostAI> GhostAIs = null;


        [Space]
        public Pacman Pacman;
        public PacmanAI PacmanAI; // This is the AI that will be used for the player
        

        [Header("Cheats")]
        public bool Invincible = false;
        public bool DrawAI = false;
        //public bool FreezeGhosts = false;










        void Start()
        {
            Level.Build();
            Level.Draw();
        }



        #region Initialisation

        public override void InitialiseGame()
        {
            // Rebuilds the level
            Level.Build();
            if (Rendering)
                Level.Draw();

            // Instantiates the AI
            InitialiseAgents();
            if (Rendering)
                Level.DrawAgents();
        }

        // Clones an AI object (shallow copy)
        // and destroys the previous one (if it was created in the editor, and is not an asset)
        private static TAgentAI CloneAndDestroyOriginal<TAgentAI>(TAgentAI clonableAI, TAgentAI oldAI = null)
            where TAgentAI : AgentAI
        {
            // Clones the values (shallow copy)
            TAgentAI clonedAI = Instantiate(clonableAI);

            // Destroy the previous clone (if needed)
            if (oldAI != null &&
                !EditorUtility.IsPersistent(oldAI))
                Destroy(oldAI);

            return clonedAI;
        }
        

        /*
        public void SetAI(AgentAI ai)
        {
            PacmanAI = ai;
        }*/
        public void InitialiseAgents()
        {
            //Pacman.AI = PacmanAI;   // Uses the AI from the inspector

            if (!ValidateAIPath())
                Debug.LogWarning("AI name or folder might be incorrect! Please fix before assignment submission!");


            // Creates a private copy of each scriptable object,
            //  so the original ones are not changed
            // This also means that we can run multiple instances of this in parallel
            //  without the risk of sharing values!
            // Keep in mind this does NOT make a deep copy!
            // (although float[] are deep copied!)
            if (MakeClonesOfAgentAIs)
            {
                // Clones the AI for Pacman
                Pacman.AI = CloneAndDestroyOriginal(PacmanAI, Pacman.AI);


                // Retrieves a copy of the original ghost AIs
                //  (for the first time only)
                if (GhostAIs == null)
                    GhostAIs = Ghosts
                        .Select(ghost => ghost.AI as GhostAI)
                        .ToList();

                // Clones AIs for the ghosts
                foreach (var (ghost, originalGhostAI) in Ghosts.Zip(GhostAIs))
                    ghost.AI = CloneAndDestroyOriginal(originalGhostAI, ghost.AI);

                // Both PacmanAI and GhostAIs are not changed by this script
                // So they retain the original scriptable object that was used
                //
                // This means that restarting created further copies of them,
                //  not copies of copies of copies of copies...
                

                // Updates the PacmanAI reference
                // using the new copy
                //PacmanAI = Pacman.AI as PacmanAI;


                /*
                foreach (Agent agent in Agents)
                {
                    AgentAI oldAI = agent.AI;

                    agent.AI = Instantiate(agent.AI); // Clones the values (shallow copy)

                    // Clean up the previous clone if needed
                    if (!EditorUtility.IsPersistent(oldAI))
                        Destroy(oldAI);
                }
                */

            }



            // Intialises all the agents
            foreach (Agent agent in Agents)
                agent.Initialise(this);
        }
        #endregion


        //static readonly ProfilerMarker s_PreparePerfMarker = new ProfilerMarker("PacmanGame.UpdateGame");

        #region GameLoop
        public override void UpdateGame()
        {
            //s_PreparePerfMarker.Begin();

            UpdateAgentPositions(); // Includes AI, positions, and collisions

            // Once the agent positions are all updated,
            // we updates the states (pellets eaten, timers, ...)
            foreach (var agent in Agents)
                agent.UpdateState();

            //s_PreparePerfMarker.End();

            if (Rendering)
            {
                Level.Draw();
                Level.DrawAgents();
            }
        }

        public override bool IsGameOver()
        {
            // Out of time
            if (Turn >= MaxTurns)
                return true;

            // Player has died
            if (Pacman.IsEaten() && !Invincible)
                return true;

            // All pellets eaten
            if (Level.IsCleared())
                return true;

            // The game is still running!
            return false;
        }

        public override float Score() => Pacman.Score;
        #endregion




        #region Update

        public void UpdateAgentPositions()
        {
            // Updates the new positions
            foreach (Agent agent in Agents)
                agent.UpdatePosition();

            /* Test for collisions
             * 
             * Since all positions have been updated in the previous foreach loop,
             * there are many situations in which tunnel can occur.
             * For instance, two agents facing each other, would simply swap positions.
             * 
             * We need to detect if a collision occurred, in two ways:
             * [1] Two agents moving into the same tile
             * [2] Two agents swapping positions
             */
            // Use this one if you only want collisions between Pacman and the ghosts,
            // but not between the ghosts themselves
            foreach (var agentB in Ghosts)
            {
                //Agent agentA = Player;
                Pacman agentA = Pacman;

                bool movingIntoSameTile =
                    agentA.Position == agentB.Position;
                bool swappingPlaces =
                    agentA.OldPosition == agentB.Position &&
                    agentB.OldPosition == agentA.Position;

                if (movingIntoSameTile || swappingPlaces)
                    ResolveCollision(agentA, agentB);
            }
            /*
            foreach (var agentB in Ghosts)
            {
                Pacman agentA = Player;

                bool movingIntoSameTile =
                    agentA.Position == agentB.Position;
                bool swappingPlaces =
                    agentA.OldPosition == agentB.Position &&
                    agentB.OldPosition == agentA.Position;

                if (movingIntoSameTile || swappingPlaces)
                {
                    agentA.CollisionWith(agentB); // Pacman collides with the ghost
                    agentB.CollisionWith(agentA); // The ghost collides  with pacman
                }
            }*/
            /*
            // Use this one if you want all agents being able to collide with eachothers
            foreach (var (agentA, agentB) in Agents.DistinctPairs())
            {
                bool movingIntoSameTile =
                    agentA.Position == agentB.Position;
                bool swappingPlaces =
                    agentA.OldPosition == agentB.Position &&
                    agentB.OldPosition == agentA.Position;

                if (movingIntoSameTile || swappingPlaces)
                {
                    agentA.CollisionWith(agentB);
                    agentB.CollisionWith(agentA);
                }
            }*/

        }

        // TODO: perhaps a future version for (agent, agent)?
        public void ResolveCollision(Pacman pacman, Ghost ghost)
        {
            // Pacman eats the ghost
            if (pacman.CanEat(ghost))
            {
                pacman.OnEating(ghost);
                ghost.OnEatenBy(pacman);
                return;
            }

            // The ghost eats pacman
            if (ghost.CanEat(pacman))
            {
                ghost.OnEating(pacman);
                pacman.OnEatenBy(ghost);
                return;
            }
        }
        #endregion




        #region LevelAccess
        public Item this[int x, int y]
        {
            get => Level[x, y];
            set => Level[x, y] = value;
        }
        public Item this[Vector2Int position]
        {
            get => Level[position];
            set => Level[position] = value;
        }

        //public Vector2Int Loop(Vector2Int position) => Level.Loop(position);
        #endregion




        #region Iterators
        /** Iterates over all agents: both pacman and the ghosts. */
        public IEnumerable<Agent> Agents
        {
            get => Ghosts
                .Cast<Agent>()
                .Prepend(Pacman);
        }

        #endregion


        #region Ghosts
        /** Iterates over all ghosts/enemy agents. */
        //public IEnumerable<Agent> Ghosts()
        //    => Agents.Skip(1); // Assumes the first agent is the player/pacman
        //.Where(agent => ! agent.IsPlayer);


        // Retrieves a specific AI
        public IEnumerable<T> GhostsWithAI<T>()
            where T : AgentAI
            //=> Agents()
            => Ghosts
                .Where (agent => agent.AI is T)
                .Select(agent => agent.AI as T);

        // Retrieves blinky from the list of ghosts
        public GhostAI_Blinky BlinkyAI() =>
            GhostsWithAI<GhostAI_Blinky>()
            .FirstOrDefault();

        // Retrieves pinky from the list of ghosts
        public GhostAI_Pinky PinkyAI() =>
            GhostsWithAI<GhostAI_Pinky>()
            .FirstOrDefault();

        // Retrieves inky from the list of ghosts
        public GhostAI_Inky InkyAI() =>
            GhostsWithAI<GhostAI_Inky>()
            .FirstOrDefault();

        // Retrieves inky from the list of ghosts
        public GhostAI_Clyde ClydeAI() =>
            GhostsWithAI<GhostAI_Clyde>()
            .FirstOrDefault();
        #endregion




        #region MaxScores
        // TODO: these should only be called BEFORE the game started,
        //  otherwise Pellets() and PowerPellets() would be eaten already!

        // Returns the maximum number of eaten things score
        public int GetMaxThingsEaten()
        {
            int pellets      = Level.Pellets()     .Count();
            int powerPellets = Level.PowerPellets().Count();
            int ghosts       = Ghosts              .Count();

            return pellets + powerPellets + (powerPellets * ghosts);
        }
        // Returns the maximum number of eaten things score
        public int GetMaxScore()
        {
            int pellets      = Level.Pellets()     .Count();
            int powerPellets = Level.PowerPellets().Count();
            int ghosts       = Ghosts              .Count();

            // Calculates the maximum score of eating all ghosts
            int score_eatingGhost = 0;
            for (int g = 0; g < ghosts; g ++)
                score_eatingGhost += Pacman.Score_Ghost * (int) Mathf.Pow(2, g);

            return
                pellets * Pacman.Score_Pellet +
                powerPellets * Pacman.Score_PowerPellet +
                (powerPellets * score_eatingGhost);
        }
        #endregion


        /*
        [Button(Editor = true)]
        public void GenerateLevel()
        {
            string level = PacmanMazeGenerator2.GenerateLevel();
            Debug.Log(level);
        }


        [Button(Editor = false)]
        public void RegenerateLevel()
        {
            Level.Text = PacmanMazeGenerator2.GenerateLevel();
            Level.Build();
            Level.Draw();
        }
        */


        #region ValidateAIPath

        // This method checks if the scriptable object SnakeAI
        // is in the "right" folder.
        // This is to test if the students have put this in the "right" folder.
        public bool ValidateAIPath()
        {
            string path = UnityEditor.AssetDatabase.GetAssetPath(PacmanAI);

            // Is this an example AI?
            if (path.Contains("Solutions") ||
                path.Contains("Examples"))
                return true;

            // Path must have at least one of the allowed formats
            return
                ValidateAIPath_Goldsmiths(path) ||
                ValidateAIPath_GameAI(path);
        }



        public bool ValidateAIPath_Goldsmiths(string path)
        {
            // Assets/Pacman/AIs/Goldsmiths/2018-19/azucc002/PacmanAI_azucc002.asset
            string pattern = @"Assets/Games/Pacman/AIs/Goldsmiths/20\d\d-\d\d/(\w\w\w\w?\w?\d\d\d)/PacmanAI_(\w\w\w\w?\w?\d\d\d)(_resit\d)??.asset";

            Match match = Regex.Match(path, pattern);

            // No match?
            if (!match.Success)
                return false;

            // Folder names not correct?
            if (match.Groups[1].Value != match.Groups[2].Value)
                return false;

            return true;
        }

        // Validates the path for submissions made through the Game AI course
        public bool ValidateAIPath_GameAI(string path)
        {
            // Assets/Games/Pacman/AIs/Game AI/ORD000000/SnakeAI_ORD000000.asset
            //string pattern = @"Assets/Games/Pacman/AIs/Game AI/ORD(\d\d\d\d\d\d)/PacmanAI_(\d\d\d\d\d\d)(?:_[^.]+)?\.asset";
            string pattern = @"Assets/Games/Pacman/AIs/Game AI/(\w+\d\d\d)/PacmanAI_(\w+\d\d\d)(?:_[^.]+)?\.asset";

            Match match = Regex.Match(path, pattern);

            // No match?
            if (!match.Success)
                return false;

            // Folder names not correct?
            if (match.Groups[1].Value != match.Groups[2].Value)
                return false;

            return true;
        }
        #endregion
    }
}