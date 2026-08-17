using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor.VersionControl;
using UnityEngine.Rendering.VirtualTexturing;
using System.Buffers.Text;
using UnityEditor;
using AlanZucconi.Core;

namespace AlanZucconi.Pacman
{
    // https://pacman.holenet.info/#Chapter_3
    // https://gameinternals.com/understanding-pac-man-ghost-behavior
    // https://pacman.holenet.info/#CH2_Frightening_Behavior
    // https://pacman.fandom.com/wiki/Maze_Ghost_AI_Behaviors
    /* Ghost AI:
     * 
     * - All ghost have the same pathfindin AI
     * - What changes is how they calculate the target
     * - Ghosts:
     *      - They pick the action that makes you closer to the target
     *      - Never reverses direction (unless state has changed!)
     *      - When frightened, they move randomly
     *      - On becoming frightened, they reverse direction
     */


    // Agents speeds:
    // https://www.designoriented.net/blog/2015/06/30/2015630pac-man-design-variables-of-difficulty/
    // Level 1:
    //  Pacman speed: 80%    = 1.125 pixels/frame
    //  Ghost speed: 75%        = 0.9375 pixels/frame
    //  Power pellet speed: 90%
    //  Frightened ghost speed: 50% = 0.625 pixels/frame
    // ==> Ghost 100% speed = 1.25 pixels/frame = 75.75... pixels/second = 9.46... tiles/second
    // ==> Pacman 100% speed = 1.40625 pixles/frame = 85.22... pixels/second = 10.65... tiles/second
    //
    // // https://www.reddit.com/r/Pacman/comments/1cg2ogp/does_anyone_know_the_pixel_per_frame_speeds_of/
    // Original Pacman runs at 60.6..fps
    // Tiles are 8x8 pixels

    // TODO: ghost comes out after x many pellets have been eaten.
    //  Blinky: 0
    //  Pinky: 0
    //  Inky: 30
    //  Clyde: 60
    // https://pacman.holenet.info/#CH2_Home_Sweet_Home


    public abstract class GhostAI : AgentAI
    {
        public enum GhostState
        {
            Scatter,
            Chase,
            Frightened,
            Eaten
        }
        [Header("State")]
        public GhostState State = GhostState.Scatter;
        //public DeterministicRandom RandomSource; // Used for the Frightened state

        [HideInInspector]
        public Vector2Int Target;
        [HideInInspector]
        public Action CurrentDirection = Action.None;

        // The last position that was different from the current one
        // This is used to make sure the Ghost does not go "backward"
        //  when moving alongside curved corridors.
        // "Agent" has "OldPosition, but that is not the last different position,
        //  and can  potentially be the same as the current one!
        [HideInInspector]
        public Vector2Int LastDifferentPosition;



        [Header("Known Locations")]
        public Vector2Int GhostHousePosition;
        public Vector2Int ScatterTarget;




        [Header("Speeds")]
        [Range(0f,1f)]
        public float DefaultSpeed = 0.75f;
        [Range(0f, 1f)]
        public float FrigthenedSpeed = 0.5f;
        [Range(0f, 1f)]
        public float EatenSpeed = 1.0f;

        
 


        // Assumed GhostAIs are attached to Ghosts
        // This allows to easily access them
        public Ghost Ghost
        {
            get => (Ghost) Agent;
        }

        public override void Initialise()
        {
            //base.Initialise();

            // Reset
            State = GhostState.Scatter;
            CurrentDirection = Action.None;
            Agent.Speed = DefaultSpeed;

            LastDifferentPosition = Position;

            //RandomSource.Initialise();
        }

