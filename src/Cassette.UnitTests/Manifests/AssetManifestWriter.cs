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
                References =
                    {
                        new AssetReferenceManifest
                        {
                            Path = "~/bundle",
                            Type = AssetReferenceType.DifferentBundle,
                            SourceLineNumber = 1
                        },
                        new AssetReferenceManifest
                        {
                            Path = "~/image.png",
                            Type = AssetReferenceType.RawFilename,
                            SourceLineNumber = 2
                        },
                        new AssetReferenceManifest
                        {
                            Path = "http://example.com/",
                            Type = AssetReferenceType.Url,
                            SourceLineNumber = 3
                        }
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