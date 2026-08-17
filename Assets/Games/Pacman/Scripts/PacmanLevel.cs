using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AlanZucconi.AI.PF;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AlanZucconi.Pacman
{
    public enum Item
    {
        Void,           // unreachable/out of bounds (behaves like wall)
        Wall,
        Ground,
        Pellet,
        PowerPellet
    }

    public static class ItemExtension
    {
        public static bool IsEdible(this Item item) =>
            item == Item.Pellet      ||
            item == Item.PowerPellet ;
    }

    public class PacmanLevel : MonoBehaviour
    {
        public PacmanGame Game;

        [Header("Level")]
        [Monospaced(31, 31)]
        //[TextArea(5, 10)]
        public string Text;
        public Item[,] Data;

        [ReadOnly]
        // Pellet + Power Pellets
        // The level is cleared when this gets to zero
        public int ItemsLeft;
// TODO: create a hashset for the items, so it's quicker to access them?

        [Header("Rendering")]
        //public bool Rendering = true;
        //[Space]
        [EditorOnly]
        public Tilemap Tilemap;
        public Tile VoidTile;
        public RuleTile WallTile;
        public Tile GroundTile;
        public Tile PelletTile;
        public Tile PowerPelletTile;
        
        //[Space]
        //public Tile GhostTile;
        //public Tile PlayerTile;


        // TODO: void tile for empty (but unwalkable)

        /*
        void Awake()
        {
            // Allows the tile to change colour
            GhostTile.flags = TileFlags.None;
        }
        */


        #region LevelAccess
        public int Width
        {
            get
            {
                return Data.GetLength(0);
            }
        }
        public int Height
        {
            get
            {
                return Data.GetLength(1);
            }
        }

        // Easy access
        public Item this[Vector2Int position]
        {
            get
            {
                position = Loop(position);
                return Data[position.x, position.y];
            }

            set
            {
                position = Loop(position);

                // Item eaten?
                // (= edible item replaced by non-edible item)
                Item item = Data[position.x, position.y];
                if (item.IsEdible() && !value.IsEdible())
                    ItemsLeft--;

                Data[position.x, position.y] = value;
            }
        }

        public Item this[int x, int y]
        {
            get => this[new Vector2Int(x, y)];
            set => this[new Vector2Int(x, y)] = value;
        }
        #endregion

        #region Looping

        /** The levels in pacman can loop.
         * This means that when a coordinate goes outside the boundaries,
         * it re-enters from the other side.
         * 
         * This method updates a position to keep track of this.
         */
        public Vector2Int Loop(Vector2Int position)
        {
            position.x = ((position.x % Width) + Width) % Width;
            position.y = ((position.y % Height) + Height) % Height;
            return position;
        }
        
        public (int x, int y) Loop(int x, int y)
        {
            Vector2Int position = Loop(new Vector2Int(x, y));
            return (position.x, position.y);
        }
        
        public Vector2 Loop(Vector2 position)
        {
            position.x = ((position.x % Width) + Width) % Width;
            position.y = ((position.y % Height) + Height) % Height;
            return position;
        }


        /** Returns the Euclidean distance between two points,
         * taking into account the fact that the level is looping around.
         * 
         * This method does not calculate the actual distance an agent has to travel,
         * which depends on the walls and requires pathfinding.
         */
        public float EuclideanLoopDistance (Vector2Int a, Vector2Int b)
        {
            a = Loop(a);
            b = Loop(b);

            int dx = Mathf.Abs(a.x - b.x);
            dx = Mathf.Min(dx, Width - dx);

            int dy = Mathf.Abs(a.y - b.y);
            dy = Mathf.Min(dy, Height - dy);

            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /** Returns the Manhattan distance between two points,
         * taking into account the fact that the level is looping around.
         */
        public float ManhatanLoopDistance(Vector2Int a, Vector2Int b)
        {
            a = Loop(a);
            b = Loop(b);

            int dx = Mathf.Abs(a.x - b.x);
            dx = Mathf.Min(dx, Width - dx);

            int dy = Mathf.Abs(a.y - b.y);
            dy = Mathf.Min(dy, Height - dy);

            return dx + dy;
        }
        #endregion

        /*
        public (int, int) Loop(int x, int y)
        {
            x = ((x % Width ) + Width ) % Width ;
            y = ((y % Height) + Height) % Height;
            return (x, y);
        }

        public Vector2Int Loop(Vector2Int position)
        {
            var (x, y) = Loop(position.x, position.y);
            return new Vector2Int(x, y);
        }
        */

        #region Level

        /** Returns true if the position is free.
         * Free means that an agent can move there.
         * Pellets and ghosts are all considered free,
         * as the agents can overlap.
         * 
         * Loops coordinates.
         */
        public bool IsFree(Vector2Int position)
        {
            position = Loop(position);

            return Data[position.x, position.y] switch
            {
                // Obstacles
                Item.Void => false,
                Item.Wall => false,
                // Walkable
                _ => true
            };

            // Empty?
            //return Data[x, y] != Item.Wall;
        }
        /*
        public bool IsFree(int x, int y)
        {
            (x, y) = Loop(x, y);

            // Out of bounds
            //if (x < 0 || x > Width - 1)
            //    return false;
            //if (y < 0 || y > Height - 1)
            //    return false;


            return Data[x, y] switch
            {
                // Obstacles
                Item.Void => false,
                Item.Wall => false,
                // Walkable
                _ => true
            };

            // Empty?
            //return Data[x, y] != Item.Wall;
        }
        */

        /** Returns true if the position is an obstacle.
         * 
         * Loops coordinates;
         */
        public bool IsObstacle(Vector2Int position) => ! IsFree(position);


        public bool IsFree(int x, int y) => IsFree(new Vector2Int(x, y));
        public bool IsObstacle(int x, int y) => !IsFree(x, y);

        
        
        

        public void Build()
        {
            // https://steamcommunity.com/sharedfiles/filedetails/?id=593226813
            /*
            wwwwwwwwwwwwwwwwwwwwwwwwwwww
            w............ww............w
            w.wwww.wwwww.ww.wwww.wwwww.w
            wowwww.wwwww.ww.wwww.wwwwwow
            w.wwww.wwwww.ww.wwww.wwwww.w
            w..........................w
            w.wwww.ww.wwwwwwww.ww.wwww.w
            w.wwww.ww.wwwwwwww.ww.wwww.w
            w......ww....ww....ww......w
            wwwwww.wwwww ww wwwww.wwwwww
            -----w.wwwww ww wwwww.w-----
            -----w.ww          ww.w-----
            -----w.ww wwwHHwww ww.w-----
            wwwwww.ww w      w ww.wwwwww
                  .   w      w   .      
            wwwwww.ww w      w ww.wwwwww
            -----w.ww wwwwwwww ww.w-----
            -----w.ww          ww.w-----
            -----w.ww wwwwwwww ww.w-----
            wwwwww.ww wwwwwwww ww.wwwwww
            w............ww............w
            w.wwww.wwwww.ww.wwwww.wwww.w
            w.wwww.wwwww.ww.wwwww.wwww.w
            wo..ww.......  .......ww..ow
            www.ww.ww.wwwwwwww.ww.ww.www
            www.ww.ww.wwwwwwww.ww.ww.www
            w......ww....ww....ww......w
            w.wwwwwwwwww.ww.wwwwwwwwww.w
            w.wwwwwwwwww.ww.wwwwwwwwww.w
            w..........................w
            wwwwwwwwwwwwwwwwwwwwwwwwwwww
            */

            // W = wall
            // . = pellet
            // O = power pellet
            // H = ghost house walls
            //   = groud (walkable)
            // _ = void (unreachable)

            ItemsLeft = 0;

            string[] lines = Text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            Data = new Item[lines[0].Length, lines.Length];
            for (int r = 0; r < lines.Length; r++)
                for (int c = 0; c < lines[r].Length; c++)
                {
                    //Data[c, (lines.Length - 1) - r] = CharToItem(lines[r][c]);
                    Item item = CharToItem(lines[r][c]);
                    Data[c, (lines.Length - 1) - r] = item;

                    // Counts the pellets and power pellets
                    if (item.IsEdible())
                        ItemsLeft++;
                }
        }

        /** Return true if the level has been cleared.
         * This means that all pellets and power pellets have been collected. */
        public bool IsCleared() => ItemsLeft <= 0;
        #endregion







        #region Draw

        public enum Layer
        {
            None,
            Walls = 1,
            Items = 2, // (CURRENTLY UNUSED)
            Ghost = 3,
            Pacman = 4
        }

        //private const int TilemapLayer_Walls = 0; // The tilemap layer used for the walls
        //private const int TilemapLayer_Items    = 1; // The tilemap layer used for the items (CURRENTLY UNUSED)
        //private const int TilemapLayer_Agents = 2; // The tilemap layer used for the agents

        public void Draw()
        {
            Tilemap.ClearAllTiles();

            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    Tilemap.SetTile
                    (
                        //new Vector3Int(x, y, TilemapLayer_Walls),
                        new Vector3Int(x, y, (int) Layer.Walls),
                        ItemToTile(Data[x, y])
                    );
        }


        public void DrawAgents()
        {
            foreach (Agent agent in Game.Agents)
                agent.Draw();
        }

        private Item CharToItem(char c) =>
            c switch
            {
                '-' => Item.Void,           // Obstacle
                'w' => Item.Wall,           // Obstacle
                ' ' => Item.Ground,
                '.' => Item.Pellet,
                'o' => Item.PowerPellet,

                // Anything else is a wall
                _ => Item.Wall
            };
        private TileBase ItemToTile(Item i) =>
            i switch
            {
                Item.Void        => VoidTile,
                Item.Wall        => WallTile,
                Item.Ground      => GroundTile,
                Item.Pellet      => PelletTile,
                Item.PowerPellet => PowerPelletTile,
                
                // Anything else is a wall
                _ => WallTile
            };
        #endregion


        #region Iterators
        // This method does not LOOP through the coordinates,
        //  so it's a bit faster
        private Item GetItemAt(Vector2Int position)
            => Data[position.x, position.y];

        /** Iterates over all (x,y) coordinates in the level.
         * This includes cells that are walls, empty, etc.
         */
        public IEnumerable<Vector2Int> AllPositions()
        {
            int w = Width;
            int h = Height;
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                    yield return new Vector2Int(x, y);
        }

        /** Iterates over all free coordinates.
         * Free coordinates are the ones you can walk into.
         * This includes pellets, power pellets, and even positions
         * where ghosts are (as you can walk into there).
         */
        public IEnumerable<Vector2Int> FreePositions()
            => AllPositions()
                .Where(position => IsFree(position));

        /** Iterates over all edible items in the level.
         * This include pellets and power pellets
         * (frightened ghosts are not included as they are not items).
         */
        public IEnumerable<Vector2Int> Edibles()
            => AllPositions()
                .Where(position => GetItemAt(position).IsEdible());
            //.Where(position => this[position].IsEdible());

        /** Iterates over all pellets.
         * Power pellets are NOT included.
         */
        public IEnumerable<Vector2Int> Pellets()
            => AllPositions()
                .Where(position => GetItemAt(position) == Item.Pellet);
        //.Where(position => this[position] == Item.Pellet);

        /** Iterates over all power pellets.
         * Non-power pellets are NOT included.
         */
        public IEnumerable<Vector2Int> PowerPellets()
            => AllPositions()
                .Where(position => GetItemAt(position) == Item.PowerPellet);
        //.Where(position => this[position] == Item.PowerPellet);
        #endregion


        #region Pathfinding
        // If no actions are available, the list if empty
        // It does NOT return Action.None!
        public IEnumerable<Action> AvailableActions(Vector2Int position, bool isNoneAllowed = false)
        {
            if (IsFree(position + Vector2Int.up))
                yield return Action.Up;

            if (IsFree(position + Vector2Int.left))
                yield return Action.Left;

            if (IsFree(position + Vector2Int.down))
                yield return Action.Down;

            if (IsFree(position + Vector2Int.right))
                yield return Action.Right;

            // Stays where you are
            if (isNoneAllowed)
                if (IsFree(position))
                    yield return Action.None;

            // No available cells
            //yield break;
        }


        // Given a position,
        // iterates over the nearby indices the agents can travel to
        //
        // Loops positions
        public IEnumerable<(Vector2Int position, Edge<Action> action)> AvailableNeighbours(Vector2Int position)
        //    => AvailableActions(position)
        //        .Select(action => (Loop(position + action.ToV2I()), action));
        {
            // Loops through all avaialble actions
            foreach (Action action in AvailableActions(position))
                yield return (Loop(position + action.ToV2I()), action);
        }
        /*
        public IEnumerable<Vector2Int> AvailableNeighbours(Vector2Int position)
        {
            
            if (IsFree(position + Vector2Int.up))
                yield return Loop(position + Vector2Int.up);

            if (IsFree(position + Vector2Int.left))
                yield return Loop(position + Vector2Int.left);

            if (IsFree(position + Vector2Int.down))
                yield return Loop(position + Vector2Int.down);

            if (IsFree(position + Vector2Int.right))
                yield return Loop(position + Vector2Int.right);

            // No available cells
            //yield break;
        }
        */


        // Given a position,
        // iterates over the nearby positions the agents can travel to
        public IEnumerable<Vector2Int> AvailablePositions(Vector2Int position) =>
            AvailableActions(position)
            .Select(action => Loop(position + action.ToV2I()));

        #endregion
    }
}