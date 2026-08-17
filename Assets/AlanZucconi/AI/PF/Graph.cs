using System.Collections;
using System.Collections.Generic;
using Unity.PlasticSCM.Editor.UI;

namespace AlanZucconi.AI.PF
{
    public class Graph<N> : IPathfinding<N>
    {
        // Nodes[node] = List of neighbours nodes
        public Dictionary<N, HashSet<N>> Nodes = new Dictionary<N, HashSet<N>>();

        // Adds the node if not connected
        public void Connect(N a, N b, bool twoWays = true)
        {
            HashSet<N> to;

            // First time this node "a" is added
            if (! Nodes.ContainsKey(a))
            {
                to = new HashSet<N>();
                Nodes.Add(a, to);
            } else
            // The node "a" is already known
            {
                to = Nodes[a];
            }

            to.Add(b);

            if (twoWays)
                Connect(b, a, false);
        }

        // Disconnects two nodes
        // Returns false if no such connection existed.
        public bool Disconnect (N a, N b, bool twoWays = true)
        {
            // "a" is not connected to anything
            if (!Nodes.ContainsKey(a))
                return false;

            HashSet<N> to = Nodes[a];
            bool removed = to.Remove(b);


            if (twoWays)
                removed |= Disconnect(b, a, false);

            return removed;
        }

        public IEnumerable<N> Outgoing (N from)
        {
            // The node is not present
            if (!Nodes.ContainsKey(from))
                yield break;

            // Loops over the connected nodes
            foreach (N to in Nodes[from])
                yield return to;
        }
    }
    /*
    public class GraphInt : MapStruct<int>
    {
        public override IEnumerable<int> Outstar(int node)
        {
            //List<int> list = new List<int>();
            //return list;
            return new int[] { };
        }
    }
    */

    /*
    // Used to test
    public void Test ()
    {
        Graph<string> graph = new Graph<string>();
        graph.Connect("a", "b");
        graph.Connect("a", "c");
        graph.Connect("a", "e");

        graph.Connect("b", "f");

        graph.Connect("c", "d");
        graph.Connect("c", "g");

        graph.Connect("d", "g");
        graph.Connect("d", "f");

        graph.Connect("e", "f");
        graph.Connect("e", "h");

        graph.Connect("f", "z");

        graph.Connect("g", "z");

        graph.Connect("h", "z");


        List<string> p = graph.BreadthFirstSearch("a", "z");
        Debug.Log("a to z:");
        foreach (string node in p)
            Debug.Log("\t" + node);


        p = graph.BreadthFirstSearch("a", "a");
        Debug.Log("a to a:");
        foreach (string node in p)
            Debug.Log("\t" + node);

        p = graph.BreadthFirstSearch("a", "x");
        Debug.Log(p);

        p = graph.BreadthFirstSearch("x", "a");
        Debug.Log(p);
    }
    */
}