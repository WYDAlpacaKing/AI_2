using UnityEngine;

namespace AlanZucconi.Pacman
{
    public class PacmanAPSP : MonoBehaviour
    {
        // Singleton
        public static PacmanAPSP S = null;

        // TODO: add database with a scriptable object for each level
        //  accessed using hash of level string data
        public PacmanGame Game;

        // The scriptable object with the data
        public PacmanAPSPData Data = null;

        

        public void Awake()
        {
            S = this;
        }

        [Button(Editor = true)]
        public void Build()
        {
            // If unset, gets the first one
            if (Game == null)
                Game = FindAnyObjectByType<PacmanGame>(); // finds the first one

            // If unset, creates one on the fly
            if (Data == null)
                Data = ScriptableObject.CreateInstance<PacmanAPSPData>();

            Data.Build(Game);
        }
    }


    public static class PacmanAPSPExtension
    {
        // Move towards
        public static Action MoveTowards_APSP(this PacmanGame game, Vector2Int start, Vector2Int end)
            => PacmanAPSP.S.Data.MoveTowards(start, end);

        public static Action MoveTowards_APSP(this Agent agent, Vector2Int end)
            => PacmanAPSP.S.Data.MoveTowards(agent, end);


        // Distance from
        public static int DistanceFrom_APSP(this PacmanGame game, Vector2Int start, Vector2Int end)
            => PacmanAPSP.S.Data.DistanceFrom(start, end);

        public static int DistanceFrom_APSP(this Agent agent, Vector2Int end)
            => PacmanAPSP.S.Data.DistanceFrom(agent, end);


        // Is Reachable
        public static bool IsReachable_APSP(this PacmanGame game, Vector2Int start, Vector2Int end)
            => PacmanAPSP.S.Data.IsReachable(start, end);

        public static bool IsReachable_APSP(this Agent agent, Vector2Int end)
            => PacmanAPSP.S.Data.IsReachable(agent, end);
    }
}