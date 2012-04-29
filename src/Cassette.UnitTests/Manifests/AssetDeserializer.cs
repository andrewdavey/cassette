using System.Linq;
using System.Xml.Linq;
using Should;
using Xunit;

namespace Cassette.Manifests
{
    public class AssetDeserializer_Tests
    {
        IAsset asset;

        [Fact]
        public void AssetManifestPathInitializedFromXmlAttribute()
        {
            Deserialize("<Asset Path=\"~/asset/path\"/>");
            asset.Path.ShouldEqual("~/asset/path");
        }

        [Fact]
        public void RawFileReferencesEmptyWhenXmlElementHasNoChildren()
        {
            Deserialize("<Asset Path=\"~/asset/path\"/>");
            asset.References.ShouldBeEmpty();
        }

        [Fact]
        public void RawFileReferenceInitializedFromXmlChildElement()
        {
            Deserialize("<Asset Path=\"~\"><Reference Type=\"RawFilename\" Path=\"~/EXPECTED-PATH\" SourceLineNumber=\"1\"/></Asset>");
            var reference = asset.References.First();
            reference.Type.ShouldEqual(AssetReferenceType.RawFilename);
            reference.ToPath.ShouldEqual("~/EXPECTED-PATH");
            reference.SourceLineNumber.ShouldEqual(1);
        }

        [Fact]
        public void TwoRawFileReferenceInitializedFromXmlChildElements()
        {
            Deserialize("<Asset Path=\"~\">" +
                        "<Reference Type=\"RawFilename\" Path=\"~/EXPECTED-PATH-1\" SourceLineNumber=\"1\"/>" +
                        "<Reference Type=\"RawFilename\" Path=\"~/EXPECTED-PATH-2\" SourceLineNumber=\"2\"/>" +
                        "</Asset>");
            var references = asset.References.ToArray();
            references[0].ToPath.ShouldEqual("~/EXPECTED-PATH-1");
            references[1].ToPath.ShouldEqual("~/EXPECTED-PATH-2");
        }

        void Deserialize(string xml)
        {
            var element = XElement.Parse(xml);
            var reader = new AssetDeserializer();
            asset = reader.Deserialize(element);
        }
    }
}