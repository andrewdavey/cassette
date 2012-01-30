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
                Media = "MEDIA"
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

        void WriteToElement()
        {
            var container = new XDocument();
            var writer = new StylesheetBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }
    }
}