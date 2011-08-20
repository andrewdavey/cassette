using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Cassette.ModuleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ConcatenatedAsset_Tests : IDisposable
    {
        public ConcatenatedAsset_Tests()
        {
            stream = new MemoryStream(new byte[] { 1, 2, 3 });
            asset = new ConcatenatedAsset(Enumerable.Empty<IAsset>(), stream);
        }

        readonly MemoryStream stream;
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
            stream.Dispose();
        }
    }

    public class GivenConcatenatedAsset_WithTwoChildren : IDisposable
    {
        public GivenConcatenatedAsset_WithTwoChildren()
        {
            child1 = new Mock<IAsset>();
            child2 = new Mock<IAsset>();
            asset = new ConcatenatedAsset(
                new[] { child1.Object, child2.Object },
                new MemoryStream()
            );
        }

        readonly ConcatenatedAsset asset;
        readonly Mock<IAsset> child1, child2;

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
            var visitor = new Mock<IAssetVisitor>();
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
