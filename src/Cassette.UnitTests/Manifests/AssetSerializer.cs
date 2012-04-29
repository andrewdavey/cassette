using System.Linq;
using System.Xml.Linq;
using Moq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class AssetSerializer_Tests
    {
        readonly XElement containerElement;
        readonly XElement assetElement;
        readonly Mock<IAsset> asset;

        public AssetSerializer_Tests()
        {
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset");
            asset.SetupGet(a => a.References).Returns(new[]
            {
                new AssetReference("~/asset", "~/bundle", 1, AssetReferenceType.DifferentBundle),
                new AssetReference("~/asset", "~/image.png", 2, AssetReferenceType.RawFilename),
                new AssetReference("~/asset", "http://example.com/", 2, AssetReferenceType.Url)
            });

            containerElement = new XElement("Bundle");
            var serializer = new AssetSerializer(containerElement);
            serializer.Serialize(asset.Object);
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
            assetElement.Attribute("Path").Value.ShouldEqual("~/asset");
        }

        [Fact]
        public void AssetElementContainsReferenceElements()
        {
            var elements = assetElement.Elements("Reference");
            elements.Count().ShouldEqual(3);
        }

        [Fact]
        public void FirstReferenceElementEqualsFirstReferenceManifest()
        {
            var element = assetElement.Elements("Reference").ElementAt(0);
            element.Attribute("Path").Value.ShouldEqual("~/bundle");
            element.Attribute("Type").Value.ShouldEqual("DifferentBundle");
            element.Attribute("SourceLineNumber").Value.ShouldEqual("1");
        }

        [Fact]
        public void SecondReferenceElementEqualsSecondReferenceManifest()
        {
            var element = assetElement.Elements("Reference").ElementAt(1);
            element.Attribute("Path").Value.ShouldEqual("~/image.png");
            element.Attribute("Type").Value.ShouldEqual("RawFilename");
            element.Attribute("SourceLineNumber").Value.ShouldEqual("2");
        }

        [Fact]
        public void ThirdReferenceElementEqualsThirdReferenceManifest()
        {
            var element = assetElement.Elements("Reference").ElementAt(2);
            element.Attribute("Path").Value.ShouldEqual("http://example.com/");
            element.Attribute("Type").Value.ShouldEqual("Url");
            element.Attribute("SourceLineNumber").Value.ShouldEqual("3");
        }
    }
}