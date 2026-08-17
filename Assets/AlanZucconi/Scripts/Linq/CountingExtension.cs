using System.Collections.Generic;
using System.Linq;
using UnityEngine.Jobs;

namespace AlanZucconi
{
    public static class Counting
    {
        // All pairs of elements from a list
        // Returns (i,i)
        // Returns both (i,j) and (j,i)
        public static IEnumerable<(T, T)> AllPairs<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                for (int j = 0; j < list.Count; j++)
                        yield return (list[i], list[j]);
        }


        // All pairs of elements from a list
        // Return both (i,j) and (j,i)
        public static IEnumerable<(T, T)> AllDistinctPairs<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
                for (int j = 0; j < list.Count; j++)
                    if (i != j)
                        yield return (list[i], list[j]);
        }


        // All distinct pairs of elements from a list
        public static IEnumerable<(T, T)> DistinctPairs<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
                for (int j = i + 1; j < list.Count; j++)
                    yield return (list[i], list[j]);
        }
        /*
        public static IEnumerable<List<T>> DistinctPairs<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    List<T> pair = new List<T>();
                    pair.Add(list[i]);
                    pair.Add(list[j]);

                    yield return pair;
                }
            }
        }
        */
        // Indices for an array
        public static IEnumerable<(int, int)> DistinctPairs(int count)
        {
            for (int i = 0; i < count - 1; i++)
                for (int j = i + 1; j < count; j++)
                    yield return (i, j);
        }
        /*
        public static IEnumerable<Tuple<int, int>> DistinctPairs(int count)
        {
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    yield return new Tuple<int, int>(i, j);
                }
            }
        }
        */
    }
}