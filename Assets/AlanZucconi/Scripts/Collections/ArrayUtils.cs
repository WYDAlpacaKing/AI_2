using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi
{
    public static class ArrayUtils
    {
        public static T[] Flatten<T>(this T[,] array)
        {
            T[] flat = new T[array.GetLength(0) * array.GetLength(1)];

            int t = 0;
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    flat[t++] = array[i, j];

            return flat;
        }

        public static T[,] Unflatten<T> (this T[] flat, int length0, int length1)
        {
            T[,] array = new T[length0,length1];

            int t = 0;
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    array[i, j] = flat[t++];

            return array;

        }

        /*
        // Multidimensional arrays [,] are not serializable in Unity
        // but jagged arrays [][] are!
        // This code allows converting between the two
        public static T[][] ToJaggedArray <T>(this T[,] array)
        {
            T[][] jaggedArray = new T[array.GetLength(0)][];
            for (int i = 0; i < array.GetLength(0); i++)
            {
                jaggedArray[i] = new T[array.GetLength(1)];

                for (int j = 0; j < array.GetLength(1); j++)
                    jaggedArray[i][j] = array[i, j];
            }

            return jaggedArray;
        }

        // Assumes second dimension is always the same
        public static T[,] ToMultiArray<T>(this T[][] jaggedArray)
        {
            Debug.Log(jaggedArray);
            T[,] array = new T[jaggedArray.Length,jaggedArray[0].Length];
            for (int i = 0; i < array.GetLength(0); i++)
                for (int j = 0; j < array.GetLength(1); j++)
                    array[i,j] = jaggedArray[i][j];

            return array;
        }
        */
    }
}