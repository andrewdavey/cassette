using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleDeserializer_Tests
    {
        readonly ExternalScriptBundleDeserializer reader;
        readonly XElement element;
        ExternalScriptBundle bundle;
        readonly FakeFileSystem directory;

        public ExternalScriptBundleDeserializer_Tests()
        {
            element = new XElement(
                "ExternalScriptBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("FallbackCondition", "CONDITION")
            );
            directory = new FakeFileSystem
            {
                { "~/script/010203.js", "content" }
            };
            var urlModifier = new VirtualDirectoryPrepender("/");
            reader = new ExternalScriptBundleDeserializer(directory, urlModifier);
            DeserializeToBundle();
        }

        [Fact]
        public void BundleExternalUrlEqualsUrlAttribute()
        {
            bundle.ExternalUrl.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void ThrowsExceptionWhenUrlAttributeIsMissing()
        {
            element.SetAttributeValue("Url", null);
            Assert.Throws<CassetteDeserializationException>(
                () => DeserializeToBundle()
            );
        }

        [Fact]
        public void BundleFallbackConditionEqualsFallbackConditionAttribute()
        {
            bundle.FallbackCondition.ShouldEqual("CONDITION");
        }

        [Fact]
        public void BundleFallbackConditionIsNullWhenAttributeMissing()
        {
            element.SetAttributeValue("FallbackCondition", null);
            DeserializeToBundle();
            bundle.FallbackCondition.ShouldBeNull();
        }

        void DeserializeToBundle()
        {
            bundle = reader.Deserialize(element, directory);
        }
    }
}