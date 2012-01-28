using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette
{
    public class AssetManifestDeserializer_Tests
    {
        AssetManifest assetManifest;

        [Fact]
        public void AssetManifestPathInitializedFromXmlAttribute()
        {
            Deserialize("<Asset Path=\"~/asset/path\"/>");
            assetManifest.Path.ShouldEqual("~/asset/path");
        }

        [Fact]
        public void RawFileReferencesEmptyWhenXmlElementHasNoChildren()
        {
            Deserialize("<Asset Path=\"~/asset/path\"/>");
            assetManifest.RawFileReferences.ShouldBeEmpty();
        }

        [Fact]
        public void RawFileReferenceInitializedFromXmlChildElement()
        {
            Deserialize("<Asset Path=\"~\"><RawFileReference Path=\"~/EXPECTED-PATH\"/></Asset>");
            assetManifest.RawFileReferences[0].ShouldEqual("~/EXPECTED-PATH");
        }

        [Fact]
        public void TwoRawFileReferenceInitializedFromXmlChildElements()
        {
            Deserialize("<Asset Path=\"~\"><RawFileReference Path=\"~/EXPECTED-PATH-1\"/><RawFileReference Path=\"~/EXPECTED-PATH-2\"/></Asset>");
            assetManifest.RawFileReferences[0].ShouldEqual("~/EXPECTED-PATH-1");
            assetManifest.RawFileReferences[1].ShouldEqual("~/EXPECTED-PATH-2");
        }

        void Deserialize(string xml)
        {
            var element = XElement.Parse(xml);
            var deseralizer = new AssetManifestDeserializer();
            assetManifest = deseralizer.Deserialize(element);
        }
    }
}