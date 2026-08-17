using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAI_Flee",
        menuName = "Pacman/Examples/Flee"
    )]
    /* This agent flees from the closest ghost.
     * It does so, by choosing the movement that maximises the distance.
     * This is a greedy technique which is not guaranteed to find the best escape route.
     */
    public class PacmanAI_Flee : PacmanAI
    {

        public override Action Move()
        {
            // Finds the closest ghost
            Agent closestGhost = Game
                .Ghosts
                .MinBy(ghost => Vector2Int.Distance(Position, ghost));
                //.MinBy(ghost => Game.Level.EuclideanLoopDistance(Position, ghost))
                //.MinBy(ghost => Pacman.DistanceFrom(ghost)

            // Chooses the action that maximises the distance from the closest ghost
            return Game.Level
                .AvailableActions(Agent.Position)
                .DefaultIfEmpty() // Action.None if no action is available
                .MaxBy(action => Vector2Int.Distance(closestGhost, Position + action.ToV2I()));
        }
    }
}