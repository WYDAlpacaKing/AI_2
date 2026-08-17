using System;
using System.Linq;
using UnityEngine;
using AlanZucconi.Pacman;

namespace Pacman.ywang146
{
    [CreateAssetMenu(
        fileName = "PacmanAI_ywang146_v2",
        menuName = "Pacman/2025-26/PacmanAI_ywang146_v2"
    )]
    /// <summary>
    /// VERSION 2 - FINITE STATE MACHINE (EatFood / Flee / Hunt)
    ///
    /// Introduces ghost awareness on top of V1:
    ///   EatFood : default state. Same greedy behaviour as V1, but the target is
    ///             now LOCKED until it is reached (fixes V1's oscillation) and
    ///             distances use the real path distance instead of euclidean.
    ///   Flee    : a ghost that can kill us is within FleeDistance: pick the
    ///             direction that maximises the distance from it.
    ///   Hunt    : powered up and an eatable ghost is within HuntDistance:
    ///             chase the closest eatable ghost for the 200/400/800/1600 bonus.
    ///
    /// State diagram:
    ///         (no threat)                      (ghost far / not eatable)
    ///   EatFood ----------------> Flee <-----------------+
    ///      |                     ^   ^                   |
    ///      |  (eatable ghost      |   | (no threat)      |
    ///      |   in range)          |   |                   |
    ///      +------------------> Hunt --------------------+
    ///      (ghost not eatable anymore, or out of range)
    /// </summary>
    public class PacmanAI_ywang146_v2 : PacmanAI
    {
        public enum AiState
        {
            EatFood,
            Flee,
            Hunt
        }

        public AiState State = AiState.EatFood;

        [Header("Parameters")]
        [Tooltip("Path distance at which a dangerous ghost triggers the Flee state")]
        public float FleeDistance = 12f;

        [Tooltip("Path distance at which an eatable ghost triggers the Hunt state")]
        public float HuntDistance = 30f;

        // Target of the EatFood state (locked to avoid oscillation)
        private Vector2Int? target = null;

        public override void Initialise()
        {
            base.Initialise();

            State = AiState.EatFood;
            target = null;
        }

        public override Action Move()
        {
            // ---------- State transitions (evaluated every frame) ----------
            Ghost dangerous = NearestGhost(ghost => ghost.CanEat(Pacman));
            Ghost eatable = NearestGhost(ghost => Pacman.CanEat(ghost));

            if (eatable != null && Dist(eatable.PositionInteger) <= HuntDistance)
                State = AiState.Hunt;
            else if (dangerous != null && Dist(dangerous.PositionInteger) <= FleeDistance)
                State = AiState.Flee;
            else
                State = AiState.EatFood;

            // ---------- State behaviour ----------
            switch (State)
            {
                case AiState.Hunt:
                    return Pacman.MoveTowards(eatable.PositionInteger);

                case AiState.Flee:
                    return FleeFrom(dangerous);

                default:
                    return EatFood();
            }
        }

        /// <summary>Greedy food eating, with target locking to prevent oscillation.</summary>
        private Action EatFood()
        {
            // Re-picks the target only when none is set, or it has been reached
            if (target == null || target == Agent.Position)
            {
                target = Game.Level
                    .Edibles()
                    .OrderBy(position => Dist(position))
                    .FirstOrDefault();

                // No food left -> level is basically cleared
                if (target == null)
                    return Action.None;
            }

            Action action = Pacman.MoveTowards(target.Value);

            // Target reached (or unreachable): force a re-pick next frame
            if (action == Action.None)
                target = null;

            return action;
        }

        /// <summary>Runs away from the ghost: maximises the distance of the next tile.</summary>
        private Action FleeFrom(Ghost ghost)
        {
            return Game.Level
                .AvailableActions(Position)
                .OrderByDescending(action =>
                    Dist(Game.Level.Loop(Position + action.ToV2I()), ghost.PositionInteger))
                .FirstOrDefault();
        }

        /// <summary>Closest ghost matching the predicate (null if none).</summary>
        private Ghost NearestGhost(Func<Ghost, bool> predicate)
        {
            return Game.Ghosts
                .Where(predicate)
                .OrderBy(ghost => Dist(ghost.PositionInteger))
                .FirstOrDefault();
        }

        // ---------- Distance helpers ----------
        // Prefers the precomputed APSP lookup (O(1), simulator built-in);
        // falls back to the regular Dijkstra pathfinding.
        private int Dist(Vector2Int a, Vector2Int b)
        {
            if (PacmanAPSP.S != null && PacmanAPSP.S.Data != null)
                return PacmanAPSP.S.Data.DistanceFrom(a, b);

            return Game.DistanceFrom(a, b);
        }

        private int Dist(Vector2Int b) => Dist(Position, b);
    }
}
