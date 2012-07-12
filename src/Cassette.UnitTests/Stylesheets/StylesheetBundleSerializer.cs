using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleSerializer_Tests
    {
        readonly StylesheetBundle bundle;
        XElement element;

        public StylesheetBundleSerializer_Tests()
        {
            var prepender = new VirtualDirectoryPrepender("/");
            bundle = new StylesheetBundle("~")
            {
                Hash = new byte[0],
                Media = "MEDIA",
                Condition = "CONDITION",
                Renderer = new ConstantHtmlRenderer<StylesheetBundle>("", prepender, prepender)
            };

            WriteToElement();
        }

        [Fact]
        public void MediaAttributeEqualsBundleMedia()
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
            WriteToElement();
            element.Attribute("Media").ShouldBeNull();
        }

        [Fact]
        public void ConditionAttributeEqualsBundleCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(bundle.Condition);
        }

        [Fact]
        public void GivenConditionIsNullThenElementHasNoConditionAttribute()
        {
            bundle.Condition = null;
            WriteToElement();
            element.Attribute("Condition").ShouldBeNull();
        }

        void WriteToElement()
        {
            var container = new XDocument();
            var writer = new StylesheetBundleSerializer(container);
            writer.Serialize(bundle);
            element = container.Root;
        }
    }
}