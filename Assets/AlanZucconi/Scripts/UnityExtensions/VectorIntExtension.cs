using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi
{
    public static class VectorIntExtension
    {
        public static Vector3Int V3I (this Vector2Int v)
        {
            return new Vector3Int(v.x, v.y, 0);
        }


        // Deconstructs Vector2Int into (int, int)
        // Allows this:
        //  var (x, y) = v2i;
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct
        public static void Deconstruct (this Vector2Int v, out int x, out int y)
        {
            x = v.x;
            y = v.y;
        }

        // Deconstructs Vector2Int into (int, int)
        // Allows this:
        //  var (x, y, z) = v3i;
        // https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/functional/deconstruct
        public static void Deconstruct(this Vector3Int v, out int x, out int y, out int z)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }
    }
}