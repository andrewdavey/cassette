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
        public void IsFromFirstFile_ReturnsTrue()
        {
            child1.Setup(c => c.IsFrom("c:\\test1.js")).Returns(true);
            asset.IsFrom("c:\\test1.js").ShouldBeTrue();
        }

        [Fact]
        public void IsFromSecondFile_ReturnsTrue()
        {
            child2.Setup(c => c.IsFrom("c:\\test2.js")).Returns(true);
            asset.IsFrom("c:\\test2.js").ShouldBeTrue();
        }

        [Fact]
        public void IsFromFileNotInSource_ReturnsFalse()
        {
            asset.IsFrom("c:\\test3.js").ShouldBeFalse();
        }

        [Fact]
        public void CreateManifestReturnsEachInnerAssetsManifest()
        {
            child1.Setup(c => c.CreateManifest()).Returns(new[] { new XElement("asset") });
            child2.Setup(c => c.CreateManifest()).Returns(new[] { new XElement("asset") });
            var manifest = asset.CreateManifest().ToArray();
            manifest.Length.ShouldEqual(2);
        }

        [Fact]
        public void RepeatedOpenStreamCallsReturnNewStreams()
        {
            using (var stream1 = asset.OpenStream())
            using (var stream2 = asset.OpenStream())
            {
                stream1.ShouldNotBeSameAs(stream2);
            }
        }
    }
}
