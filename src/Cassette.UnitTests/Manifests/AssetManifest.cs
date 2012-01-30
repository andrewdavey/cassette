using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class AssetManifest_Equals_Tests
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