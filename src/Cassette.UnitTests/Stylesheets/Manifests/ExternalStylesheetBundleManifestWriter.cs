using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class ExternalStylesheetBundleManifestWriter_Tests
    {
        readonly ExternalStylesheetBundleManifest manifest;
        readonly XElement element;

        public ExternalStylesheetBundleManifestWriter_Tests()
        {
            manifest = new ExternalStylesheetBundleManifest
            {
                Path = "~",
                Hash = new byte[0],
                Url = "http://example.com/",
                Media = "MEDIA"
            };

            var container = new XDocument();
            var writer = new ExternalStylesheetBundleManifestWriter(container);
            writer.Write(manifest);
            element = container.Root;
        }

        [Fact]  
        public void UrlAttributeEqualsManifestUrl()
        {
            element.Attribute("Url").Value.ShouldEqual(manifest.Url);
        }
    }
}
