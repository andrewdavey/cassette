using System.IO;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleManifest_Tests
    {
        readonly ScriptBundleManifest manifest;
        readonly ScriptBundle createdBundle;
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
            createdBundle = (ScriptBundle)manifest.CreateBundle(file.Object);
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

    public class ScriptBundleManifest_InitializeFromXElement_Tests
    {
        [Fact]
        public void UrlAssignedFromAttribute()
        {
            var manifest = new ExternalScriptBundleManifest();
            manifest.InitializeFromXElement(new XElement("ExternalScriptBundleManifest",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/")
            ));

            manifest.Url.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void GivenUrlAttributeMissingThenExceptionThrown()
        {
            var manifest = new ExternalScriptBundleManifest();
            Assert.Throws<InvalidBundleManifestException>(
                () => manifest.InitializeFromXElement(new XElement("ExternalScriptBundleManifest",
                    new XAttribute("Path", "~"),
                    new XAttribute("Hash", "")
                ))
            );
        }

        [Fact]
        public void FallbackConditionAssignedFromAttribute()
        {
            var manifest = new ExternalScriptBundleManifest();
            manifest.InitializeFromXElement(new XElement("ExternalScriptBundleManifest",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("FallbackCondition", "CONDITION")
            ));

            manifest.FallbackCondition.ShouldEqual("CONDITION");
        }

        [Fact]
        public void FallbackConditionIsNullWhenAttributeMissing()
        {
            var manifest = new ExternalScriptBundleManifest();
            manifest.InitializeFromXElement(new XElement("ExternalScriptBundleManifest",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/")
            ));

            manifest.FallbackCondition.ShouldBeNull();
        }
    }
}