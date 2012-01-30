using System.Xml.Linq;
using Cassette.Manifests;
using Should;
using Xunit;

namespace Cassette.Scripts.Manifests
{
    public class ScriptBundleManifestReader_Tests
    {
        BundleManifest readManifest;
        readonly XElement element;

        public ScriptBundleManifestReader_Tests()
        {
            element = new XElement("ScriptBundle",
                new XAttribute("Path", "~"),
                new XAttribute("Hash", "010203"),
                new XAttribute("ContentType", "CONTENT-TYPE"),
                new XAttribute("PageLocation", "PAGE-LOCATION"),
                new XElement("Asset", new XAttribute("Path", "~/asset-1")),
                new XElement("Asset", new XAttribute("Path", "~/asset-2")),
                new XElement("Reference", new XAttribute("Path", "~/reference-1")),
                new XElement("Reference", new XAttribute("Path", "~/reference-2"))
            );
            ReadBundleManifest();
        }

        [Fact]
        public void ReadManifestPathEqualsPathAttibute()
        {
            readManifest.Path.ShouldEqual("~");
        }

        [Fact]
        public void ThrowsExceptionWhenPathAttributeMissing()
        {
            element.SetAttributeValue("Path", null);
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void ReadManifestHashEqualsHashAttribute()
        {
            readManifest.Hash.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void GivenNoHashAttributeThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", null);
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void GivenWrongLengthHashHexStringThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", "012");
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void GivenInvalidHashHexStringThenThrowInvalidCassetteManifestException()
        {
            element.SetAttributeValue("Hash", "qq");
            ReadBundleManifestThrowsInvalidCassetteManifestException();
        }

        [Fact]
        public void ManifestContentTypeEqualsContentTypeAttribute()
        {
            readManifest.ContentType.ShouldEqual("CONTENT-TYPE");
        }

        [Fact]
        public void GivenNoContentTypeAttributeThenManifestContentTypeIsNull()
        {
            element.SetAttributeValue("ContentType", null);
            ReadBundleManifest();
            readManifest.ContentType.ShouldBeNull();
        }

        [Fact]
        public void ManifestPageLocationEqualsPageLocationAttribute()
        {
            readManifest.PageLocation.ShouldEqual("PAGE-LOCATION");
        }

        [Fact]
        public void GivenNoPageLocationAttributeThenManifestPageLocationIsNull()
        {
            element.SetAttributeValue("PageLocation", null);
            ReadBundleManifest();
            readManifest.PageLocation.ShouldBeNull();
        }

        [Fact]
        public void ReadManifestAssetCountEqualsAssetElementCount()
        {
            readManifest.Assets.Count.ShouldEqual(2);
        }

        [Fact]
        public void ReadManifestReferencesEqualReferenceElements()
        {
            readManifest.References.Count.ShouldEqual(2);
            readManifest.References[0].ShouldEqual("~/reference-1");
            readManifest.References[1].ShouldEqual("~/reference-2");
        }

        void ReadBundleManifest()
        {
            var reader = new ScriptBundleManifestReader(element);
            readManifest = reader.Read();
        }

        void ReadBundleManifestThrowsInvalidCassetteManifestException()
        {
            Assert.Throws<InvalidCassetteManifestException>(() => ReadBundleManifest());
        }
    }
}