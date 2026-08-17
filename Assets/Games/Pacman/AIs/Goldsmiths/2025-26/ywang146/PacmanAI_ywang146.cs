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
    /// Since v3.1 two survival layers were added on top of the utility:
    ///   - Ghost prediction: the safety heuristic scores every candidate tile
    ///     against where each dangerous ghost will be NEXT TURN (a pursuit
    ///     model that mirrors the simulator's own GhostAI.Move()), instead of
    ///     its current position. This fixes the classic "walks into a ghost
    ///     that was 3 tiles away" failure of one-step greedy AIs.
    ///   - Panic rule: a hard safety filter that refuses to step onto any tile
    ///     a ghost is predicted to reach next turn (certain death). When every
    ///     option is lethal it flees towards the least dangerous tile.
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
        // NOTE: Weights is INHERITED from PacmanAIEvo — never redeclare it here,
        // Unity serialization does not support the same field in class + parent.

        // Defaults applied when the array is empty (e.g. freshly created assets,
        // or simulator copies created before the first SetWeights call).
        private static readonly float[] DefaultWeights = new float[]
        {
            1.0f,  // w0 FoodProximity       (positive: eat)
            3.0f,  // w1 GhostSafety         (positive: survive)
            0.8f,  // w2 PowerProximity      (positive: plan ahead)
            1.2f,  // w3 HuntProximity       (positive: score via ghosts)
            0.5f,  // w4 DeadEndAvoidance    (positive: stay mobile)
            0.4f   // w5 DirectionPersistence(positive: stay the course)
        };

        private void OnEnable()
        {
            if (Weights == null || Weights.Length == 0)
                Weights = (float[]) DefaultWeights.Clone();
        }

        public override Action Move()
        {
            var neighbours = Game.Level.AvailableNeighbours(Position).ToArray();
            if (neighbours.Length == 0)
                return Action.None;

            // Predicted positions of every ghost that can kill us (pursuit
            // model, see PredictedPosition). Frightened/eaten ghosts are not
            // threats here — they become prey instead (see HuntProximity).
            Vector2Int[] threats = Game.Ghosts
                .Where(ghost => !ghost.IsEaten() && ghost.CanEat(Pacman))
                .Select(ghost => PredictedPosition(ghost))
                .ToArray();

            // ---- Panic rule (hard safety filter) ---------------------------
            // Never step onto a tile that a ghost is predicted to reach next
            // turn: that would be certain death. Only when EVERY option is
            // lethal do we flee towards the least dangerous one.
            var safe = neighbours
                .Where(neighbour =>
                    threats.All(threat => Dist(neighbour.position, threat) > 1))
                .ToArray();

            if (safe.Length > 0)
                return safe
                    .OrderByDescending(neighbour => Utility(neighbour.position, neighbour.action, threats))
                    .First()
                    .action;

            // All tiles are lethal: run away from the closest predicted ghost.
            return neighbours
                .OrderByDescending(neighbour =>
                    threats.Min(threat => Dist(neighbour.position, threat)))
                .First()
                .action;
        }

        // ---------- Utility ----------

        private float Utility(Vector2Int next, Action action, Vector2Int[] threats)
        {
            float utility = 0f;

            utility += W(0) * FoodProximity(next);
            utility += W(1) * GhostSafety(next, threats);
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

        // How far the next tile is from the nearest ghost that can kill us,
        // measured against each ghost's PREDICTED position next turn (one-step
        // lookahead) instead of its current one. This is what stops the AI
        // from walking into a ghost that is still 2-3 tiles away.
        // Returns 1 (perfectly safe) when no dangerous ghost exists.
        private float GhostSafety(Vector2Int next, Vector2Int[] threats)
        {
            int best = int.MaxValue;
            foreach (Vector2Int threat in threats)
                best = Mathf.Min(best, Dist(next, threat));

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

        // ---------- Ghost prediction ----------

        // Predicts where a (dangerous) ghost will be next turn by mirroring the
        // simulator's own GhostAI.Move():
        //   - ghosts greedily pick the available action that minimises the
        //     Euclidean distance to their target (same metric as the engine);
        //   - ghosts can never reverse direction (unless their state changed);
        //   - frightened ghosts pick a RANDOM target, but they are not a
        //     threat, so the caller never asks us to predict them.
        // Every dangerous ghost is modelled as a pursuer (target = our current
        // position): this is exact for Blinky, and conservative for Pinky
        // (predicts ahead), Inky (mirrors Blinky) and Clyde (retreats), as well
        // as during the scatter phase (ghosts then head to their corners).
        // The ghost's own state (Agent.Action) is read-only game state.
        private Vector2Int PredictedPosition(Ghost ghost)
        {
            Vector2Int position = ghost.PositionInteger;

            Vector2Int best = position;
            float bestDistance = float.MaxValue;
            bool found = false;

            foreach (var neighbour in Game.Level.AvailableNeighbours(position))
            {
                // Ghosts cannot reverse direction
                if (neighbour.action.Content.IsReverseOf(ghost.Action))
                    continue;

                // Same metric used by GhostAI.Move()
                float distance = Vector2Int.Distance(neighbour.position, Position);
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = neighbour.position;
                    found = true;
                }
            }

            // Cornered ghost: it cannot move, so it stays in place
            return found ? best : position;
        }

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
