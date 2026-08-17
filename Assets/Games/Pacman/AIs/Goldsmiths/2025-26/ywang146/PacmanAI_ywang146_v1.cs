using System.Linq;
using UnityEngine;
using AlanZucconi.Pacman;

namespace Pacman.ywang146
{
    [CreateAssetMenu(
        fileName = "PacmanAI_ywang146_v1",
        menuName = "Pacman/2025-26/PacmanAI_ywang146_v1"
    )]
    /// <summary>
    /// VERSION 1 - GREEDY EATER (honest baseline)
    ///
    /// Every frame it re-picks the closest edible item (pellet or power pellet)
    /// and pathfinds to it. Ghosts are completely ignored.
    ///
    /// Known limitations (kept on purpose as development history, see report):
    ///  1. The target is re-evaluated EVERY frame: when two items are equally
    ///     close the agent can oscillate between them, wasting turns.
    ///  2. Distances use the euclidean metric, which ignores walls: the item
    ///     that looks closest can actually be very far in path terms.
    ///  3. No ghost awareness at all: it dies as soon as a ghost intercepts it.
    /// </summary>
    public class PacmanAI_ywang146_v1 : PacmanAI
    {
        public override Action Move()
        {
            // Closest edible item, measured with euclidean distance
            Vector2Int target = Game.Level
                .Edibles()
                .OrderBy(position => Vector2Int.Distance(Position, position))
                .First();

            // Pathfind the first step towards it
            return Pacman.MoveTowards(target);
        }
    }
}
