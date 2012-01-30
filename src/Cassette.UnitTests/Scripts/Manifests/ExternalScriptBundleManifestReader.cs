using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ExternalScriptBundleManifestReader_Tests
    {
        readonly XElement element;
        ExternalScriptBundleManifest readManifest;

        public ExternalScriptBundleManifestReader_Tests()
        {
            element = new XElement(
                "ExternalScriptBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("FallbackCondition", "CONDITION")
            );
            ReadManifestFromElement();
        }

        [Fact]
        public void ReadManifestUrlEqualsUrlAttribute()
        {
            readManifest.Url.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void ThrowsExceptionWhenUrlAttributeIsMissing()
        {
            element.SetAttributeValue("Url", null);
            Assert.Throws<InvalidBundleManifestException>(
                () => ReadManifestFromElement()
            );
        }

        [Fact]
        public void ReadManifestFallbackConditionEqualsFallbackConditionAttribute()
        {
            readManifest.FallbackCondition.ShouldEqual("CONDITION");
        }

        [Fact]
        public void ReadManifestFallbackConditionIsNullWhenAttributeMissing()
        {
            element.SetAttributeValue("FallbackCondition", null);
            ReadManifestFromElement();
            readManifest.FallbackCondition.ShouldBeNull();
        }

        void ReadManifestFromElement()
        {
            var reader = new ExternalScriptBundleManifestReader(element);
            readManifest = reader.Read();
        }
    }
}