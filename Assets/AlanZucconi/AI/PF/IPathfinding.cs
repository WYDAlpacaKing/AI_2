using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AlanZucconi.AI.PF
{
    // N is the type of the graph
    // for instance, if this is a grid, N would be the position (Vector2Int)
    public interface IPathfinding<N>
    {
        // List of connected nodes from "node"
        IEnumerable<N> Outgoing(N node);
    }
}