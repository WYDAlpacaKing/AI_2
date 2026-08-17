using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;

using DataStructures.PriorityQueue;

namespace AlanZucconi.AI.PF
{
    public static partial class Pathfinding
    {
        // ASSUMPTION:
        // from two nodes "A" and "B"
        // there can only be ONE edge!
        //
        // The returned path includes the start and goal nodes:
        //  BreadthFirstSearch(a, -) = null         // unreachable goal
        //  BreadthFirstSearch(-, a) = null         // unreachable start
        //  BreadthFirstSearch(a, a) = [a]          // already on the node
        //  BreadthFirstSearch(a, c) = [a, b, c]

        /* This version of the pathfinding does not have a goal,
         * but has a function to test is a node is the goal.
         * 
         * This is because with GOAP, we don't have an exact goal state,
         * but a list of properties a state needs to be a goal!
         * 
         * 
         * Normally, you would expect the heuristic to be in the following form:
         *      heuristic(current, goal)
         * However, we can't use this version!
         * This is because this implementation of A* does NOT have a goal,
         * but a "isGoal" function.
         * The reason for this is that when used for planning,
         * the goal is not the exact node we want to reach, but a node that contains is.
         * 
         * There will be an overload of AStar which takes a goal node rather than a isGoal,
         * which will also provide an appropriate heuristic in the typical form.
         */
        public static List<(N, E)> AStar <N, E>
        (
            this IPathfindingCost<N,E> graph,
            N start,
            Func<N, bool> isGoal,
            Func<N, float> heuristic, //IHeuristic<N, N> heuristic,
            int maxSteps = 1000
        )
            where E : IEdge
        {
            // The frontier of active nodes
            PriorityQueue<N, float> frontier = new PriorityQueue<N, float>(0f);
            frontier.Insert(start, 0f);
            //frontier.Enqueue(start, 0f);

            // The list of visited nodes
            //HashSet<N> visited = new HashSet<N>();
            //visited.Add(start);

            // The node we came from
            // Dictionary<to node, from node>
            //Dictionary<N, N> visitedFrom = new Dictionary<N, N>();
            Dictionary<N, Null<N>> visitedFrom = new Dictionary<N, Null<N>>();
            visitedFrom[start] = null;

            // Cost so far
            Dictionary<N, float> costSoFar = new Dictionary<N, float>();
            costSoFar[start] = 0;

            // Current edge we came from
            Dictionary<N, E> fromEdge = new Dictionary<N, E>();
            fromEdge[start] = default;
            //fromEdge[start] = null;


            // BIG problem!
            // We want a method that works with both classes (IMap<string>)
            // and struct (IMap<Vector2Int>).
            // This is *very* tricky because struct cannot be null.
            // To go around this, we wrap the generic type N
            // ins a reference type "Null" which makes it nullable.
            // This step is redundant if N was already a class,
            // but at least it works with struct as well!

            int step = 0;

            // --------------------------------------
            // Keeps expanding the frontier
            //while (frontier.Count > 0)
            Null<N> goal = null;
            while (! frontier.IsEmpty())
            {
                // Out of time?
                step++;
                if (step >= maxSteps)
                    break;

                // Loops over all the connected nodes in the frontier...
                N current = frontier.Pop();
                //N current = frontier.Dequeue();
                // Early termination
                if (isGoal(current))
                {
                    goal = current;
                    break;
                }
                //if (EqualityComparer<N>.Default.Equals(current, goal))
                //    break;
                
                // Loops over the outstar of the current node...
                //foreach (N edge in current)
                //foreach (N next in current)
                foreach ((N next, E edge) in graph.Outgoing(current))
                {
                    float newCost = costSoFar[current] + edge.Cost;

                    //Debug.Log($"{step}\t{costSoFar[current]}\t{current}\t>> {edge}\t>> {next}\t");

                    // A better path has been found!
                    if (! costSoFar.ContainsKey(next) || 
                            newCost < costSoFar[next] )
                    {
                        //frontier.Enqueue(next, newCost);
                        frontier.Insert(next, newCost + heuristic(next)); // heuristic(next, goal)
                        visitedFrom[next] = current;
                        fromEdge   [next] = edge;
                        costSoFar  [next] = newCost;
                    }
                }
            }

            // Is there a path?
            //if (!visitedFrom.ContainsKey(goal))
            //if (! goalFound)
            if (goal == null)
                return null;

            // --------------------------------------
            // Reconstructs the path in reverse
            List<(N, E)> path = new List<(N, E)>();
            {
                //N from = visitedFrom[goal];
                //N from = goal;
                Null<N> from = goal;
                E edge = default;
                do
                {
                    path.Add((from, edge));
                    edge = fromEdge[from];
                    from = visitedFrom[from];

                } while (from != null);
            }

            // Reverse the path and returns it
            path.Reverse();
            return path;
        }


        // A version of the methods that takes a goal
        // (rather than a isGoal function)
        // FIXME: THIS should return just the list of Nodes, not actions
        public static List<(N, E)> AStar<N, E>
        (
            this IPathfindingCost<N, E> graph,
            N start, N goal,
            Func<N, N, float> heuristic,
            int maxSteps = 1000
        )
            where E : IEdge
        {
            return graph.AStar
            (   start,
                state => EqualityComparer<N>.Default.Equals(state, goal),
                state => heuristic(state, goal),
                maxSteps
            );
        }
    }
}