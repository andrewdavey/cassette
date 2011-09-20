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

using System.Collections.Generic;
using System.Linq;
using Should;
using Xunit;

namespace Cassette.Utilities
{
    public class Graph_FindCycles_Tests
    {
        [Fact]
        public void NoCycle()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[]{2} },
                { 2, new[]{3} },
                { 3, new int[]{} }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3 }, i => edges[i]);

            graph.FindCycles().ShouldBeEmpty();
        }

        [Fact]
        public void Disconnected()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new int[]{} },
                { 2, new int[]{} },
                { 3, new int[]{} }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3 }, i => edges[i]);

            graph.FindCycles().ShouldBeEmpty();
        }

        [Fact]
        public void Diamond()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[]{2,3} },
                { 2, new[]{4} },
                { 3, new[]{4} },
                { 4, new int[]{} }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3, 4 }, i => edges[i]);

            graph.FindCycles().ShouldBeEmpty();
        }

        [Fact]
        public void TotalCycle()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[]{2} },
                { 2, new[]{3} },
                { 3, new[]{1} }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3 }, i => edges[i]);

            graph.FindCycles().First().SetEquals(new[] { 1, 2, 3 }).ShouldBeTrue();
        }

        [Fact]
        public void PartialCycle()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[]{2} },
                { 2, new[]{3} },
                { 3, new[]{1} },
                { 4, new[]{1} }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3, 4 }, i => edges[i]);

            graph.FindCycles().First().SetEquals(new[] { 1, 2, 3 }).ShouldBeTrue();
        }

        [Fact]
        public void DisconnectedWithACycle()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new int[]{} },
                { 2, new[]{3} },
                { 3, new[]{2} }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3 }, i => edges[i]);

            graph.FindCycles().First().SetEquals(new[] { 2, 3 }).ShouldBeTrue();
        }

        [Fact]
        public void DiamondWithCycle()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[] { 2, 3 } },
                { 2, new[] { 4 } },
                { 3, new[] { 4 } },
                { 4, new[] { 1 } }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3, 4 }, i => edges[i]);

            graph.FindCycles().First().SetEquals(new[] { 1, 2, 3, 4 }).ShouldBeTrue();
        }

        [Fact]
        public void TwoDisconnectedCycles()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[] { 2 } },
                { 2, new[] { 1 } },
                { 3, new[] { 4 } },
                { 4, new[] { 3 } }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3, 4 }, i => edges[i]);

            var cycles = graph.FindCycles().ToArray();
            cycles[0].SetEquals(new[] { 1, 2 }).ShouldBeTrue();
            cycles[1].SetEquals(new[] { 3, 4 }).ShouldBeTrue();
        }

        [Fact]
        public void BackReferenceDoesNotCauseCycle()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new[] { 2 } },
                { 2, new[] { 3 } },
                { 3, new int[] { } },
                { 4, new[] { 1 } }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3, 4 }, i => edges[i]);

            graph.FindCycles().ShouldBeEmpty();
        }

        [Fact]
        public void MoreComplexGraphWithNoCycles()
        {
            var edges = new Dictionary<int, int[]>
            {
                { 1, new int[] { } },
                { 2, new int[] { } },
                { 3, new[] { 1, 2, 5 } },
                { 4, new[] { 1, 2, 6 } },
                { 5, new[] { 2 } },
                { 6, new[] { 2, 3, 7, 5 } },
                { 7, new[] { 3 } },
                { 8, new[] { 3 } }
            };
            var graph = new Graph<int>(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, i => edges[i]);

            var result = graph.FindCycles().ToArray();
            result.ShouldBeEmpty();
        }
    }
}
