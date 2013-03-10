using System;
using System.Xml.Linq;
using Cassette.IO;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Caching
{
    public class BundleCollectionCache_Write_Tests : IDisposable
    {
        readonly TempDirectory path;
        readonly FileSystemDirectory directory;
        readonly Mock<ScriptBundle> scriptBundle;
        readonly Mock<StylesheetBundle> stylesheetBundle;

        public BundleCollectionCache_Write_Tests()
        {
            path = new TempDirectory();
            directory = new FileSystemDirectory(path);

            var bundles = new BundleCollection(new CassetteSettings(), Mock.Of<IFileSearchProvider>(), Mock.Of<IBundleFactoryProvider>());
            scriptBundle = new Mock<ScriptBundle>("~/test1");
            scriptBundle.CallBase = true;
            scriptBundle.Object.Hash = new byte[] { 1, 2, 3 };
            scriptBundle.Object.Assets.Add(new StubAsset("~/test/asset.js", "script-bundle-content"));
            scriptBundle.Object.Renderer = new ScriptBundleHtmlRenderer(Mock.Of<IUrlGenerator>());
            scriptBundle.Setup(b => b.Render()).Returns("");
            bundles.Add(scriptBundle.Object);
            
            stylesheetBundle = new Mock<StylesheetBundle>("~/test2");
            stylesheetBundle.CallBase = true;
            stylesheetBundle.Object.Hash = new byte[] { 4, 5, 6 };
            stylesheetBundle.Object.Assets.Add(new StubAsset("~/test2/asset.css", "stylesheet-bundle-content"));
            stylesheetBundle.Object.Renderer = new StylesheetHtmlRenderer(Mock.Of<IUrlGenerator>());
            stylesheetBundle.Setup(b => b.Render()).Returns("");
            bundles.Add(stylesheetBundle.Object);

            var cache = new BundleCollectionCache(directory, b => null);
            cache.Write(new Manifest(bundles, "VERSION"));
        }

        [Fact]
        public void ItCreatesManifestXmlFile()
        {
            directory.GetFile("manifest.xml").Exists.ShouldBeTrue();
        }

        [Fact]
        public void CreatedManifestXmlHasVersionAttribute()
        {
            var xml = directory.GetFile("manifest.xml").OpenRead().ReadToEnd();
            xml.ShouldContain("Version=\"VERSION\"");
        }

        [Fact]
        public void ItSerializesBundlesIntoManifest()
        {
            scriptBundle.Verify(b => b.SerializeInto(It.IsAny<XContainer>()));
            stylesheetBundle.Verify(b => b.SerializeInto(It.IsAny<XContainer>()));
        }

        [Fact]
        public void ItCreatesScriptBundleContentFile()
        {
            var hash = new byte[] { 1, 2, 3 }.ToHexString();
            var file = directory.GetFile("~/script/test1/" + hash + ".js");
            var content = file.OpenRead().ReadToEnd();
            content.ShouldEqual("script-bundle-content");
        }

        [Fact]
        public void ItCreatesStylesheetBundleContentFile()
        {
            var hash = new byte[] { 4, 5, 6 }.ToHexString();
            var file = directory.GetFile("~/stylesheet/test2/" + hash + ".css");
            var content = file.OpenRead().ReadToEnd();
            content.ShouldEqual("stylesheet-bundle-content");
        }

        public void Dispose()
        {
            path.Dispose();
        }
    }
}