        private void UpdateStateAndTarget()
        {
            // https://pacman.holenet.info/#CH2_Frightening_Behavior
            // Ghosts are forced to reverse direction by the system anytime the mode changes from:
            // chase-to-scatter, chase-to-frightened, scatter-to-chase, and scatter-to-frightened.
            // Ghosts do not reverse direction when changing back from frightened to chase or scatter modes.


            // ===========================================
            // Eaten event
            switch (State)
            {
                // ---------------------------
                case GhostState.Scatter:
                case GhostState.Chase:
                case GhostState.Frightened:
                    // [X -> Eaten]
                    if (Ghost.IsEaten())
                    {
                        CurrentDirection = Action.None; // Can change direction
                        State = GhostState.Eaten;
                        break;
                    }
                    break;
            }

            // ===========================================
            // Frightening event
            // Timer event
            // Resurrect event
            switch (State)
            {
                // ---------------------------
                case GhostState.Scatter:
                case GhostState.Chase:
                    // [X -> Frightened]
                    if (Ghost.IsFrightened())
                    {
                        CurrentDirection = CurrentDirection.Reverse();
                        State = GhostState.Frightened;
                        break;
                    }

                    // [X -> Scatter/Chase]
                    if (Ghost.Timer[Game.Turn] != State)
                    {
                        CurrentDirection = CurrentDirection.Reverse();
                        State = Ghost.Timer[Game.Turn];
                        break;
                    }
                    break;

                // ---------------------------
                case GhostState.Frightened:
                    // [Frightened -> Scatter/Chase]
                    if (! Ghost.IsFrightened())
                    {
                        CurrentDirection = CurrentDirection.Reverse();
                        State = Ghost.Timer[Game.Turn];
                        break;
                    }
                    break;

                // ---------------------------
                case GhostState.Eaten:
                    // [Dead -> Chase]
                    // Ghost house reached!
                    if (Position == GhostHousePosition)
                    {
                        Ghost.Resurrect();

                        CurrentDirection = Action.None; // Reset direction
                        State = Ghost.Timer[Game.Turn];
                        break;
                    }
                    break;
            }




            // Target
            switch (State)
            {
                // ---------------------------
                case GhostState.Scatter:
                    Target = ScatterTarget;
                    break;

                // ---------------------------
                case GhostState.Chase:
                    Target = FindTarget();
                    break;

                // ---------------------------
                case GhostState.Frightened:
                    // Targets a random neighbour
                    // The Move() function will prevent to change direction,
                    // so we don't need to take care of that here!
                    Target = Game.Level
                        .AvailablePositions(Position)
                        //.AvailableNeighbours(Position)
                        //.Select(x => x.position) // <Vector2Int, Action>
                        //.DeterministicRandom(RandomSource, Game.Turn); // Deterministic randomness
                        .Random();
                    break;

                // ---------------------------
                case GhostState.Eaten:
                    Target = GhostHousePosition;
                    break;
            }




            // Speed
            Agent.Speed = State switch
            {
                GhostState.Scatter    => DefaultSpeed,
                GhostState.Chase      => DefaultSpeed,
                GhostState.Frightened => FrigthenedSpeed,
                GhostState.Eaten      => EatenSpeed,

                _ => DefaultSpeed
            };

            

        }

        public override Action Move()
        {
            // Finds the target, based on the current state
            UpdateStateAndTarget();

            // FIX ME: i know where the problem is!
            //  When the ghost walks through a bent corridor, it goes from going DOWN to going LEFT (for example)
            //  If the speed is 0.5, then it stay in the same cell for multiple frames.
            //  At that point, it is in the same cell as before, but it can go back UP again, because the
            //  current new direction was LEFT.
            //  Technically, the ghost did not "really" change direction, because it continue
            //  following the corridor.
            //  If there are only TWO options to move, you cannot go back.

            if (Agent.Position != Agent.OldPosition)
                LastDifferentPosition = Agent.OldPosition;

            // Gets the action that bring you closer to the target
            Action nextAction = Game.Level
                .AvailableActions(Position)
                // Cannot reverse direction!
                // So we select only the actions that are NOT reverse of the current direction
                .Where(action => !action.IsReverseOf(CurrentDirection))
                // Prevents to go back to the last different position
                .Where(action => Position + action.ToV2I() != LastDifferentPosition)
                .DefaultIfEmpty(Action.None)
                //.MinBy(action => Game.Level.EuclideanLoopDistance(Agent.Position + action.ToV2I(), Target));
                .MinBy(action => Vector2Int.Distance(Position + action.ToV2I(), Target));

            CurrentDirection = nextAction;
            return nextAction;
        }

