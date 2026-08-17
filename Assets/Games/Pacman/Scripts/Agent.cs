using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.Tilemaps;
using static UnityEngine.GraphicsBuffer;

namespace AlanZucconi.Pacman
{
    //[System.Serializable]
    public class Agent : MonoBehaviour
    {
        [HideInInspector]
        public PacmanGame Game;
        public AgentAI AI;

        public Vector2Int InitialPosition;
        [ReadOnly]
        public Vector2Int Position;
        // This is used to detect collisions and void tunneling
        [HideInInspector]
        public Vector2Int OldPosition;

        // Stores the current action, and the previous one
        [ReadOnly]
        public Action Action;
        [HideInInspector]
        public Action OldAction; // Used for Pinky
        
        // Used by Large Language Models, instead of Position
        public Vector2Int PositionInteger
        {
            get => Position;
        }

        [ReadOnly]
        public Vector2 FloatPosition;
        [Range(0f,1f)]
        public float Speed = 1f; // between 0 and 1
        public static readonly Vector2 CENTRE = new Vector2(0.5f, 0.5f);

        [Header("Graphics")]
        public Color Color = Color.yellow;
        public PacmanLevel.Layer Layer;
        public Tile Tile_None;
        public Tile Tile_Up;
        public Tile Tile_Down;
        public Tile Tile_Left;
        public Tile Tile_Right;

        
        [Header("Eating")]
        /*
        // Strength: indicates who can eat who
        // Can only eat players with lower strength
        public int Strength = Strength_Ghost;

        public const int Strength_Pacman = -1;
        public const int Strength_Ghost  = 0;
        public const int Strength_PowerPellet = +1;
        
        // True when this agent has been eaten
        //  - Ghost:    goes back to the ghost house
        //  - Player:   gameover
        */
        [ReadOnly]
        public bool Eaten = false;


        void Awake()
        {
            // Allows the tile to change colour
            Tile_None.flags  =
            Tile_Up.flags    =
            Tile_Down.flags  =
            Tile_Left.flags  =
            Tile_Right.flags =
                TileFlags.None;
        }

        public void Initialise(PacmanGame game)
        {
            // Game
            Game = game;
            Position = OldPosition = InitialPosition;

            FloatPosition = Position + CENTRE;

            Action = OldAction = Action.None;

            Eaten = false;

            // AI
            AI.Game = Game;
            AI.Agent = this;
            AI.Initialise();

            // Custom
            Initialise();
        }

        // Override to initialise/reset
        public virtual void Initialise() { }

        /*
        public void InitialiseAI ()
        {
            AI.Game = Game;
            AI.Agent = this;
            AI.Initialise();
        }*/

        #region Eating


        // Needs to be overriden
        public virtual bool CanEat(Agent agent) => false;

        public bool CanBeEaten(Agent agent) => agent.CanEat(this);

        // This agent gets eaten
        public virtual void OnEatenBy(Agent agent)
        {
            // Eaten already!
            if (Eaten)
                return;

            Eaten = true;
        }

        public virtual void OnEating(Agent agent)
        {

        }

        public bool IsEaten () => Eaten;

        public virtual void Resurrect()
        {
            // Eaten already!
            if (!Eaten)
                return;

            Eaten = false;
        }
        #endregion


        // Updates the position
        // Returns true if the target position is free
        public bool UpdatePosition()
        {
            // Saves the old actions
            OldPosition = Position;
            OldAction   = Action;

            // Pick an action
            Action action = Move();

            // Checks if the new position is free
            Vector2 targetFloatPosition = FloatPosition + (Vector2) action.ToV2I() * Speed;
            Vector2Int targetPosition = Vector2Int.FloorToInt(targetFloatPosition);

            //Vector2Int targetPosition = Position + action.ToV2I();
            if (!Game.Level.IsFree(targetPosition))
            {
                Action = Action.None; // It did not move

                SnapFloatPosition();
                return false;
            }

            // Updates the position
            FloatPosition = Game.Level.Loop(targetFloatPosition); // Vector2
            Position = Game.Level.Loop(targetPosition); // Vector2Int
            Action = action;

            SnapFloatPosition();

            return true;
        }


        public void SnapFloatPosition()
        {
            // Stopped
            if (Action == Action.None)
            {
                FloatPosition = Position + CENTRE;
                return;
            }

            // If direction is just reverse, it preserves the float progress
            // = no snap
            if (Action.IsReverseOf(OldAction))
                return;

            // Changing direction
            // (but no reversal!)
            if (Action != OldAction)
            {
                FloatPosition = Position + CENTRE;
                return;
            }
        }

        public Action Move() => AI.Move();
        //{
        //    Action action = AI.Move();
        //    return action;
        //}


        public virtual void UpdateState() { }


        // This agent has collided with another
        // This happens if they both stepped onto the same time
        // Or if the swapped coordinates
        //
        // When there is a collision between two agents,
        // the following methods are called:
        //  a.CollisionWith(b)
        //  b.CollisionWith(a)
        // To avoid executing the same code twice,
        // each agent updates its OWN status in CollisionWith,
        // not the status of the other agent.


        #region Casts
        // Implic cast at Vector2Int, so we can use the agent itself instead of agent.Position
        public static implicit operator Vector2Int(Agent agent)
            => agent.Position;
        #endregion


        #region Draw
        public virtual void Draw()
        {
            Vector3Int position = new Vector3Int(Position.x, Position.y, (int)Layer);
            Game.Level.Tilemap.SetTile(position, GetTile());
            Game.Level.Tilemap.SetColor(position, GetColor());


            // Smoothely interpolates between tile positions
            Matrix4x4 matrix = Matrix4x4.Translate((FloatPosition - CENTRE) - Position);
            Game.Level.Tilemap.SetTransformMatrix(position, matrix);


            if (Game.DrawAI)
            {
                DebugDraw.Rectangle(Position + CENTRE, 1f, 1f, Color, Game.Delay);
                AI.Draw();
            }
        }
        public virtual Tile GetTile() =>
            Action switch
            {
                Action.None  => Tile_None,
                Action.Up    => Tile_Up,
                Action.Down  => Tile_Down,
                Action.Left  => Tile_Left,
                Action.Right => Tile_Right,
                _            => Tile_None
            };



        // Can be overriden to change the ghost base colour
        public virtual Color GetColor()
        {
            // TODO? fix me with white?
            return Color.xA(Eaten? 0.5f : 1);
        }


        #endregion
    }
}