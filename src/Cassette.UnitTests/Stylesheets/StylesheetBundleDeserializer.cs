using System.Xml.Linq;
using Cassette.TinyIoC;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleDeserializer_Tests
    {
        readonly StylesheetBundleDeserializer reader;
        readonly XElement element;
        readonly FakeFileSystem directory;
        readonly XElement mediaElement;
        StylesheetBundle bundle;

        public StylesheetBundleDeserializer_Tests()
        {
            mediaElement = new XElement("HtmlAttribute", new XAttribute("Name", "media"), new XAttribute("Value", "expected-media"));
            element = new XElement(
                "StylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("Condition", "expected-condition"),
                new XAttribute("Renderer", typeof(StylesheetHtmlRenderer).AssemblyQualifiedName),
                mediaElement
            );
            directory = new FakeFileSystem
            {
                { "~/stylesheet/010203.css", "content" }
            };
            var container = new TinyIoCContainer();
            container.Register(Mock.Of<IUrlGenerator>());
            reader = new StylesheetBundleDeserializer(container);
            DeserializeBundle();
        }

        [Fact]
        public void DeserializedBundleMediaEqualsMediaAttribute()
        {
            bundle.Media.ShouldEqual("expected-media");
        }

        [Fact]
        public void DeserializedBundleMediaIsNullIfMediaHtmlAttributeMissing()
        {
            mediaElement.Remove();
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