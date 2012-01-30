using Cassette.Scripts.Manifests;
using Cassette.Stylesheets.Manifests;
using Should;
using Xunit;
using System;

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

    public class BundleManifest_CreateBundle_Tests
    {
        readonly TestableBundleManifest manifest;

        public BundleManifest_CreateBundle_Tests()
        {
            manifest = new TestableBundleManifest
            {
                Path = "~",
                Hash = new byte[0]
            };
        }

        [Fact]
        public void GivenManifestHasContent_WhenCreateBundle_ThenBundleOpenStreamReturnsTheContent()
        {
            manifest.Content = new byte[] { 1, 2, 3 };
            var bundle = manifest.CreateBundle();

            using (var stream = bundle.OpenStream())
            {
                var bytes = new byte[3];
                stream.Read(bytes, 0, 3);
                bytes.ShouldEqual(new byte[] { 1, 2, 3 });
            }
        }

        [Fact]
        public void GivenManifestHasNoContent_WhenCreateBundle_ThenBundleOpenStreamThrowsException()
        {
            manifest.Content = null;
            var bundle = manifest.CreateBundle();

            Assert.Throws<InvalidOperationException>(() => bundle.OpenStream());
        }

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                return new TestableBundle(Path);
            }
        }
    }
}