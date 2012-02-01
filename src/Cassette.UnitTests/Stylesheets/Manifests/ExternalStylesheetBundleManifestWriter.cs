using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class ExternalStylesheetBundleManifestWriter_Tests
    {
        readonly ExternalStylesheetBundleManifest manifest;
        XElement element;

        public ExternalStylesheetBundleManifestWriter_Tests()
        {
            manifest = new ExternalStylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Url = "http://example.com/",
                Media = "MEDIA",
                Condition = "CONDITION"
            };

            WriteToElement();
        }

        [Fact]  
        public void UrlAttributeEqualsManifestUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(manifest.Url);
        }

        [Fact]
        public void ConditionAttributeEqualsManifestCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(manifest.Condition);
        }

        [Fact]
        public void MediaAttributeEqualsManifestMedia()
        {
            element.Attribute("Media").Value.ShouldEqual(manifest.Media);
        }

        [Fact]
        public void GivenMediaIsNullThenElementHasNoMediaAttribute()
        {
            manifest.Media = null;
            WriteToElement();
            element.Attribute("Media").ShouldBeNull();
        }

        [Fact]
        public void GivenConditionIsNullThenElementHasNoConditionAttribute()
        {
            manifest.Condition = null;
            WriteToElement();
            element.Attribute("Condition").ShouldBeNull();
        }

        void WriteToElement()
        {
            var container = new XDocument();
            var writer = new ExternalStylesheetBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }
    }
}