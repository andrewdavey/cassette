using System.IO;
using Moq;
using Should;
using Xunit;
using Cassette.Utilities;
using Cassette.IO;

namespace Cassette.Scripts
{
    public class ScriptBundleManifest_Tests
    {
        readonly ScriptBundleManifest manifest;
        readonly Bundle createdBundle;
        const string BundleContent = "BUNDLE-CONTENT";

        public ScriptBundleManifest_Tests()
        {
            manifest = new ScriptBundleManifest
            {
                Path = "~",
                Hash = new byte[] { 1, 2, 3 },
                ContentType = "CONTENT-TYPE",
                PageLocation = "PAGE-LOCATION",
                Assets =
                    {
                        new AssetManifest { Path = "~/asset-a" },
                        new AssetManifest { Path = "~/asset-b" }
                    }
            };
            var file = new Mock<IFile>();
            file.Setup(f => f.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(() => BundleContent.AsStream());
            createdBundle = manifest.CreateBundle(file.Object);
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
        public void CreatedBundleContainsAssetPathA()
        {
            createdBundle.ContainsPath("~/asset-a").ShouldBeTrue();
        }

        [Fact]
        public void CreatedBundleContainsAssetPathB()
        {
            createdBundle.ContainsPath("~/asset-b").ShouldBeTrue();
        }

        [Fact]
        public void CreatedBundleOpenStreamReturnsBundleContent()
        {
            var content = createdBundle.OpenStream().ReadToEnd();
            content.ShouldEqual(BundleContent);
        }
    }
}