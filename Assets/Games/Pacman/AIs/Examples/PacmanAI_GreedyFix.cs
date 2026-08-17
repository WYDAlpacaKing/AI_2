using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.Pacman
{
    [CreateAssetMenu
    (
        fileName = "PacmanAI_GreedyFix",
        menuName = "Pacman/Examples/GreedyFix"
    )]
    /* This agent goes to the closest edible item (pellet or power pellet).
     * Because this uses agreedy approach, it does not guarantee that all food
     * is eaten in the shortest amount of time.
     * Ghosts are ignored.
     * 
     * The vanilla approach used in PacmanAI_Greedy can get stuck
     * in non-monotonic scenarios, due to the greedy nature of this approach.
     * 
     * The solution is to pick a target, and to only re-evaluate it once that has been reached.
     */
    public class PacmanAI_GreedyFix : PacmanAI
    {

        private Vector2Int? Target = null;

        // Resets the target
        public override void Initialise()
        {
            base.Initialise();

            Target = null;
        }

        public override Action Move()
        {
            // Finds the closest edible item
            // (if one has not been choosen already)
            if (Target == null ||           // No target
                Target == Agent.Position)   // Target reached
                Target = Game.Level
                    .Edibles()
                    .MinBy(position => Vector2Int.Distance(Position, position));
                    //.MinBy(position => Pacman.DistanceFrom(position));
                    //.OrderBy(position => Game.Level.EuclideanLoopDistance(Agent, position))

            // Move towards the target
            return Pacman.MoveTowards(Target.Value);
        }
    }
}