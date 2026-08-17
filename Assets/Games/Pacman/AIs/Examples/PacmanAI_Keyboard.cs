using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAI_Keyboard",
        menuName = "Pacman/Examples/Keyboard"
    )]
    public class PacmanAI_Keyboard : PacmanAI
    {
        [HideInInspector]
        public Action CurrentDirection = Action.None;

        public override Action Move()
        {
            Action action = CurrentDirection;

            if (Input.GetKey(KeyCode.UpArrow))
                action = Action.Up;

            if (Input.GetKey(KeyCode.DownArrow))
                action = Action.Down;

            if (Input.GetKey(KeyCode.LeftArrow))
                action = Action.Left;

            if (Input.GetKey(KeyCode.RightArrow))
                action = Action.Right;

            CurrentDirection = action;

            return action;
        }
    }
}