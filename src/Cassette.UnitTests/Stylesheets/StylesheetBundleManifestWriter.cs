using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class StylesheetBundleManifestWriter_Tests
    {
        readonly StylesheetBundleManifest manifest;
        readonly XElement element;

        public StylesheetBundleManifestWriter_Tests()
        {
            manifest = new StylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Media = "MEDIA"
            };

            var container = new XDocument();
            var writer = new StylesheetBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }

        [Fact]
        public void MediaAttributeEqualsManifestMedia()
        {
            element.Attribute("Media").Value.ShouldEqual(manifest.Media);
        }
    }
}