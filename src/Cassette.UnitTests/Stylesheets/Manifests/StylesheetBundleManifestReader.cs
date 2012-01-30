using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Stylesheets.Manifests
{
    public class StylesheetBundleManifestReader_Tests
    {
        readonly XElement element;
        StylesheetBundleManifest readManifest;

        public StylesheetBundleManifestReader_Tests()
        {
            element = new XElement(
                "StylesheetBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", ""),
                new XAttribute("Media", "expected-media")
            );
            ReadManifestFromElement();
        }

        [Fact]
        public void ReadManifestMediaEqualsMediaAttribute()
        {
            readManifest.Media.ShouldEqual("expected-media");
        }

        [Fact]
        public void ReadManifestMediaIsNullIfMediaAttributeMissing()
        {
            element.SetAttributeValue("Media", null);
            ReadManifestFromElement();
            readManifest.Media.ShouldBeNull();
        }

        void ReadManifestFromElement()
        {
            var reader = new StylesheetBundleManifestReader(element);
            readManifest = reader.Read();
        }
    }
}