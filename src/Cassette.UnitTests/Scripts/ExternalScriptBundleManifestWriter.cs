using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleManifestWriter_Tests
    {
        readonly ExternalScriptBundleManifest manifest;
        XElement element;

        public ExternalScriptBundleManifestWriter_Tests()
        {
            manifest = new ExternalScriptBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Url = "http://example.com/",
                FallbackCondition = "condition"
            };

            WriteToElement();
        }

        [Fact]  
        public void UrlAttributeEqualsManifestUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(manifest.Url);
        }

        [Fact]
        public void FallbackConditionAttributeEqualsManifestFallbackCondition()
        {
            element.Attribute("FallbackCondition").Value.ShouldEqual(manifest.FallbackCondition);
        }

        [Fact]
        public void GivenManifestFallbackConditionIsNullThenElementHasNoFallbackConditionAttribute()
        {
            manifest.FallbackCondition = null;
            WriteToElement();
            element.Attribute("FallbackCondition").ShouldBeNull();
        }

        void WriteToElement()
        {
            var container = new XDocument();
            var writer = new ExternalScriptBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }
    }
}
