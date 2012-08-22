using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetBundleDeserializer_Tests
    {
        readonly ExternalStylesheetBundleDeserializer reader;
        readonly XElement element;
        readonly FakeFileSystem directory;
        ExternalStylesheetBundle bundle;

        public ExternalStylesheetBundleDeserializer_Tests()
        {
            element = new XElement(
                "ExternalStylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("Condition", "CONDITION"),
                new XElement("HtmlAttribute", new XAttribute("Name", "media"), new XAttribute("Value", "MEDIA"))
            );
            directory = new FakeFileSystem
            {
                { "~/stylesheet/010203.css", "content"}
            };
            var urlModifier = new VirtualDirectoryPrepender("/");

            reader = new ExternalStylesheetBundleDeserializer(urlModifier, urlModifier);

            DeserializeElement();
        }

        [Fact]
        public void DeserializedBundleExternalUrlEqualsUrlAttribute()
        {
            bundle.ExternalUrl.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void ThrowsExceptionWhenUrlAttributeIsMissing()
        {
            element.SetAttributeValue("Url", null);
            Assert.Throws<CassetteDeserializationException>(
                () => DeserializeElement()
            );
        }

        [Fact]
        public void DeserializedBundleMediaEqualsMediaAttribute()
        {
            bundle.Media.ShouldEqual("MEDIA");
        }

        [Fact]
        public void DeserializedBundleConditionEqualsConditionAttribute()
        {
            bundle.Condition.ShouldEqual("CONDITION");
        }

        void DeserializeElement()
        {
            bundle = reader.Deserialize(element, directory);
        }
    }
}