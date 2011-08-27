using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Xml.Linq;
using Cassette.ModuleProcessing;
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
            var visitor = new Mock<IAssetVisitor>();
            asset.Accept(visitor.Object);
            visitor.Verify(v => v.Visit(child1.Object));
            visitor.Verify(v => v.Visit(child2.Object));
        }

        [Fact]
        public void CreateCacheManifestCreatesElementForEachAsset()
        {
            cacheableChild1.Setup(c => c.CreateCacheManifest()).Returns(new[] { new XElement("Asset") });
            cacheableChild2.Setup(c => c.CreateCacheManifest()).Returns(new[] { new XElement("Asset") });
            var assetElements = asset.CreateCacheManifest().ToArray();
            assetElements.Length.ShouldEqual(2);
        }

        public void Dispose()
        {
            asset.Dispose();
        }
    }
}
