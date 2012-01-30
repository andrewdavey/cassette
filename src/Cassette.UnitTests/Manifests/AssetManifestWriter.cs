using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class AssetManifestWriter_Tests
    {
        readonly XElement containerElement;
        readonly XElement assetElement;
        readonly AssetManifest assetManifest;

        public AssetManifestWriter_Tests()
        {
            containerElement = new XElement("Bundle");
            var writer = new AssetManifestWriter(containerElement);
            assetManifest = new AssetManifest
            {
                Path = "~/asset",
                RawFileReferences =
                    {
                        "~/file-1",
                        "~/file-2"
                    }
            };
            writer.Write(assetManifest);
            assetElement = containerElement.Elements("Asset").FirstOrDefault();
        }

        [Fact]
        public void ContainerElementContainsAssetElement()
        {
            assetElement.ShouldNotBeNull();
        }

        [Fact]
        public void AssetElementPathAttributeEqualsManifestPath()
        {
            assetElement.Attribute("Path").Value.ShouldEqual(assetManifest.Path);
        }

        [Fact]
        public void AssetElementContainsRawFileReferenceElements()
        {
            var elements = assetElement.Elements("RawFileReference").ToArray();
            elements[0].Attribute("Path").Value.ShouldEqual("~/file-1");
            elements[1].Attribute("Path").Value.ShouldEqual("~/file-2");
            elements.Length.ShouldEqual(2);
        }
    }
}