using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Cassette.BundleProcessing;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ConcatenatedAsset_Tests : IDisposable
    {
        public ConcatenatedAsset_Tests()
        {
            var child = new Mock<IAsset>();
            child.Setup(c => c.OpenStream()).Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));
            asset = new ConcatenatedAsset("~/path", new[] { child.Object }, "");
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
            child1.Setup(c => c.OpenStream()).Returns(() => Stream.Null);
            child2 = new Mock<IAsset>();
            child2.Setup(c => c.OpenStream()).Returns(() => Stream.Null);
            asset = new ConcatenatedAsset(
                "~/path",
                new[] { child1.Object, child2.Object },
                ""
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

    public class ConcatenatedAssetWithSeparator_Tests : IDisposable
    {
        readonly ConcatenatedAsset asset;

        public ConcatenatedAssetWithSeparator_Tests()
        {
            var child1 = new Mock<IAsset>();
            var child2 = new Mock<IAsset>();
            child1.Setup(c => c.OpenStream()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("first")));
            child2.Setup(c => c.OpenStream()).Returns(() => new MemoryStream(Encoding.UTF8.GetBytes("second")));
            asset = new ConcatenatedAsset("~/path", new[] { child1.Object, child2.Object }, ";");
        }

        [Fact]
        public void OpenStreamReturnsStreamWhereChildAssetContentIsSeparatedWithSeparator()
        {
            using (var stream = asset.OpenStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                reader.ReadToEnd().ShouldEqual("first;second");
            }
        }

        public void Dispose()
        {
            asset.Dispose();
        }
    }
}