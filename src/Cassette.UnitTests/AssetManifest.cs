using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette
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

    public class AssetManifest_SerializeToXElement_Tests
    {
        readonly AssetManifest manifest;
        readonly XElement element;

        public AssetManifest_SerializeToXElement_Tests()
        {
            manifest = new AssetManifest
            {
                Path = "~/path",
                RawFileReferences = { "~/reference" }
            };

            element = manifest.SerializeToXElement();
        }

        [Fact]
        public void ElementNameIsAsset()
        {
            element.Name.LocalName.ShouldEqual("Asset");            
        }

        [Fact]
        public void PathAttributeEqualsManifestPath()
        {
            element.Attribute("Path").Value.ShouldEqual(manifest.Path);            
        }

        [Fact]
        public void RawFileReferencePathEqualsManifestRawFileReference()
        {
            element.Element("RawFileReference").Attribute("Path").Value.ShouldEqual(manifest.RawFileReferences[0]);            
        }
    }
}