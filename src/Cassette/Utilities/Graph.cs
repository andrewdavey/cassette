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
        class Node
        {
            public T Value;
            public bool Visited;
            public int Index;
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
            return GetConnectedSets()
                .Select(FindCycle)
                .Where(cycle => cycle.Count > 1);
        }

        ISet<T> FindCycle(ISet<Node> connectedSet)
        {
            var roots = new List<Node>();
            foreach (var node in connectedSet)
            {
                node.Index = node.Incoming.Count;
                if (node.Index == 0)
                {
                    roots.Add(node);
                }
            }

            if (roots.Count == 0) // Totally connected
            {
                return new HashSet<T>(connectedSet.Select(n => n.Value));
            }

            foreach (var root in roots)
            {
                DetectCyclesFromNode(root);
            }

            return new HashSet<T>(
                connectedSet.Where(node => node.Index < 0)
                            .Select(n => n.Value)
            );
        }

        void UnVisitAllNodes()
        {
            foreach (var node in nodes)
            {
                node.Visited = false;
            }
        }

        IEnumerable<ISet<Node>> GetConnectedSets()
        {
            UnVisitAllNodes();

            var sets = new List<ISet<Node>>();
            foreach (var node in nodes)
            {
                if (node.Visited) continue;

                var set = new HashSet<Node>();
                sets.Add(set);
                GetConnectedSet(node, set);
            }
            if (sets.Count == 0 && nodes.Length > 0)
            {
                sets.Add(new HashSet<Node>(nodes));
            }
            return sets;
        }

        void GetConnectedSet(Node start, ISet<Node> set)
        {
            start.Visited = true;
            set.Add(start);
            foreach (var node in start.Outgoing.Concat(start.Incoming))
            {
                if (node.Visited) continue;

                GetConnectedSet(node, set);
            }
        }

        void DetectCyclesFromNode(Node node)
        {
            if (node.Index > 0)
            {
                node.Index--;
                if (node.Index < 0) return;
            }
            foreach (var nextNode in node.Outgoing)
            {
                DetectCyclesFromNode(nextNode);
            }
        }
    }

}

