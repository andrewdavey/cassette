using System.Xml.Linq;
using Cassette.TinyIoC;
using Moq;
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
                new XAttribute("FallbackCondition", "CONDITION"),
                new XAttribute("Renderer", typeof(ExternalScriptBundle.ExternalScriptBundleRenderer).AssemblyQualifiedName),
                new XAttribute("FallbackRenderer", typeof(ScriptBundleHtmlRenderer).AssemblyQualifiedName)
            );
            directory = new FakeFileSystem
            {
                { "~/script/010203.js", "content" }
            };
            var container = new TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());
            reader = new ExternalScriptBundleDeserializer(container);
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

        [Fact]
        public void BundleFallbackRendererIsAssigned()
        {
            bundle.FallbackRenderer.ShouldNotBeNull();
        }

        void DeserializeToBundle()
        {
            bundle = reader.Deserialize(element, directory);
        }
    }
}