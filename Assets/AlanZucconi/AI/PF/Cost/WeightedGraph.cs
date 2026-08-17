using System.Collections;
using System.Collections.Generic;

namespace AlanZucconi.AI.PF
{
    public class WeightedGraph<N, E> : IPathfindingCost<N, E>
        where E : IEdge
    {
        // Nodes[node] = List of neighbours nodes
        public Dictionary<N, HashSet<(N, E)>> Nodes = new Dictionary<N, HashSet<(N, E)>>();

        // Adds the node if not connected
        public void Connect(N a, N b, E edge)//, bool twoWays = true)
        {
            HashSet<(N, E)> to;

            // First time this node "a" is added
            if (! Nodes.ContainsKey(a))
            {
                to = new HashSet<(N,E)>();
                Nodes.Add(a, to);
            } else
            // The node "a" is already known
            {
                to = Nodes[a];
            }

            to.Add((b, edge));

            //if (twoWays)
            //    Connect(b, a, edge, false);
        }

        // Disconnects two nodes
        // Returns the number of edges removed (or 0 if they were not connected)
        // There might be multiple connctions between "a" and "b": this removes them all!
        public int Disconnect(N a, N b)//, bool twoWays = true)
        {
            // "a" is not connected to anything
            if (!Nodes.ContainsKey(a))
                return 0;

            HashSet<(N, E)> to = Nodes[a];
            int removed = to.RemoveWhere(x => x.Item1.Equals(b)); // x = (N targetNode, E edgeFromAtoN)

            //if (twoWays)
            //    removed |= Disconnect(b, a, false);

            return removed;
        }

        public IEnumerable<(N, E)> Outgoing (N from)
        {
            // The node is not present
            if (!Nodes.ContainsKey(from))
                yield break;

            // Loops over the connected nodes
            foreach ((N to, E edge) in Nodes[from])
                yield return (to, edge);
        }
    }
}