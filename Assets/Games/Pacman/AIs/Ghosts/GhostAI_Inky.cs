using System.Collections;
using System.Collections.Generic;
using System.Resources;
using UnityEngine;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "GhostAI_Inky",
        menuName = "Pacman/Ghosts/Inky"
    )]
    public class GhostAI_Inky : GhostAI
    {
        // Doubles the distance from Blinky to the player,
        // and dobules that
        public override Vector2Int FindTarget()
        {
            GhostAI_Blinky blinky = Game.BlinkyAI();
            // Blinky is not present in this run!
            // Behaves like Blinky
            if (blinky == null)
                return Game.Pacman.Position;

            return
                Game.Pacman.Position +
                (Game.Pacman.Position - blinky.Position);
        }

        public override void Draw()
        {
            base.Draw();

            // Line from player to target
            if (State == GhostState.Chase)
            {
                Vector2 offset = new Vector2(0.5f, 0.5f);
                DebugDraw.DashedLine(Game.Pacman.Position + offset, Target + offset, Agent.Color, 0.5f, Game.Delay);
            }
        }
    }
}