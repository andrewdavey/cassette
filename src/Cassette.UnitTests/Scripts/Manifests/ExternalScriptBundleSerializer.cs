using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ExternalScriptBundleSerializer_Tests
    {
        ExternalScriptBundle bundle;
        XElement element;

        public ExternalScriptBundleSerializer_Tests()
        {
            bundle = new ExternalScriptBundle("http://example.com/", "~", "condition")
            {
                Hash = new byte[0]
            };

            SerializeToElement();
        }

        [Fact]  
        public void UrlAttributeEqualsManifestUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(bundle.Url);
        }

        [Fact]
        public void FallbackConditionAttributeEqualsManifestFallbackCondition()
        {
            element.Attribute("FallbackCondition").Value.ShouldEqual(bundle.FallbackCondition);
        }

        [Fact]
        public void GivenManifestFallbackConditionIsNullThenElementHasNoFallbackConditionAttribute()
        {
            bundle = new ExternalScriptBundle("http://example.com/")
            {
                Hash = new byte[0]
            };
            SerializeToElement();
            element.Attribute("FallbackCondition").ShouldBeNull();
        }

        void SerializeToElement()
        {
            var container = new XDocument();
            var writer = new ExternalScriptBundleSerializer(container);
            writer.Serialize(bundle);
            element = container.Root;
        }
    }
}