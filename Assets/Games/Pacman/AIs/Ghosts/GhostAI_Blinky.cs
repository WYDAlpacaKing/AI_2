using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "GhostAI_Blinky",
        menuName = "Pacman/Ghosts/Blinky"
    )]
    public class GhostAI_Blinky : GhostAI
    {
        // Blinky chases the player
        public override Vector2Int FindTarget() => Game.Pacman.Position;
    }
}