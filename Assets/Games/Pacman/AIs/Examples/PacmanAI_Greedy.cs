using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAI_Greedy",
        menuName = "Pacman/Examples/Greedy"
    )]
    /* This agent goes to the closest edible item (pellet or power pellet).
     * Because this uses agreedy approach, it does not guarantee that all food
     * is eaten in the shortest amount of time.
     * Ghosts are ignored.
     * 
     * ISSUE: due to the greedy nature of this approach,
     *  sometimes the agent might get stuck between two options.
     *  Check PacmanAI_GreedyFix for a solution.
     */
    public class PacmanAI_Greedy : PacmanAI
    {

        public override Action Move()
        {
            // Finds the closest edible item
            Vector2Int itemPosition = Game.Level
                .Edibles()
                .MinBy(position => Vector2Int.Distance(Position, position));
                //.MinBy(position => Game.Level.EuclideanLoopDistance(Agent, position));
                //.OrderBy(position => Pacman.DistanceFrom(position))    
                // Assume there's at least one edible item (if not, the game would end)

            // Move towards that
            return Pacman.MoveTowards(itemPosition);
        }
    }
}