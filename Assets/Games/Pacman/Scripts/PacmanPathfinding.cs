using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AlanZucconi.AI.PF;

namespace AlanZucconi.Pacman
{
    public static class PacmanPathfindingExtension
    {
        public static bool IsReachable(this PacmanGame game, Vector2Int start, Vector2Int end)
        {
            PacmanPathfinding pf = new PacmanPathfinding(game);
            //List<Vector2Int> path = pf.BreadthFirstSearch(start, end);
            //List<(Vector2Int, Edge<Action>)> path = pf.Dijkstra(start, end);
            var path = pf.Dijkstra(start, end);
            return path != null; // Path is null -> no path
        }


        // From the agent
        //public static bool IsReachable(this PacmanGame game, Vector2Int end)
        public static bool IsReachable(this Agent agent, Vector2Int end)
            => agent.Game.IsReachable(agent, end);
        

        // Moves the agent towards the target, using pathfinding
        // Returns Action.None if there is no path, or if you are already on the target
        public static Action MoveTowards(this PacmanGame game, Vector2Int start, Vector2Int end)
        {
            PacmanPathfinding pf = new PacmanPathfinding(game);
            //List<Vector2Int> path = pf.BreadthFirstSearch(start, end);
            //List<(Vector2Int, Edge<Action>)> path = pf.Dijkstra(start, end);
            var path = pf.Dijkstra(start, end);
            // No path available
            if (path == null)
                return Action.None;

            // Already on the target
            if (path.Count == 1)
                return Action.None;

            return path[0].edge; // Edge<Action>

            // !! This doesn't work if you're going through a world boundary !!
            // Next action to advance on the path
            //Vector2Int direction = path[1] - path[0];
            //if (direction.x > 0) return Action.Right;
            //if (direction.x < 0) return Action.Left;
            //if (direction.y > 0) return Action.Up;
            //if (direction.y < 0) return Action.Down;

            //return Action.None;
        }
        //public static Action MoveTowards(this PacmanGame game, Vector2Int end)
        public static Action MoveTowards(this Agent agent, Vector2Int end)
            => agent.Game.MoveTowards(agent, end);

        //public static void MoveTowards(this PacmanGame game, int x, int y)
        //    => MoveTowards(game, new Vector2Int(x, y));



        // Distance between two points
        // returns int.MaxValue is target is unreachable
        // returns 0 is start == end
        public static int DistanceFrom (this PacmanGame game, Vector2Int start, Vector2Int end)
        {
            PacmanPathfinding pf = new PacmanPathfinding(game);
            //List<Vector2Int> path = pf.BreadthFirstSearch(start, end);
            List<(Vector2Int, Edge<Action>)> path = pf.Dijkstra(start, end);
            if (path == null)
                return int.MaxValue;

            return path.Count - 1;
        }

        //public static int DistanceFrom(this PacmanGame game, Vector2Int end)
        public static int DistanceFrom(this Agent agent, Vector2Int end)
            => agent.Game.DistanceFrom(agent, end);
    }
    
    // Helper class
    // This class uses Actions are edges.
    // This is necessary, because with the looping level, the coordinate values are not enough
    // to decide if we have to go left/right/up/down.
    public struct PacmanPathfinding : IPathfindingCost<Vector2Int, Edge<Action>>
    {
        public PacmanGame Game;
        public PacmanPathfinding(PacmanGame game)
        {
            Game = game;
        }

        // Given a position on the board, which neighbouring cells
        // can the snake move to?
        public IEnumerable<(Vector2Int, Edge<Action>)> Outgoing (Vector2Int position)
            => Game.Level.AvailableNeighbours(position);
    }
    /*
    public struct PacmanPathfinding : IPathfinding<Vector2Int>
    {
        public PacmanGame Game;
        public PacmanPathfinding(PacmanGame game)
        {
            Game = game;
        }

        // Given a position on the board, which neighbouring cells
        // can the snake move to?
        public IEnumerable<Vector2Int> Outgoing(Vector2Int position)
        {
            return Game.Level.AvailableNeighbours(position);
        }
    }
    */
}