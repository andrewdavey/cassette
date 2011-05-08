using System;
using System.Collections.Generic;
using System.Linq;

namespace Knapsack.Utilities
{
    class Graph<T>
    {
        class Node
        {
            public T Value;
            public bool Visited;
            public ISet<Node> Incoming = new HashSet<Node>();
        }

        Node[] nodes;

        public Graph(IEnumerable<T> values, Func<T, IEnumerable<T>> getDependencies)
        {
            nodes = values.Select(value => new Node { Value = value }).ToArray();
            // Create the Incoming edge sets for each node.
            foreach (var fromNode in nodes)
            {
                foreach (var dependency in getDependencies(fromNode.Value))
                {
                    var toNode = nodes.First(n => n.Value.Equals(dependency));
                    toNode.Incoming.Add(fromNode);
                }
            }
        }

        public IEnumerable<T> TopologicalSort()
        {
            var results = new List<T>();
            var initial = nodes.Where(n => n.Incoming.Count == 0);
            foreach (var node in initial)
            {
                Visit(node, results);
            }
            return results;
        }

        void Visit(Node node, List<T> results)
        {
            if (node.Visited) return;

            node.Visited = true;

            foreach (var next in nodes.Where(n => n.Incoming.Contains(node)))
            {
                Visit(next, results);
            }

            results.Add(node.Value);
        }
    }

}
