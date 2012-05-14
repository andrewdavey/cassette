using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExternalStylesheetBundleSerializer_Tests
    {
        readonly ExternalStylesheetBundle bundle;
        XElement element;

        public ExternalStylesheetBundleSerializer_Tests()
        {
            bundle = new ExternalStylesheetBundle("http://example.com/", "~")
            {
                Hash = new byte[0],
                Media = "MEDIA",
                Condition = "CONDITION",
                Renderer = new ConstantHtmlRenderer<StylesheetBundle>("", new VirtualDirectoryPrepender("/"))
            };
            
            SerializeToElement();
        }

        [Fact]  
        public void UrlAttributeEqualsBundleExternalUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(bundle.ExternalUrl);
        }

        [Fact]
        public void ConditionAttributeEqualsBundleCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(bundle.Condition);
        }

        [Fact]
        public void MediaHtmlAttributeEqualsBundleMedia()
        {
            var media = element
                .Elements("HtmlAttribute")
                .First(e => e.Attribute("Name").Value == "media")
                .Attribute("Value").Value;
            media.ShouldEqual(bundle.Media);
        }

        [Fact]
        public void GivenMediaIsNullThenElementHasNoMediaAttribute()
        {
            bundle.Media = null;
            SerializeToElement();
            element.Attribute("Media").ShouldBeNull();
        }

        [Fact]
        public void GivenConditionIsNullThenElementHasNoConditionAttribute()
        {
            bundle.Condition = null;
            SerializeToElement();
            element.Attribute("Condition").ShouldBeNull();
        }

        void SerializeToElement()
        {
            var container = new XDocument();
            var writer = new ExternalStylesheetBundleSerializer(container);
            writer.Serialize(bundle);
            element = container.Root;
        }
    }
}