using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace AlanZucconi.AI.PF
{
    // Used for Dijkstra and A*
    public interface IPathfindingCost<N, E>
        where E : IEdge
    {
        // List of connected nodes from "node", and the edge that let do them
        IEnumerable<(N, E)> Outgoing(N node);
    }

    // Used for the edges that connect two nodes
    // This is important for things such as Planning,
    // in which the list of traversed edges is the actual plan to execute
    public interface IEdge
    {
        float Cost { get; }
    }







    // Used for when the cost is simply a float
    //public interface IPathfindingCost<N> : IPathfindingCost<N,Edge>
    //{

    //}

    // A connection between two nodes which has a cost as a float
    public struct Edge : IEdge // used to be "class" -> change back if there are issues!
    {
        public Edge(float cost) => Cost = cost;

        public float Cost { get; }

        public static implicit operator float(Edge edge) => edge.Cost;
        public static implicit operator Edge(float cost) => new Edge(cost);
    }

    // A wrapper that can be used to make any type (T) into an edge
    // This is useful if have to use classes/structs that cannot extend IEdge
    // (such asn enums, or other classes you don't have access to).
    // If no cost is specified, edges are assumed to have cost 1
    public struct Edge<T> : IEdge
    {
        public Edge(T content, float cost = 1f)
        {
            Content = content;
            Cost = cost;
        }

        public T Content;
        public float Cost { get; }

        public static implicit operator float(Edge<T> edge) => edge.Cost;
        public static implicit operator Edge<T>(T content) => new Edge<T>(content);
        public static implicit operator T (Edge<T> edge) => edge.Content;
    }



    /*
    // Calculates the heuristic between two nodes
    // This is used for algorithms like A*
    public interface IHeuristic<N>
    {
        float Heuristic(N a, N b);
    }
    */


    // This class is a wrapper that takes an IPathfinding<N>
    // and converts it into an IPathfindingCost<N,E>.
    // This allows using Dijstra's and A* onto IPahtfinding<N>
    // despite not having a cost associated with their edges.
    public class UnitCostGraph<N> : IPathfindingCost<N, Edge>
    {
        private IPathfinding<N> Graph;

        public UnitCostGraph(IPathfinding<N> graph)
        {
            Graph = graph;
        }

        // List of connected nodes from "node", and the edge that let do them
        // Uses the same edges as Pathfinding, but adds a node with unit cost
        public IEnumerable<(N, Edge)> Outgoing(N node)
        {
            return Graph
                .Outgoing(node)                 // IEnumerable< N       >
                .Select(n => (n, new Edge(1))); // IEnumerable<(N, Edge)>
        }

        // Conversion to and from interfaces are not allows in C# :(
        //public static implicit operator PathfindingUnitCost<N> (IPathfinding<N> pathfinding)
    }

    
    public static partial class Pathfinding
    {
        // Extension methods to convert from IPahtfinding<E> to IPathfindingCost<N,E>
        public static UnitCostGraph<N> ToWeightedGraph<N> (this IPathfinding<N> graph)
            => new UnitCostGraph<N>(graph);
    }
}