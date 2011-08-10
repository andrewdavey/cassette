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
            var url = app.CreateModuleUrl(new ScriptModule("test", Mock.Of<IFileSystem>()));
            url.ShouldStartWith("/");
        }

        [Fact]
        public void AppendsSlashToVirtualDirectoryWhenMissingFromEnd()
        {
            var app = new UrlGenerator("/myapp");
            var url = app.CreateModuleUrl(new ScriptModule("test", Mock.Of<IFileSystem>()));
            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void Inserts_assetsPrefix()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(new ScriptModule("test", Mock.Of<IFileSystem>()));
            url.ShouldStartWith("/_assets/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedScriptModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(new ScriptModule("test", Mock.Of<IFileSystem>()));
            url.ShouldStartWith("/_assets/scripts/");
        }

        [Fact]
        public void InsertsLowercasedPluralisedStylesheetModuleTypeName()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(new StylesheetModule("test", Mock.Of<IFileSystem>()));
            url.ShouldStartWith("/_assets/stylesheets/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(new ScriptModule("test", Mock.Of<IFileSystem>()));
            url.ShouldEqual("/_assets/scripts/test");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var app = new UrlGenerator("/");
            var url = app.CreateModuleUrl(new ScriptModule("test\\foo\\bar", Mock.Of<IFileSystem>()));
            url.ShouldEqual("/_assets/scripts/test/foo/bar");
        }
    }

    public class UrlGenerator_CreateAssetUrl_Tests
    {
        [Fact]
        public void StartsWithApplicationVirtualDirectory()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/");
        }

        [Fact]
        public void StartsWithApplicationVirtualDirectoryEndingInSlash()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/");
        }

        [Fact]
        public void Inserts_assetsPrefix()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/_assets/");
        }

        [Fact]
        public void InsertsModuleDirectory()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/_assets/test/");
        }

        [Fact]
        public void InsertsModuleDirectoryWithBackSlashesConvertedToForwardSlashes()
        {
            var module = new Module("test\\foo\\bar", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/myapp");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/myapp/_assets/test/foo/bar");
        }

        [Fact]
        public void InsertsAssetSourceFilename()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/_assets/test/asset.js");
        }

        [Fact]
        public void InsertsAssetSourceFilenameWithBackSlashesConvertedToForwardSlashes()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\asset.js");
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldStartWith("/_assets/test/sub/asset.js");
        }

        [Fact]
        public void AppendsHashHexString()
        {
            var module = new Module("test", Mock.Of<IFileSystem>());
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("sub\\asset.js");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });
            var app = new UrlGenerator("/");

            var url = app.CreateAssetUrl(module, asset.Object);

            url.ShouldEqual("/_assets/test/sub/asset.js?01020f10");
        }
    }

    public class UrlGenerator_ModuleUrlPattern_Tests
    {
        [Fact]
        public void InsertsConventionalScriptModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.ModuleUrlPattern<ScriptModule>();
            url.ShouldEqual("_assets/scripts/{*path}");
        }

        [Fact]
        public void InsertsConventionalStylesheetModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.ModuleUrlPattern<StylesheetModule>();
            url.ShouldEqual("_assets/stylesheets/{*path}");
        }

        [Fact]
        public void InsertsConventionalHtmlTemplateModuleName()
        {
            var app = new UrlGenerator("/");
            var url = app.ModuleUrlPattern<HtmlTemplateModule>();
            url.ShouldEqual("_assets/htmltemplates/{*path}");
        }
    }
}
