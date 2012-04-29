using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
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
                Condition = "CONDITION"
            };
            
            SerializeToElement();
        }

        [Fact]  
        public void UrlAttributeEqualsManifestUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(bundle.Url);
        }

        [Fact]
        public void ConditionAttributeEqualsManifestCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(bundle.Condition);
        }

        [Fact]
        public void MediaAttributeEqualsManifestMedia()
        {
            element.Attribute("Media").Value.ShouldEqual(bundle.Media);
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