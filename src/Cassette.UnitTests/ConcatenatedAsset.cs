using System.IO;
using System.Linq;
using System.Xml.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class GivenConcatenatedAsset_WithTwoChildren
    {
        public GivenConcatenatedAsset_WithTwoChildren()
        {
            child1 = new Mock<IAsset>();
            child2 = new Mock<IAsset>();
            asset = new ConcatenatedAsset(
                new[] { child1.Object, child2.Object },
                Stream.Null
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
    }
}
