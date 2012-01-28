using Should;
using Xunit;

namespace Cassette
{
    public class BundleManifest_Tests
    {
        [Fact]
        public void BundleManifestsWithSamePathAndNoAssetsAreEqual()
        {
            var manifest1 = new BundleManifest { Path = "~/path" };
            var manifest2 = new BundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }

        [Fact]
        public void BundleManifestsOfDifferentTypeAreNotEqual()
        {
            var manifest1 = new Stylesheets.StylesheetBundleManifest { Path = "~/path" };
            var manifest2 = new BundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeFalse();
        }

        [Fact]
        public void BundleManifestsWithDifferentAssetsAreNotEqual()
        {
            var manifest1 = new BundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            var manifest2 = new BundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/different-asset-path" } }
            };
            manifest1.Equals(manifest2).ShouldBeFalse();
        }

        [Fact]
        public void BundleManifestsWithSameAssetsAreEqual()
        {
            var manifest1 = new BundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            var manifest2 = new BundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }
    }
}