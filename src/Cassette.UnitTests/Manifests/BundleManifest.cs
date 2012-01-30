using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class BundleManifest_Equals_Tests
    {
        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                throw new System.NotImplementedException();
            }
        }

        [Fact]
        public void BundleManifestsWithSamePathAndNoAssetsAreEqual()
        {
            var manifest1 = new TestableBundleManifest { Path = "~/path" };
            var manifest2 = new TestableBundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }

        [Fact]
        public void BundleManifestsOfDifferentTypeAreNotEqual()
        {
            BundleManifest manifest1 = new StylesheetBundleManifest { Path = "~/path" };
            BundleManifest manifest2 = new ScriptBundleManifest { Path = "~/path" };
            manifest1.Equals(manifest2).ShouldBeFalse();
        }

        [Fact]
        public void BundleManifestsWithDifferentAssetsAreNotEqual()
        {
            var manifest1 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            var manifest2 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/different-asset-path" } }
            };
            manifest1.Equals(manifest2).ShouldBeFalse();
        }

        [Fact]
        public void BundleManifestsWithSameAssetsAreEqual()
        {
            var manifest1 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            var manifest2 = new TestableBundleManifest
            {
                Path = "~/path",
                Assets = { new AssetManifest { Path = "~/asset-path" } }
            };
            manifest1.Equals(manifest2).ShouldBeTrue();
        }
    }
}