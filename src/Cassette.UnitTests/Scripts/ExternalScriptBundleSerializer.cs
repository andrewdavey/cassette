using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Scripts
{
    public class ExternalScriptBundleSerializer_Tests
    {
        ExternalScriptBundle bundle;
        XElement element;

        public ExternalScriptBundleSerializer_Tests()
        {
            var prepender = new VirtualDirectoryPrepender("/");
            bundle = new ExternalScriptBundle("http://example.com/", "~", "condition")
            {
                Hash = new byte[0],
                Renderer = new ConstantHtmlRenderer<ScriptBundle>("", prepender, prepender)
            };

            SerializeToElement();
        }

        [Fact]
        public void UrlAttributeEqualsManifestExternalUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(bundle.ExternalUrl);
        }

        [Fact]
        public void FallbackConditionAttributeEqualsManifestFallbackCondition()
        {
            element.Attribute("FallbackCondition").Value.ShouldEqual(bundle.FallbackCondition);
        }

        [Fact]
        public void GivenManifestFallbackConditionIsNullThenElementHasNoFallbackConditionAttribute()
        {
            var urlModifier = new VirtualDirectoryPrepender("/");
            bundle = new ExternalScriptBundle("http://example.com/")
            {
                Hash = new byte[0],
                Renderer = new ConstantHtmlRenderer<ScriptBundle>("", urlModifier, urlModifier)
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