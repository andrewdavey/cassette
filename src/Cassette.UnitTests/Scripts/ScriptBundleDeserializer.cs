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
                    new XAttribute("Name", "type"),
                    new XAttribute("Value", "text/javascript")
                ),
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
                { "~/script/AQID.js", "CONTENT" } // "AQID" is base64 of {1,2,3}
            };
            var urlModifier = new VirtualDirectoryPrepender("/");
            deserializer = new ScriptBundleDeserializer(urlModifier);

            DeserializeBundle();
        }

        [Fact]
        public void ReadBundlePathEqualsPathAttibute()
        {
            bundle.Path.ShouldEqual("~");
        }

        [Fact]
        public void ThrowsExceptionWhenPathAttributeMissing()
        {
            element.SetAttributeValue("Path", null);
            DeserializeThrowsInvalidCassetteBundleException();
        }

        [Fact]
        public void ReadBundleHashEqualsHashAttribute()
        {
            bundle.Hash.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void GivenNoHashAttributeThenThrowInvalidCassetteBundleException()
        {
            element.SetAttributeValue("Hash", null);
            DeserializeThrowsInvalidCassetteBundleException();
        }

        [Fact]
        public void GivenWrongLengthHashHexStringThenThrowInvalidCassetteBundleException()
        {
            element.SetAttributeValue("Hash", "012");
            DeserializeThrowsInvalidCassetteBundleException();
        }

        [Fact]
        public void GivenInvalidHashHexStringThenThrowInvalidCassetteBundleException()
        {
            element.SetAttributeValue("Hash", "qq");
            DeserializeThrowsInvalidCassetteBundleException();
        }

        [Fact]
        public void BundleConditionEqualsConditionAttribute()
        {
            bundle.Condition.ShouldEqual("expected-condition");
        }

        [Fact]
        public void BundleConditionIsNulIfConditionAttributeMissing()
        {
            element.SetAttributeValue("Condition", null);
            DeserializeBundle();
            bundle.Condition.ShouldBeNull();
        }

        [Fact]
        public void BundleContentTypeEqualsContentTypeAttribute()
        {
            bundle.ContentType.ShouldEqual("text/javascript");
        }

        [Fact]
        public void GivenNoContentTypeAttributeThenBundleContentTypeIsDefault()
        {
            element.SetAttributeValue("ContentType", null);
            DeserializeBundle();
            bundle.ContentType.ShouldEqual("text/javascript");
        }

        [Fact]
        public void BundlePageLocationEqualsPageLocationAttribute()
        {
            bundle.PageLocation.ShouldEqual("PAGE-LOCATION");
        }

        [Fact]
        public void GivenNoPageLocationAttributeThenBundlePageLocationIsNull()
        {
            element.SetAttributeValue("PageLocation", null);
            DeserializeBundle();
            bundle.PageLocation.ShouldBeNull();
        }

        [Fact]
        public void ReadBundleAssetCountEqualsAssetElementCount()
        {
            var asset = bundle.Assets[0].ShouldBeType<CachedBundleContent>();
            asset.OriginalAssets.Count().ShouldEqual(2);
        }

        [Fact]
        public void ReadBundleReferencesEqualReferenceElements()
        {
            var references = bundle.References.ToArray();
            references.ShouldEqual(new[] { "~/reference-1", "~/reference-2" });
        }

        [Fact]
        public void ReadBundleHas3HtmlAttributes()
        {
            bundle.HtmlAttributes.Count.ShouldEqual(3);
        }

        [Fact]
        public void ReadBundleHtmlAttributeTypeEqualsTextJavaScript()
        {
            bundle.HtmlAttributes["type"].ShouldEqual("text/javascript");
        }

        [Fact]
        public void ReadBundleContentEqualsBase64DecodedContentElement()
        {
            bundle.OpenStream().ReadToEnd().ShouldEqual("CONTENT");
        }

        void DeserializeBundle()
        {
            bundle = deserializer.Deserialize(element, directory);
        }

        void DeserializeThrowsInvalidCassetteBundleException()
        {
            Assert.Throws<CassetteDeserializationException>(() => DeserializeBundle());
        }
    }
}