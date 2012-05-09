using System.Linq;
using System.Xml.Linq;
using Cassette.Caching;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ScriptBundleDeserializer_Tests
    {
        readonly ScriptBundleDeserializer deserializer;
        readonly XElement element;
        ScriptBundle bundle;
        readonly FakeFileSystem directory;

        public ScriptBundleDeserializer_Tests()
        {
            element = new XElement("ScriptBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("Condition", "expected-condition"),
                new XAttribute("ContentType", "text/javascript"),
                new XAttribute("PageLocation", "PAGE-LOCATION"),
                new XElement("Asset", new XAttribute("Path", "~/asset-1"), new XAttribute("AssetCacheValidatorType", typeof(FileAssetCacheValidator).FullName)),
                new XElement("Asset", new XAttribute("Path", "~/asset-2"), new XAttribute("AssetCacheValidatorType", typeof(FileAssetCacheValidator).FullName)),
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

            directory = new FakeFileSystem
            {
                { "~/script/010203.js", "CONTENT" }
            };
            var urlModifier = new VirtualDirectoryPrepender("/");
            deserializer = new ScriptBundleDeserializer(urlModifier);

            DeserializeBundle();
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
            DeserializeThrowsInvalidCassetteManifestException();
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
            DeserializeThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void GivenWrongLengthHashHexStringThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", "012");
            DeserializeThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void GivenInvalidHashHexStringThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", "qq");
            DeserializeThrowsInvalidCassetteManifestException();
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
            DeserializeBundle();
            bundle.Condition.ShouldBeNull();
        }

        [Fact]
        public void ManifestContentTypeEqualsContentTypeAttribute()
        {
            bundle.ContentType.ShouldEqual("text/javascript");
        }

        [Fact]
        public void GivenNoContentTypeAttributeThenManifestContentTypeIsDefault()
        {
            element.SetAttributeValue("ContentType", null);
            DeserializeBundle();
            bundle.ContentType.ShouldEqual("text/javascript");
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
            DeserializeBundle();
            bundle.PageLocation.ShouldBeNull();
        }

        [Fact]
        public void ReadManifestAssetCountEqualsAssetElementCount()
        {
            var asset = bundle.Assets[0].ShouldBeType<CachedBundleContent>();
            asset.OriginalAssets.Count().ShouldEqual(2);
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

        void DeserializeBundle()
        {
            bundle = deserializer.Deserialize(element, directory);
        }

        void DeserializeThrowsInvalidCassetteManifestException()
        {
            Assert.Throws<CassetteDeserializationException>(() => DeserializeBundle());
        }
    }
}