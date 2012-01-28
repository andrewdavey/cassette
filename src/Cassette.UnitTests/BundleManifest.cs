using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette
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
            BundleManifest manifest1 = new Stylesheets.StylesheetBundleManifest { Path = "~/path" };
            BundleManifest manifest2 = new Scripts.ScriptBundleManifest { Path = "~/path" };
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

    public class BundleManifest_SerializeToXElement_Tests
    {
        readonly TestableBundleManifest manifest;
        readonly XElement element;

        class TestableBundleManifest : BundleManifest
        {
            protected override Bundle CreateBundleCore()
            {
                throw new System.NotImplementedException();
            }
        }

        public BundleManifest_SerializeToXElement_Tests()
        {
            manifest = new TestableBundleManifest
            {
                Path = "~",
                Hash = new byte[] { 1, 2, 3 },
                ContentType = "content-type",
                PageLocation = "page-location",
                Assets =
                    {
                        new AssetManifest
                        {
                            Path = "~/asset",
                            RawFileReferences =
                                {
                                    "~/raw-file/reference"
                                }
                        }
                    },
                References =
                    {
                        "~/bundle-reference"
                    }
            };

            element = manifest.SerializeToXElement();
        }

        [Fact]
        public void PathAttributeEqualsManifestPath()
        {
            element.Attribute("Path").Value.ShouldEqual(manifest.Path);
        }

        [Fact]
        public void HashAttributeEqualsHexStringOfManifestHash()
        {
            element.Attribute("Hash").Value.ShouldEqual("010203");
        }

        [Fact]
        public void ContentTypeAttributeEqualsManifestContentType()
        {
            element.Attribute("ContentType").Value.ShouldEqual(manifest.ContentType);            
        }

        [Fact]
        public void PageLocationAttributeEqualsManifestPageLocation()
        {
            element.Attribute("PageLocation").Value.ShouldEqual(manifest.PageLocation);
        }

        [Fact]
        public void ElementHasAssetChildElement()
        {
            element.Elements("Asset").Count().ShouldEqual(1);
        }

        [Fact]
        public void ElementHasReferenceChildElement()
        {
            element.Elements("Reference").Count().ShouldEqual(1);
        }
    }
}