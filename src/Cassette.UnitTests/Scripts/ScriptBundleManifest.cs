using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleManifest_Tests
    {
        readonly ScriptBundleManifest manifest;
        readonly Bundle createdBundle;

        public ScriptBundleManifest_Tests()
        {
            manifest = new ScriptBundleManifest
            {
                Path = "~",
                Hash = new byte[] { 1, 2, 3 },
                ContentType = "EXPECTED-CONTENT-TYPE",
                PageLocation = "EXPECTED-PAGE-LOCATION",
                Assets =
                    {
                        new AssetManifest { Path = "~/asset" }
                    }
            };
            createdBundle = manifest.CreateBundle();
        }

        [Fact]
        public void CreateBundledPathEqualsManifestPath()
        {
            createdBundle.Path.ShouldEqual(manifest.Path);
        }

        [Fact]
        public void CreatedBundleHashEqualsManifestHash()
        {
            createdBundle.Hash.ShouldEqual(manifest.Hash);    
        }

        [Fact]
        public void CreatedBundleContentTypeEqualsManifestContentType()
        {
            createdBundle.ContentType.ShouldEqual(manifest.ContentType);
        }

        [Fact]
        public void CreatedBundlePageLocationEqualsManifestPageLocation()
        {
            createdBundle.PageLocation.ShouldEqual(manifest.PageLocation);
        }

        [Fact]
        public void CreatedBundleIsFromCache()
        {
            createdBundle.IsFromCache.ShouldBeTrue();
        }

        [Fact]
        public void CreatedBundleContainsAssetPath()
        {
            createdBundle.ContainsPath("~/asset").ShouldBeTrue();
        }
    }
}
