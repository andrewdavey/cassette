using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class StylesheetBundleManifestWriter_Tests
    {
        readonly StylesheetBundleManifest manifest;
        XElement element;

        public StylesheetBundleManifestWriter_Tests()
        {
            manifest = new StylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Media = "MEDIA",
                Condition = "CONDITION"
            };

            WriteToElement();
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
        public void ConditionAttributeEqualsManifestCondition()
        {
            element.Attribute("Condition").Value.ShouldEqual(manifest.Condition);
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
            var writer = new StylesheetBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }
    }
}