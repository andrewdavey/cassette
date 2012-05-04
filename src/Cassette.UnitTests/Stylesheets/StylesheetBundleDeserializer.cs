using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleDeserializer_Tests
    {
        readonly StylesheetBundleDeserializer reader;
        readonly XElement element;
        StylesheetBundle bundle;
        readonly FakeFileSystem directory;

        public StylesheetBundleDeserializer_Tests()
        {
            element = new XElement(
                "StylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("Media", "expected-media"),
                new XAttribute("Condition", "expected-condition")
            );
            directory = new FakeFileSystem
            {
                { "~/010203.css", "content" }
            };
            var urlModifier = new VirtualDirectoryPrepender("/");
            reader = new StylesheetBundleDeserializer(urlModifier);
            DeserializeBundle();
        }

        [Fact]
        public void DeserializedBundleMediaEqualsMediaAttribute()
        {
            bundle.Media.ShouldEqual("expected-media");
        }

        [Fact]
        public void DeserializedBundleMediaIsNullIfMediaAttributeMissing()
        {
            element.SetAttributeValue("Media", null);
            DeserializeBundle();
            bundle.Media.ShouldBeNull();
        }

        [Fact]
        public void DeserializedBundleConditionEqualsConditionAttribute()
        {
            bundle.Condition.ShouldEqual("expected-condition");
        }

        [Fact]
        public void DeserializedBundleConditionIsNullIfConditionAttributeMissing()
        {
            element.SetAttributeValue("Condition", null);
            DeserializeBundle();
            bundle.Condition.ShouldBeNull();
        }

        void DeserializeBundle()
        {
            bundle = reader.Deserialize(element, directory);
        }
    }
}