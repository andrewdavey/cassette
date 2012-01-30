using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class AssetManifestReader_Tests
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
            assetManifest.References.ShouldBeEmpty();
        }

        [Fact]
        public void RawFileReferenceInitializedFromXmlChildElement()
        {
            Deserialize("<Asset Path=\"~\"><Reference Type=\"RawFilename\" Path=\"~/EXPECTED-PATH\" SourceLineNumber=\"1\"/></Asset>");
            var reference = assetManifest.References[0];
            reference.Type.ShouldEqual(AssetReferenceType.RawFilename);
            reference.Path.ShouldEqual("~/EXPECTED-PATH");
            reference.SourceLineNumber.ShouldEqual(1);
        }

        [Fact]
        public void TwoRawFileReferenceInitializedFromXmlChildElements()
        {
            Deserialize("<Asset Path=\"~\">" +
                        "<Reference Type=\"RawFilename\" Path=\"~/EXPECTED-PATH-1\" SourceLineNumber=\"1\"/>" +
                        "<Reference Type=\"RawFilename\" Path=\"~/EXPECTED-PATH-2\" SourceLineNumber=\"2\"/>" +
                        "</Asset>");
            assetManifest.References[0].Path.ShouldEqual("~/EXPECTED-PATH-1");
            assetManifest.References[1].Path.ShouldEqual("~/EXPECTED-PATH-2");
        }

        void Deserialize(string xml)
        {
            var element = XElement.Parse(xml);
            var reader = new AssetManifestReader(element);
            assetManifest = reader.Read();
        }
    }
}