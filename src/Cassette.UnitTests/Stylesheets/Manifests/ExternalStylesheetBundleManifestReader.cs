using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class ExternalStylesheetBundleManifestReader_Tests
    {
        readonly XElement element;
        ExternalStylesheetBundleManifest readManifest;

        public ExternalStylesheetBundleManifestReader_Tests()
        {
            element = new XElement(
                "ExternalStylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Url", "http://example.com/"),
                new XAttribute("Media", "MEDIA")
            );
            ReadManifestFromElement();
        }

        [Fact]
        public void ReadManifestUrlEqualsUrlAttribute()
        {
            readManifest.Url.ShouldEqual("http://example.com/");
        }

        [Fact]
        public void ThrowsExceptionWhenUrlAttributeIsMissing()
        {
            element.SetAttributeValue("Url", null);
            Assert.Throws<InvalidBundleManifestException>(
                () => ReadManifestFromElement()
            );
        }

        [Fact]
        public void ReadManifestMediaEqualsMediaAttribute()
        {
            readManifest.Media.ShouldEqual("MEDIA");
        }

        void ReadManifestFromElement()
        {
            var reader = new ExternalStylesheetBundleManifestReader(element);
            readManifest = reader.Read();
        }
    }
}