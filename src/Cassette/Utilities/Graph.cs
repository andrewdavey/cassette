#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;

namespace Cassette.Utilities
{
    class Graph<T>
    {
        [System.Diagnostics.DebuggerDisplay("{Value}")]
        class Node
        {
            public T Value;
            public bool Visited;
            public int Index;
            public int LowLink;
            public readonly ISet<Node> Incoming = new HashSet<Node>();
            public readonly ISet<Node> Outgoing = new HashSet<Node>();
        }

        readonly Node[] nodes;

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
                    fromNode.Outgoing.Add(toNode);
                }
            }
        }

        public IEnumerable<T> TopologicalSort()
        {
            UnVisitAllNodes();

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

        public IEnumerable<ISet<T>> FindCycles()
        {
            // Tarjan's strongly connected components algorithm
            // http://en.wikipedia.org/wiki/Tarjan%E2%80%99s_strongly_connected_components_algorithm

            var index = 0;
            var stack = new Stack<Node>();
            var cycles = new HashSet<ISet<T>>();

            foreach (var node in nodes)
            {
                node.Index = -1;
            }

            foreach (var node in nodes)
            {
                if (node.Index == -1)
                {
                    StrongConnect(node, stack, ref index, cycles);
                }
            }

            return cycles.Where(c => c.Count > 1); // Singleton sub-graphs are not a cycle for Cassette's purposes.
        }

        void StrongConnect(Node node, Stack<Node> stack, ref int index, ISet<ISet<T>> outputs)
        {
            node.Index = index;
            node.LowLink = index;
            index++;
            stack.Push(node);

            foreach (var next in node.Outgoing)
            {
                if (next.Index == -1)
                {
                    StrongConnect(next, stack, ref index, outputs);
                    node.LowLink = Math.Min(node.LowLink, next.LowLink);
                }
                else if (stack.Contains(next))
                {
                    node.LowLink = Math.Min(node.LowLink, next.Index);
                }
            }

            if (node.LowLink == node.Index)
            {
                var cycle = new HashSet<T>();
                Node w;
                do
                {
                    w = stack.Pop();
                    cycle.Add(w.Value);
                } while (w != node);
                outputs.Add(cycle);
            }
        }

        void UnVisitAllNodes()
        {
            foreach (var node in nodes)
            {
                node.Visited = false;
            }
        }
    }
}

