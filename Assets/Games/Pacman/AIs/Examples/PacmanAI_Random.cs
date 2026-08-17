using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAI_Random",
        menuName = "Pacman/Examples/Random"
    )]
    public class PacmanAI_Random : PacmanAI
    {
        public override Action Move()
        {
            // List of available actions
            Action[] actions = Game.Level
                .AvailableActions(Position)
                .ToArrayOrNull();

            // This should never be the case on the standard level
            // but is here for safety!
            if (actions == null)
                return Action.None;

            // Picks a random action
            return actions[Random.Range(0, actions.Length)];
        }
    }
}