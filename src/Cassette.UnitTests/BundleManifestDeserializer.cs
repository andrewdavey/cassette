using System.Collections.Generic;
using System.Linq;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette
{
    public class BundleManifestDeserializer_Tests
    {
        List<BundleManifest> manifests;

        [Fact]
        public void EmptyXmlReturnsNoBundleManifests()
        {
            Deserialize("<Bundles></Bundles>");
            manifests.ShouldBeEmpty();
        }

        [Fact]
        public void XmlWithSingleBundleReturnsSingleManifest()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\" /></Bundles>");
            manifests.Count.ShouldEqual(1);
        }

        [Fact]
        public void ScriptBundleXmlMissingPathAttributeThrowsException()
        {
            var exception = Assert.Throws<InvalidBundleManifestException>(() => Deserialize("<Bundles><ScriptBundle /></Bundles>"));
            exception.Message.ShouldEqual("Bundle manifest element missing \"Path\" attribute.");
        }

        [Fact]
        public void BundleManifestPathIsInitializedFromXmlAttribute()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\"/></Bundles>");
            manifests[0].Path.ShouldEqual("~");
        }

        [Fact]
        public void BundleManifestHashIsInitializedFromXmlAttribute()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"010203\"/></Bundles>");
            manifests[0].Hash.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void BundleManifestContentTypeIsInitializedFromXmlAttribute()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\" ContentType=\"EXPECTED-CONTENT-TYPE\"/></Bundles>");
            manifests[0].ContentType.ShouldEqual("EXPECTED-CONTENT-TYPE");
        }

        [Fact]
        public void BundleManifestContentTypeIsNullWhenXmlAttributeIsMissing()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\"/></Bundles>");
            manifests[0].ContentType.ShouldBeNull();
        }

        [Fact]
        public void BundleManifestPageLocationIsInitializedFromXmlAttribute()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\" PageLocation=\"LOCATION\"/></Bundles>");
            manifests[0].PageLocation.ShouldEqual("LOCATION");
        }

        [Fact]
        public void BundleManifestPageLocationIsNullWhenXmlAttributeIsMissing()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\"/></Bundles>");
            manifests[0].PageLocation.ShouldBeNull();
        }

        [Fact]
        public void AssetManifestCreatedFromAssetsInXml()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\"><Asset Path=\"~/asset/path\"/></ScriptBundle></Bundles>");
            manifests[0].Assets.Count.ShouldEqual(1);
        }

        [Fact]
        public void ReferenceCreatedFromXmlChildElement()
        {
            Deserialize("<Bundles><ScriptBundle Path=\"~\" Hash=\"\"><Reference Path=\"~/EXPECTED-PATH\"/></ScriptBundle></Bundles>");
            manifests[0].References[0].ShouldEqual("~/EXPECTED-PATH");
        }

        [Fact]
        public void ScriptBundleManifestCreatedFromScriptElement()
        {
            TypeIsCreatedFrom<Scripts.ScriptBundleManifest>("<ScriptBundle Path=\"~\" Hash=\"\"/>");
        }

        [Fact]
        public void ExternalScriptBundleManifestCreatedFromExternalScriptElement()
        {
            TypeIsCreatedFrom<Scripts.ExternalScriptBundleManifest>("<ExternalScriptBundle Path=\"~\" Hash=\"\" Url=\"http://example.com/\"/>");
        }

        [Fact]
        public void StylesheetBundleManifestCreatedFromStylesheetElement()
        {
            TypeIsCreatedFrom<Stylesheets.StylesheetBundleManifest>("<StylesheetBundle Path=\"~\" Hash=\"\"/>");
        }

        [Fact]
        public void ExternalStylesheetBundleManifestCreatedFromExternalStylesheetElement()
        {
            TypeIsCreatedFrom<Stylesheets.ExternalStylesheetBundleManifest>("<ExternalStylesheetBundle Path=\"~\" Hash=\"\" Url=\"http://example.com/\"/>");
        }

        [Fact]
        public void HtmlTemplateBundleManifestCreatedFromHtmlTemplateElement()
        {
            TypeIsCreatedFrom<HtmlTemplates.HtmlTemplateBundleManifest>("<HtmlTemplateBundle Path=\"~\" Hash=\"\"/>");
        }

        [Fact]
        public void DeserializeUnknownBundleTypeThrowsException()
        {
            Assert.Throws<InvalidBundleManifestException>(
                () => Deserialize("<Bundles><UnknownBundle /></Bundles>")
            );
        }

        void Deserialize(string xml)
        {
            using (var stream = xml.AsStream())
            {
                var deserializer = new BundleManifestDeserializer();
                manifests = deserializer.Deserialize(stream).ToList();
            }
        }

        void TypeIsCreatedFrom<T>(string bundleXml)
        {
            Deserialize("<Bundles>" + bundleXml + "</Bundles>");
            manifests[0].ShouldBeType<T>();
        }
    }
}