        public abstract Vector2Int FindTarget();


        public override void Draw ()
        {
            Vector2 offset = new Vector2(0.5f, 0.5f);
            DebugDraw.Arrow(Agent.Position + offset, Target + offset, Agent.Color, Game.Delay);

            DebugDraw.Rectangle(Target + offset, 1f, 1f, Agent.Color, Game.Delay);
        }



        /*

        // TODO: one switch for target
        // TODO: if statements for eaten/frigthened after that

        switch (State)
        {
            // ---------------------------
            case GhostState.Scatter:
                Target = ScatterTarget;

                // [Scatter -> Eaten]
                if (IsEaten())
                {
                    // TODO: change target here or wait for next frame?
                    // TODO: change colour
                    State = GhostState.Eaten;
                    break;
                }

                // [Scatter -> Frightened]
                if (PlayerCanEatMe())
                {
                    CurrentDirection = CurrentDirection.Reverse();
                    // TODO: change colour
                    State = GhostState.Frightened;
                    break;
                }


                // [Scatter -> Chase]
                if (Timers[Game.Turn] == GhostState.Chase)
                {
                    CurrentDirection = CurrentDirection.Reverse();
                    State = GhostState.Chase;
                    break;
                }

                break;

            // ---------------------------
            case GhostState.Chase:
                Target = FindTarget();


                // [Frightened -> Eaten]
                if (IsEaten())
                {
                    // TODO: change target here or wait for next frame?
                    // TODO: change colour
                    State = GhostState.Eaten;
                    break;
                }

                //Debug.Log(Timers[Game.Turn]);
                // [Chase -> Frightened]
                // When becomes frightened, the ghost is forced to reverse direction
                // TODO: frightened and eaten?
                if (PlayerCanEatMe())
                {
                    CurrentDirection = CurrentDirection.Reverse();
                    // TODO: change colour
                    State = GhostState.Frightened;
                    break;
                }


                // [Chase -> Scatter]
                if (Timers[Game.Turn] == GhostState.Scatter)
                {
                    CurrentDirection = CurrentDirection.Reverse();
                    State = GhostState.Scatter;
                    break;
                }

                break;

            // ---------------------------
            case GhostState.Frightened:
                // Targets a random neighbour
                // The Move() function will prevent to change direction,
                // so we don't need to take care of that here!
                Target = Game.Level
                    .AvailableNeighbours(Agent.Position)
                    .Select(x => x.Item1) // <Vector2Int, Action>
                    .Random();

                // In the original pacman, frightened ghosts
                // are walking at 50% speed.
                // This cannot be done in the current engine,
                // since every agent moves 1 tile at a time.
                // This means that you can virtually never eat ghosts,
                // as they run as fast as you.
                // Even though ghosts are technically moving randomly,
                // they cannot reverse direction.
                // As a result, they can only get closer to you at
                // intersections, which are too rare.
                // To compensate for this, we set the target to the current tile,
                // so that sometimes the ghost sometimes doesn't move!
                if (Game.Turn % 2 == 0)
                    CurrentDirection = Action.None;
                    //Target = Agent.Position;

                // [Frightened -> Eaten]
                if (IsEaten())
                {
                    // TODO: change target here or wait for next frame?
                    // TODO: change colour
                    State = GhostState.Eaten;
                    break;
                }

                // [Frightened -> Chase]
                if (!PlayerCanEatMe())
                {
                    // TODO: change colour
                    //State = GhostState.Chase;
                    State = Timers[Game.Turn];
                    break;
                }
                break;

            // ---------------------------
            case GhostState.Eaten:
                Target = GhostHousePosition;

                // [Dead -> Chase]
                // Ghost house reached!
                if (Agent.Position == GhostHousePosition)
                {
                    CurrentDirection = Action.None; // Reset direction


                    Agent.Eaten = false; // Resurrect

                    // TODO: change colour
                    //State = GhostState.Chase;
                    State = Timers[Game.Turn];
                    break;
                }
                break;

        }
        */

    }
}