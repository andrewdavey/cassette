using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class StylesheetBundleDeserializer_Tests
    {
        readonly StylesheetBundleDeserializer reader;
        readonly XElement element;
        StylesheetBundle bundle;

        public StylesheetBundleDeserializer_Tests()
        {
            element = new XElement(
                "StylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Media", "expected-media"),
                new XAttribute("Condition", "expected-condition")
            );
            var directory = new FakeFileSystem();
            var urlModifier = new VirtualDirectoryPrepender("/");
            reader = new StylesheetBundleDeserializer(directory, urlModifier);
            DeserializeBundle();
        }

        [Fact]
        public void ReadManifestMediaEqualsMediaAttribute()
        {
            bundle.Media.ShouldEqual("expected-media");
        }

        [Fact]
        public void ReadManifestMediaIsNullIfMediaAttributeMissing()
        {
            element.SetAttributeValue("Media", null);
            DeserializeBundle();
            bundle.Media.ShouldBeNull();
        }

        [Fact]
        public void ReadManifestConditionEqualsConditionAttribute()
        {
            bundle.Condition.ShouldEqual("expected-condition");
        }

        [Fact]
        public void ReadManifestConditionIsNullIfConditionAttributeMissing()
        {
            element.SetAttributeValue("Condition", null);
            DeserializeBundle();
            bundle.Condition.ShouldBeNull();
        }

        void DeserializeBundle()
        {
            bundle = reader.Deserialize(element);
        }
    }
}