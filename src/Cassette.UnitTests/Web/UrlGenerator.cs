using Cassette.HtmlTemplates;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public class UrlGenerator_CreateModuleUrl_Tests
    {
        [Fact]
        public void UrlStartsWithApplicationVirtualDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/");
        }

        [Fact]
        public void AppendsSlashToVirtualDirectoryWhenMissingFromEnd()
        {
            var app = new UrlGenerator("/myapp");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void Inserts_assetsPrefix()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/_assets/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedScriptModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/_assets/scripts/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedStylesheetModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubStylesheetModule("test"));
            url.ShouldStartWith("/_assets/stylesheets/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test"));
            url.ShouldStartWith("/_assets/scripts/test");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test\\foo\\bar"));
            url.ShouldStartWith("/_assets/scripts/test/foo/bar");
        }

        [Fact]
        public void AppendsModuleHashHexString()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(StubScriptModule("test\\foo\\bar"));
            url.ShouldEqual("/_assets/scripts/test/foo/bar_010203");
        }

        ScriptModule StubScriptModule(string path)
        {
            var module = new ScriptModule(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            return module;
        }

        StylesheetModule StubStylesheetModule(string path)
        {
            var module = new StylesheetModule(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            module.Assets.Add(asset.Object);
            return module;
        }

    }

    public class UrlGenerator_CreateAssetUrl_Tests
    {
        [Fact]
        public void StartsWithApplicationVirtualDirectory()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/");
        }

        [Fact]
        public void StartsWithApplicationVirtualDirectoryEndingInSlash()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/test/");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var module = new Module("test\\foo\\bar");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/test/foo/bar");
        }

        [Fact]
        public void InsertsAssetSourceFilename()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/test/asset.js");
        }

        [Fact]
        public void InsertsAssetSourceFilenameWithBackSlashesConvertedToForwardSlashes()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/test/sub/asset.js");
        }

        [Fact]
        public void AppendsHashHexString()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\asset.js");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldEqual("/test/sub/asset.js?01020f10");
        }

        [Fact]
        public void CreateAssetCompileUrlReturnsCompileUrl()
        {
            var module = new Module("test");
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            var app = new UrlGenerator("/");

            var url = app.CreateAssetCompileUrl(module, asset.Object);

            url.ShouldEqual("/_assets/get/test/asset.coffee?01020f10");
        }
    }

    public class UrlGenerator_CreateImageUrl_Tests
    {
        [Fact]
        public void CreateImageUrlPrependsHandlerRoute()
        {
            var generator = new UrlGenerator("/");
            var url = generator.CreateImageUrl("test.png", "hash");
            url.ShouldEqual("/_assets/images/test_hash.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var generator = new UrlGenerator("/");
            var url = generator.CreateImageUrl("test\\foo.png", "hash");
            url.ShouldEqual("/_assets/images/test/foo_hash.png");
        }
    }

    public class UrlGenerator_GetModuleRouteUrl_Tests
    {
        [Fact]
        public void InsertsConventionalScriptModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<ScriptModule>();
            url.ShouldEqual("_assets/scripts/{*path}");
        }

        [Fact]
        public void InsertsConventionalStylesheetModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<StylesheetModule>();
            url.ShouldEqual("_assets/stylesheets/{*path}");
        }

        [Fact]
        public void InsertsConventionalHtmlTemplateModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.GetModuleRouteUrl<HtmlTemplateModule>();
            url.ShouldEqual("_assets/htmltemplates/{*path}");
        }
    }
}