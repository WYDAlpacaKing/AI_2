using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    public enum Action
    {
        None = 0, // Not allowed in the original game, but allowed here
        Up,
        Down,
        Left,
        Right
    }

    public static class ActionExtension
    {
        public static Vector2Int ToV2I(this Action direction) =>
            direction switch
            {
                //Action.None  => Vector2Int.zero,
                Action.Up    => Vector2Int.up,
                Action.Down  => Vector2Int.down,
                Action.Left  => Vector2Int.left,
                Action.Right => Vector2Int.right,
                _            => Vector2Int.zero
            };


        // Takes a Vector2Int, and converts it into a direction
        // ASSUMPTION: Direction needs to be aligned to 4 cardinal coordinates,
        // or this might not work correctly/
        public static Action ToAction(this Vector2Int direction)
        {
            if (direction.x > 0) return Action.Right;
            if (direction.x < 0) return Action.Left;
            if (direction.y > 0) return Action.Up;
            if (direction.y < 0) return Action.Down;

            return Action.None;
        }
        /*
            {

                switch (direction)
                {
                    case Action.None: return Vector2Int.zero;
                    case Action.Up: return Vector2Int.up;
                    case Action.Down: return Vector2Int.down;
                    case Action.Left: return Vector2Int.left;
                    case Action.Right: return Vector2Int.right;
                }

                return Vector2Int.zero;
            }*/

        // Reverses the action
        public static Action Reverse(this Action direction) =>
            direction switch
            {
                Action.Up    => Action.Down,
                Action.Down  => Action.Up,
                Action.Left  => Action.Right,
                Action.Right => Action.Left,
                _            => Action.None
            };

        // Check if the current action is the reverse of the parameter
        // This is used because ghosts can never reverse their direction!
        public static bool IsReverseOf(this Action direction, Action nextDirection)
        //    => direction.Reverse() == nextDirection; // What about Action.None?
        {
            if (direction == Action.Up && nextDirection == Action.Down)
                return true;

            if (direction == Action.Down && nextDirection == Action.Up)
                return true;

            if (direction == Action.Left && nextDirection == Action.Right)
                return true;

            if (direction == Action.Right && nextDirection == Action.Left)
                return true;

            return false;
        }
        
    }
}