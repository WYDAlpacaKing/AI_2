using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace AlanZucconi.AI.PF
{
    public class Grid2D : IPathfinding<Vector2Int>
    {
        // The directions in which you can move in a 2D grid
        private static Vector2Int[] Directions =
        {
            Vector2Int.up,
            Vector2Int.right,
            Vector2Int.down,
            Vector2Int.left
        };

        // The actual grid data
        // Wall[x, y] = true  ---> cannot walk through
        private bool[,] Wall;

        public Grid2D (Vector2Int size)
        {
            Wall = new bool[size.x, size.y];
            // Automatically initialised to false:
            // all grid is accessible
        }

        

        public void SetWall (Vector2Int position, bool wall = true)
        {
            Wall[position.x, position.y] = wall;
        }
        public bool IsWall(Vector2Int position)
        {
            return Wall[position.x, position.y];
        }

        // Considered wall if out of bounds
        private bool IsFree (Vector2Int position)
        {
            // Out of bounds: it is a wall
            if (position.x < 0 || position.x >= Wall.GetLength(0))
                return false;
            if (position.y < 0 || position.y >= Wall.GetLength(1))
                return false;

            return ! Wall[position.x, position.y];
        }

        public IEnumerable<Vector2Int> Outgoing (Vector2Int position)
        {
            // Out of bounds
            if (position.x < 0 || position.x >= Wall.GetLength(0))
                yield break;
            if (position.y < 0 || position.y >= Wall.GetLength(1))
                yield break;

            foreach (Vector2Int direction in Directions)
                if (IsFree(position + direction))
                    yield return position + direction;
        }
    }

    /*
    // Used to test
    public void Test ()
    {
        Grid2D grid = new Grid2D(new Vector2Int(10, 10));
        grid.SetWall(new Vector2Int(8, 9));
        grid.SetWall(new Vector2Int(8, 8));
        grid.SetWall(new Vector2Int(8, 7));
        grid.SetWall(new Vector2Int(8, 6));
        grid.SetWall(new Vector2Int(8, 5));
        var xxx = grid.BreadthFirstSearch(new Vector2Int(0, 0), new Vector2Int(9, 9));
        foreach (Vector2Int i in xxx)
            Debug.Log("\t" + i);
    }
    */
}