using Xunit;
using System.IO;
using Should;

namespace Cassette
{
    public class GivenInMemoryAsset_WithTwoSourceFiles
    {
        public GivenInMemoryAsset_WithTwoSourceFiles()
        {
            asset = new InMemoryAsset(
                new[] { "c:\\test1.js", "c:\\test2.js" },
                Stream.Null,
                new AssetReference[0]
            );
        }

        readonly InMemoryAsset asset;

        [Fact]
        public void IsFromFirstFile_ReturnsTrue()
        {
            asset.IsFrom("c:\\test1.js").ShouldBeTrue();
        }

        [Fact]
        public void IsFromSecondFile_ReturnsTrue()
        {
            asset.IsFrom("c:\\test2.js").ShouldBeTrue();
        }

        [Fact]
        public void IsFromFirstFileDifferentlyCased_ReturnsTrue()
        {
            asset.IsFrom("c:\\TEST1.js").ShouldBeTrue();
        }

        [Fact]
        public void IsFromFileNotInSource_ReturnsFalse()
        {
            asset.IsFrom("c:\\test3.js").ShouldBeFalse();
        }
    }
}
