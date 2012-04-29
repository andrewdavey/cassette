using System.Xml.Linq;
using Cassette.Manifests;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ExternalScriptBundleDeserializer_Tests
    {
        readonly ExternalScriptBundleDeserializer reader;
        readonly XElement element;
        ExternalScriptBundle bundle;

        public ExternalScriptBundleDeserializer_Tests()
        {
            element = new XElement(
                "ExternalScriptBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("FallbackCondition", "CONDITION")
            );
            var directory = new FakeFileSystem();
            var urlModifier = new VirtualDirectoryPrepender("/");
            reader = new ExternalScriptBundleDeserializer(directory, urlModifier);
            ReadManifestFromElement();
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
                () => ReadManifestFromElement()
            );
        }

        [Fact]
        public void ReadManifestFallbackConditionEqualsFallbackConditionAttribute()
        {
            bundle.FallbackCondition.ShouldEqual("CONDITION");
        }

        [Fact]
        public void ReadManifestFallbackConditionIsNullWhenAttributeMissing()
        {
            element.SetAttributeValue("FallbackCondition", null);
            ReadManifestFromElement();
            bundle.FallbackCondition.ShouldBeNull();
        }

        void ReadManifestFromElement()
        {
            bundle = reader.Deserialize(element);
        }
    }
}