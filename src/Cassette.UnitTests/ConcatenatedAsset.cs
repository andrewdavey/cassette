using System;
using System.Linq;
using Cassette.BundleProcessing;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ConcatenatedAsset_Tests
    {
        public ConcatenatedAsset_Tests()
        {
            var child = new Mock<IAsset>();
            child.Setup(c => c.GetTransformedContent()).Returns("test");
            asset = new ConcatenatedAsset(new[] {child.Object}, "");
        }

        readonly ConcatenatedAsset asset;

        [Fact]
        public void HashIsSHA1OfContent()
        {
            asset.Hash.SequenceEqual("test".ComputeSHA1Hash()).ShouldBeTrue();
        }
    }

    public class GivenConcatenatedAsset_WithTwoChildren
    {
        public GivenConcatenatedAsset_WithTwoChildren()
        {
            child1 = new Mock<IAsset>();
            child1.Setup(c => c.GetTransformedContent()).Returns("");
            child2 = new Mock<IAsset>();
            child2.Setup(c => c.GetTransformedContent()).Returns("");
            asset = new ConcatenatedAsset(
                new[] { child1.Object, child2.Object },
                ""
            );
        }

        readonly ConcatenatedAsset asset;
        readonly Mock<IAsset> child1, child2;

        [Fact]
        public void AcceptCallsVisitOnVisitorForEachChildAsset()
        {
            var visitor = new Mock<IBundleVisitor>();
            asset.Accept(visitor.Object);
            visitor.Verify(v => v.Visit(child1.Object));
            visitor.Verify(v => v.Visit(child2.Object));
        }
    }

    public class ConcatenatedAssetWithSeparator_Tests
    {
        readonly ConcatenatedAsset asset;

        public ConcatenatedAssetWithSeparator_Tests()
        {
            var child1 = new Mock<IAsset>();
            var child2 = new Mock<IAsset>();
            child1.Setup(c => c.GetTransformedContent()).Returns("first");
            child2.Setup(c => c.GetTransformedContent()).Returns("second");
            asset = new ConcatenatedAsset(new[] { child1.Object, child2.Object }, ";");
        }

        [Fact]
        public void OpenStreamReturnsStreamWhereChildAssetContentIsSeparatedWithSeparator()
        {
            asset.GetTransformedContent().ShouldEqual("first;second");
        }
    }
}