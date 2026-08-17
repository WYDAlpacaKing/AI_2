using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "GhostAI_Pinky",
        menuName = "Pacman/Ghosts/Pinky"
    )]
    public class GhostAI_Pinky : GhostAI
    {
        [Header("Blinky AI")]
        public int LookAhead = 4;

        // Pinky targets 4 tiles in front of the player
        public override Vector2Int FindTarget()
            => Game.Pacman.Position +
               Game.Pacman.Action.ToV2I() * LookAhead;
            

        public override void Draw()
        {
            base.Draw();

            // Line from player to prediction
            if (State == GhostState.Chase)
            {
                Vector2 offset = new Vector2(0.5f, 0.5f);
                DebugDraw.DashedLine(Game.Pacman.Position + offset, Target + offset, Agent.Color, 0.5f, Game.Delay);
            }
        }
    }
}