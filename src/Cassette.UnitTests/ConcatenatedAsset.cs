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
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;
using Cassette.Persistence;

namespace Cassette
{
    public class ConcatenatedAsset_Tests : IDisposable
    {
        public ConcatenatedAsset_Tests()
        {
            var child = new Mock<IAsset>();
            child.Setup(c => c.OpenStream()).Returns(() => new MemoryStream(new byte[] {1, 2, 3}));
            asset = new ConcatenatedAsset(new[] {child.Object});
        }

        readonly ConcatenatedAsset asset;

        [Fact]
        public void HashIsSHA1OfStream()
        {
            byte[] expected;
            using (var sha1 = SHA1.Create())
            {
                expected = sha1.ComputeHash(new byte[] { 1, 2, 3 });
            }
            asset.Hash.SequenceEqual(expected).ShouldBeTrue();
        }

        public void Dispose()
        {
            asset.Dispose();
        }
    }

    public class GivenConcatenatedAsset_WithTwoChildren : IDisposable
    {
        public GivenConcatenatedAsset_WithTwoChildren()
        {
            child1 = new Mock<IAsset>();
            cacheableChild1 = child1.As<ICacheableAsset>();
            child1.Setup(c => c.OpenStream()).Returns(() => Stream.Null);
            child2 = new Mock<IAsset>();
            cacheableChild2 = child2.As<ICacheableAsset>();
            child2.Setup(c => c.OpenStream()).Returns(() => Stream.Null);
            asset = new ConcatenatedAsset(
                new[] { child1.Object, child2.Object }
            );
        }

        readonly ConcatenatedAsset asset;
        readonly Mock<IAsset> child1, child2;
        readonly Mock<ICacheableAsset> cacheableChild1;
        readonly Mock<ICacheableAsset> cacheableChild2;

        [Fact]
        public void RepeatedOpenStreamCallsReturnNewStreams()
        {
            using (var stream1 = asset.OpenStream())
            using (var stream2 = asset.OpenStream())
            {
                stream1.ShouldNotBeSameAs(stream2);
            }
        }

        [Fact]
        public void AcceptCallsVisitOnVisitorForEachChildAsset()
        {
            var visitor = new Mock<IBundleVisitor>();
            asset.Accept(visitor.Object);
            visitor.Verify(v => v.Visit(child1.Object));
            visitor.Verify(v => v.Visit(child2.Object));
        }

        public void Dispose()
        {
            asset.Dispose();
        }
    }
}

