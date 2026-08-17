using System.Linq;
using UnityEngine;
using AlanZucconi.Pacman;
using AlanZucconi.Pacman.Evo;

namespace Pacman.ywang146
{
    [CreateAssetMenu(
        fileName = "PacmanAI_ywang146",
        menuName = "Pacman/2025-26/PacmanAI_ywang146"
    )]
    /// <summary>
    /// VERSION 3 - UTILITY THEORY (final submission AI)
    ///
    /// Instead of hard-coding a rule for every situation, every available
    /// neighbour tile is scored with a weighted sum of heuristics and the best
    /// one is chosen. The weights encode the whole strategy:
    ///
    ///   utility(next, action) =
    ///       w0 * FoodProximity(next)      + // eat: closeness to the nearest edible
    ///       w1 * GhostSafety(next)        + // survive: distance to dangerous ghosts
    ///       w2 * PowerProximity(next)     + // plan: closeness to power pellets
    ///       w3 * HuntProximity(next)      + // score: chase eatable ghosts (powered only)
    ///       w4 * DeadEndAvoidance(next)   + // mobility: avoid dead ends
    ///       w5 * DirectionPersistence(a)    // stability: avoid oscillation / reversals
    ///
    /// All heuristics are normalised to [0,1] via 1/(1+distance); the weights
    /// (inspector-tunable) define the trade-off between them. The hand-tuned
    /// defaults below are a starting point: this AI inherits PacmanAIEvo so the
    /// Evolution System scene ("Pacman Evolution") can optimise the weights
    /// automatically (see report section on evolution).
    ///
    /// Distances prefer the simulator's precomputed APSP lookup (O(1)); when the
    /// PacmanAPSP component is missing it falls back to the looping euclidean
    /// distance, which keeps the AI real-time safe at ~1000 distance queries/frame.
    /// </summary>
    public class PacmanAI_ywang146 : PacmanAIEvo
    {
        // Weight order: 0 food, 1 ghost safety, 2 power pellet,
        //               3 hunt, 4 dead end, 5 direction persistence
        // The Evolution System overwrites this array (keep the size fixed!)
        public float[] Weights = new float[]
        {
            1.0f,  // w0 FoodProximity       (positive: eat)
            3.0f,  // w1 GhostSafety         (positive: survive)
            0.8f,  // w2 PowerProximity      (positive: plan ahead)
            1.2f,  // w3 HuntProximity       (positive: score via ghosts)
            0.5f,  // w4 DeadEndAvoidance    (positive: stay mobile)
            0.4f   // w5 DirectionPersistence(positive: stay the course)
        };

        public override Action Move()
        {
            var neighbours = Game.Level.AvailableNeighbours(Position).ToArray();
            if (neighbours.Length == 0)
                return Action.None;

            return neighbours
                .OrderByDescending(neighbour => Utility(neighbour.position, neighbour.action))
                .First()
                .action;
        }

        // ---------- Utility ----------

        private float Utility(Vector2Int next, Action action)
        {
            float utility = 0f;

            utility += W(0) * FoodProximity(next);
            utility += W(1) * GhostSafety(next);
            utility += W(2) * PowerProximity(next);
            utility += W(3) * HuntProximity(next);
            utility += W(4) * DeadEndAvoidance(next);
            utility += W(5) * DirectionPersistence(action);

            return utility;
        }

        // ---------- Heuristics ----------

        // How close the next tile is to the nearest edible item (0 if none left)
        private float FoodProximity(Vector2Int next)
        {
            int best = int.MaxValue;
            foreach (Vector2Int food in Game.Level.Edibles())
                best = Mathf.Min(best, Dist(next, food));

            return best == int.MaxValue ? 0f : 1f / (1f + best);
        }

        // How far the next tile is from the nearest ghost that can kill us.
        // Returns 1 (perfectly safe) when no dangerous ghost exists.
        private float GhostSafety(Vector2Int next)
        {
            int best = int.MaxValue;
            foreach (Ghost ghost in Game.Ghosts)
            {
                if (ghost.IsEaten() || !ghost.CanEat(Pacman))
                    continue;
                best = Mathf.Min(best, Dist(next, ghost.PositionInteger));
            }

            return best == int.MaxValue ? 1f : 1f / (1f + best);
        }

        // How close the next tile is to the nearest power pellet (0 if none left)
        private float PowerProximity(Vector2Int next)
        {
            int best = int.MaxValue;
            foreach (Vector2Int powerPellet in Game.Level.PowerPellets())
                best = Mathf.Min(best, Dist(next, powerPellet));

            return best == int.MaxValue ? 0f : 1f / (1f + best);
        }

        // How close the next tile is to the nearest eatable ghost.
        // Only active while powered up, and scaled by the remaining power time:
        // chasing a ghost is only worth it while there is still time to catch it.
        private float HuntProximity(Vector2Int next)
        {
            if (!Pacman.IsPoweredUp())
                return 0f;

            float powerRatio = Pacman.PowerPelletTimer / (float) Mathf.Max(1, Pacman.PowerPelletTime);

            int best = int.MaxValue;
            foreach (Ghost ghost in Game.Ghosts)
            {
                if (!Pacman.CanEat(ghost))
                    continue;
                best = Mathf.Min(best, Dist(next, ghost.PositionInteger));
            }

            return best == int.MaxValue ? 0f : (1f / (1f + best)) * powerRatio;
        }

        // How many directions are available from the next tile (normalised):
        // dead ends are penalised because they trap Pacman.
        private float DeadEndAvoidance(Vector2Int next)
            => Game.Level.AvailableActions(next).Count() / 4f;

        // Keeps the current direction when possible (1), tolerates turns (0.4),
        // penalises reversals (-0.5): this is what prevents oscillation.
        private float DirectionPersistence(Action action)
        {
            if (action == Agent.Action)
                return 1f;
            if (action.IsReverseOf(Agent.Action))
                return -0.5f;
            return 0.4f;
        }

        // ---------- Helpers ----------

        private float W(int index)
            => index < Weights.Length ? Weights[index] : 0f;

        // Path distance. APSP lookup is O(1) and simulator-provided;
        // the fallback (looping euclidean) keeps the AI fast when APSP is absent.
        private int Dist(Vector2Int a, Vector2Int b)
        {
            if (PacmanAPSP.S != null && PacmanAPSP.S.Data != null)
                return PacmanAPSP.S.Data.DistanceFrom(a, b);

            return Mathf.RoundToInt(Game.Level.EuclideanLoopDistance(a, b));
        }
    }
}
