using System.Linq;
using System.Xml.Linq;
using Cassette.Manifests;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ScriptBundleDeserializer_Tests
    {
        readonly ScriptBundleDeserializer deserializer;
        readonly XElement element;
        ScriptBundle bundle;

        public ScriptBundleDeserializer_Tests()
        {
            element = new XElement("ScriptBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("Condition", "expected-condition"),
                new XAttribute("ContentType", "CONTENT-TYPE"),
                new XAttribute("PageLocation", "PAGE-LOCATION"),
                new XElement("Asset", new XAttribute("Path", "~/asset-1")),
                new XElement("Asset", new XAttribute("Path", "~/asset-2")),
                new XElement("Reference", new XAttribute("Path", "~/reference-1")),
                new XElement("Reference", new XAttribute("Path", "~/reference-2")),
                new XElement(
                    "HtmlAttribute", 
                    new XAttribute("Name", "attribute1"),
                    new XAttribute("Value", "value1")
                ),
                new XElement(
                    "HtmlAttribute",
                    new XAttribute("Name", "attribute2"),
                    new XAttribute("Value", "value2")
                )
            );

            var directory = new FakeFileSystem
            {
                { "010203", "CONTENT" }
            };
            var urlModifier = new VirtualDirectoryPrepender("/");
            deserializer = new ScriptBundleDeserializer(directory, urlModifier);

            ReadBundleManifest();
        }

        [Fact]
        public void ReadManifestPathEqualsPathAttibute()
        {
            bundle.Path.ShouldEqual("~");
        }

        [Fact]
        public void ThrowsExceptionWhenPathAttributeMissing()
        {
            element.SetAttributeValue("Path", null);
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void ReadManifestHashEqualsHashAttribute()
        {
            bundle.Hash.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void GivenNoHashAttributeThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", null);
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void GivenWrongLengthHashHexStringThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", "012");
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void GivenInvalidHashHexStringThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", "qq");
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void ManifestConditionEqualsConditionAttribute()
        {
            bundle.Condition.ShouldEqual("expected-condition");
        }

        [Fact]
        public void ManifestConditionIsNulIfConditionAttributeMissing()
        {
            element.SetAttributeValue("Condition", null);
            ReadBundleManifest();
            bundle.Condition.ShouldBeNull();
        }

        [Fact]
        public void ManifestContentTypeEqualsContentTypeAttribute()
        {
            bundle.ContentType.ShouldEqual("CONTENT-TYPE");
        }

        [Fact]
        public void GivenNoContentTypeAttributeThenManifestContentTypeIsNull()
        {
            element.SetAttributeValue("ContentType", null);
            ReadBundleManifest();
            bundle.ContentType.ShouldBeNull();
        }

        [Fact]
        public void ManifestPageLocationEqualsPageLocationAttribute()
        {
            bundle.PageLocation.ShouldEqual("PAGE-LOCATION");
        }

        [Fact]
        public void GivenNoPageLocationAttributeThenManifestPageLocationIsNull()
        {
            element.SetAttributeValue("PageLocation", null);
            ReadBundleManifest();
            bundle.PageLocation.ShouldBeNull();
        }

        [Fact]
        public void ReadManifestAssetCountEqualsAssetElementCount()
        {
            bundle.Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void ReadManifestReferencesEqualReferenceElements()
        {
            var references = bundle.References.ToArray();
            references.ShouldEqual(new[] { "~/reference-1", "~/reference-2" });
        }

        [Fact]
        public void ReadManifestHasTwoHtmlAttributes()
        {
            bundle.HtmlAttributes.Count.ShouldEqual(2);
        }

        [Fact]
        public void ReadManifestContentEqualsBase64DecodedContentElement()
        {
            bundle.OpenStream().ReadToEnd().ShouldEqual("CONTENT");
        }

        void ReadBundleManifest()
        {
            bundle = deserializer.Deserialize(element);
        }

        void ReadBundleManifestThrowsInvalidCassetteManifestException()
        {
            Assert.Throws<CassetteDeserializationException>(() => ReadBundleManifest());
        }
    }
}