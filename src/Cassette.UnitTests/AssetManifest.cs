using Should;
using Xunit;

namespace Cassette
{
    public class AssetManifest_Tests
    {
        [Fact]
        public void AssetsManifestsWithSamePathAreEqual()
        {
            var manifest1 = new AssetManifest { Path = "~/path" };
            var manifest2 = new AssetManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }
    }
}