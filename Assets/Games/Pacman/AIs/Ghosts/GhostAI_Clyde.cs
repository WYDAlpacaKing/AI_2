using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "GhostAI_Clyde",
        menuName = "Pacman/Ghosts/Clyde"
    )]
    public class GhostAI_Clyde : GhostAI
    {
        [Header("Clyde AI")]
        public int Distance = 8;

        public override Vector2Int FindTarget()
        
        {
            float distance = Vector2Int.Distance(Position, Game.Pacman);
            // The player is too far
            if (distance > Distance)
                return ScatterTarget;

            // Chases the player
            return Game.Pacman.Position;

        }

        public override void Draw()
        {
            base.Draw();

            // Detection radius
            if (State == GhostState.Chase || State == GhostState.Scatter)
            {
                Vector2 offset = new Vector2(0.5f, 0.5f);
                DebugDraw.Circle(Agent.Position + offset, Distance, Agent.Color, Game.Delay);
            }
        }
    }
}