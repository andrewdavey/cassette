using System.Xml.Linq;
using Cassette.Manifests;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class ExternalStylesheetBundleDeserializer_Tests
    {
        readonly ExternalStylesheetBundleDeserializer reader;
        readonly XElement element;
        ExternalStylesheetBundle bundle;

        public ExternalStylesheetBundleDeserializer_Tests()
        {
            element = new XElement(
                "ExternalStylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("Media", "MEDIA"),
                new XAttribute("Condition", "CONDITION")
            );
            var directory = new FakeFileSystem();
            var urlModifier = new VirtualDirectoryPrepender("/");
            
            reader = new ExternalStylesheetBundleDeserializer(directory, urlModifier);

            DeserializeElement();
        }

        [Fact]
        public void ReadManifestUrlEqualsUrlAttribute()
        {
            bundle.Url.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void ThrowsExceptionWhenUrlAttributeIsMissing()
        {
            element.SetAttributeValue("Url", null);
            Assert.Throws<CassetteDeserializationException>(
                () => DeserializeElement()
            );
        }

        [Fact]
        public void ReadManifestMediaEqualsMediaAttribute()
        {
            bundle.Media.ShouldEqual("MEDIA");
        }

        [Fact]
        public void ReadManifestConditionEqualsConditionAttribute()
        {
            bundle.Condition.ShouldEqual("CONDITION");
        }

        void DeserializeElement()
        {
            bundle = reader.Deserialize(element);
        }
    }
}