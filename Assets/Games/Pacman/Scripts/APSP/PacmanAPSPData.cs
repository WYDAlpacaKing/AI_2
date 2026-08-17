using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

using UnityEditor;
using AlanZucconi.AI.PF;
using System;

namespace AlanZucconi.Pacman
{
    #region SerializableData
    /*
    [System.Serializable]
    public struct Vector2IntPair
    {
        public Vector2Int Start;
        public Vector2Int End;

        public Vector2IntPair(Vector2Int start, Vector2Int end)
        {
            Start = start;
            End   = end;
        }
    }
    */
    [System.Serializable]
    public struct PFResult
    {
        public Action Action;
        public int    Distance;

        public PFResult(Action action, int distance)
        {
            Action   = action;
            Distance = distance;
        }
    }





    [Serializable]
    public class PFResult2DArray : Flat2DArray<PFResult>
    {
        public PFResult2DArray(int w, int h) : base(w, h) {}
    }





    //[System.Serializable]
    //public class APSPDictionary : SerializableDictionary<(Vector2Int start, Vector2Int end), (Action action, int distance)> { }
    //[System.Serializable]
    //public class APSPDictionary : SerializableDictionary<Vector2IntPair, PFResult> { }

    [Serializable]
    public class IndexDictionary : SerializableDictionary<Vector2Int, int> { }
    #endregion

    [CreateAssetMenu
    (
        fileName = "Pacman ASAP Data",
        menuName = "Pacman/ASAP Data"
    )]
    // All-Pairs Shortest Paths (APSP)
    public class PacmanAPSPData : ScriptableObject
    {
        /* Making this class efficient is key for good performance.
         *  A dictionary of the following type:
         *      SerializableDictionary<(Vector2Int start, Vector2Int end), (Action action, int distance)>
         *  would have around 90.000 entries, which is not that fast.
         *  
         *  To make the access faster, we are using a 2D array:
         *      (Action action, int distance)[start, end]
         *  
         *  We need to convert Vector2Int into array indices throuhg a smaller dictionary.
         */
        // Index dictionary: maps a Vector2Int to an integer position
        public IndexDictionary Index = null;
        public PFResult2DArray Paths = null;

        //public PFResult[,] Paths = null;

        // Given two positions,
        //  it returns the distance to the target
        //  and the action needed to get on that path
        //public SerializableDictionary<(Vector2Int start, Vector2Int end), (Action action, int distance)> Data = null;
        //[SerializeField]
        //public APSPDictionary Data = null;

        // Indicates which level was used for this
        public int LevelHash;
        /*
        public void Build(PacmanGame game)
        {
            Data = new();

            if (game.Level.Data == null)
                game.Level.Build();

            LevelHash = game.Level.Text.GetHashCode();

            // Loops through all distinct pairs of free positions
            foreach (var (start, end) in game.Level
                .FreePositions().ToList()
                .AllDistinctPairs())
            {
                PacmanPathfinding pf = new PacmanPathfinding(game);
                var path = pf.Dijkstra(start, end);

                Vector2IntPair pair = new Vector2IntPair(start, end);

                // No path available
                if (path == null)
                {
                    //Data[(start, end)] = (Action.None, int.MaxValue);
                    //Data[pair] = new PFResult(Action.None, int.MaxValue);

                    // Does not add anything to keep Dictionary small
                    // If (start,end) not found, it is assumed unreachable
                    continue;
                }

                // Already on the target
                if (path.Count == 1)
                {
                    //Data[(start, end)] = (Action.None, 0);
                    //Data[pair] = new PFResult(Action.None, 0);

                    // Does not add anything to keep Dictionary small
                    // If start==end, no need to do a dictionary lookup
                    continue;
                }

                // A path exists
                //Data[(start, end)] = (path[0].edge, path.Count -1);
                Data[pair] = new PFResult(path[0].edge, path.Count - 1);
            }

            // Loops through all distinct pairs of free positions
            //foreach (var (start, end) in game.Level
            //    .FreePositions().ToList()
            //    .AllDistinctPairs())
            //    Actions[(start, end)] = game.MoveTowards(start, end);
            

            // Saves
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
        */

        public (Action action, int distance) this[Vector2Int start, Vector2Int end]
        {
            get
            {
                // Same position?
                if (start == end)
                    return (Action.None, 0);


                // Retrieves the data
                //(Action action, int distance) output;
                //bool found = Data.TryGetValue((start, end), out output);

                // Start is unreachable
                int startIndex;
                if (!Index.TryGetValue(start, out startIndex))
                    return (Action.None, int.MaxValue);

                // End is unreachable
                int endIndex;
                if (!Index.TryGetValue(end, out endIndex))
                    return (Action.None, int.MaxValue);


                PFResult output = Paths[startIndex, endIndex];
                return (output.Action, output.Distance);

                /*

                // Retrieves the data
                //(Action action, int distance) output;
                //bool found = Data.TryGetValue((start, end), out output);

                PFResult output;
                Vector2IntPair pair = new Vector2IntPair(start, end);
                bool found = Data.TryGetValue(pair, out output);
                if (found)
                    return (output.Action, output.Distance);

                // Unreachable
                return (Action.None, int.MaxValue);
                */
            }
        }

        public Action MoveTowards(Vector2Int start, Vector2Int end)
            => this[start, end].action;

        public int DistanceFrom(Vector2Int start, Vector2Int end)
            => this[start, end].distance;

        // When unreachable: (Action.None, int.MaxValue)
        // (we can't check action == Action.None, because that is also true when start == end)
        public bool IsReachable(Vector2Int start, Vector2Int end)
            => this[start, end].distance == int.MaxValue;









        public void Build(PacmanGame game)
        {
            if (game.Level.Data == null)
                game.Level.Build();

            LevelHash = game.Level.Text.GetHashCode();


            Index = new();
            // Creates the indices for each avaialble location in the level
            foreach ((Vector2Int position, int i) in game.Level
                .FreePositions()
                .Select((value, i) => (value, i)))
                Index.Add(position, i);

            // Instantiates the 2D array
            //Paths = new PFResult[Index.Count, Index.Count];
            Paths = new PFResult2DArray(Index.Count, Index.Count);


            // Loops through all distinct pairs of free positions
            foreach (var (start, end) in game.Level
                .FreePositions().ToList()
                .AllDistinctPairs())
            {
                PacmanPathfinding pf = new PacmanPathfinding(game);
                var path = pf.Dijkstra(start, end);

                //Vector2IntPair pair = new Vector2IntPair(start, end);

                int startIndex = Index[start];
                int endIndex   = Index[end];

                // No path available
                if (path == null)
                {
                    Paths[startIndex, endIndex] = new PFResult(Action.None, int.MaxValue);
                    continue;
                }

                // Already on the target
                if (path.Count == 1)
                {
                    Paths[startIndex, endIndex] = new PFResult(Action.None, 0);
                    continue;
                }

                // A path exists
                //Data[(start, end)] = (path[0].edge, path.Count -1);
                Paths[startIndex, endIndex] = new PFResult(path[0].edge, path.Count - 1);
            }

            // Saves
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }
    }
}