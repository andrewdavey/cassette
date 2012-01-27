using System;
using System.Linq;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette.Web
{
    public abstract class UrlGenerator_Tests
    {
        protected readonly Mock<IUrlModifier> urlModifier = new Mock<IUrlModifier>();
        internal readonly UrlGenerator UrlGenerator;

        public UrlGenerator_Tests()
        {
            urlModifier.Setup(m => m.Modify(It.IsAny<string>()))
                       .Returns<string>(url => url);

            var container = new Mock<ICassetteApplicationContainer<ICassetteApplication>>();
            container.SetupGet(c => c.Application.Bundles).Returns(Enumerable.Empty<Bundle>());
            UrlGenerator = new UrlGenerator(urlModifier.Object, "_cassette");
        }
    }

    public class UrlGenerator_CreateBundleUrl_Tests : UrlGenerator_Tests
    {
        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test"));
            urlModifier.Verify(m => m.Modify("_cassette/scriptbundle/test_010203"));
        }

        [Fact]
        public void CreateScriptBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndPathAndHash()
        {
            var url = UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test/foo"));
            url.ShouldEqual("_cassette/scriptbundle/test/foo_010203");
        }

        [Fact]
        public void CreateStylesheetBundleUrlReturnsUrlWithRoutePrefixAndBundleTypeAndPathAndHash()
        {
            var url = UrlGenerator.CreateBundleUrl(StubStylesheetBundle("~/test/foo"));
            url.ShouldEqual("_cassette/stylesheetbundle/test/foo_010203");
        }

        static ScriptBundle StubScriptBundle(string path)
        {
            var bundle = new ScriptBundle(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            bundle.Assets.Add(asset.Object);
            return bundle;
        }

        static StylesheetBundle StubStylesheetBundle(string path)
        {
            var bundle = new StylesheetBundle(path);
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 3 });
            bundle.Assets.Add(asset.Object);
            return bundle;
        }
    }

    public class UrlGenerator_CreateAssetUrl_Tests : UrlGenerator_Tests
    {
        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[0]);

            UrlGenerator.CreateAssetUrl(asset.Object);

            urlModifier.Verify(m => m.Modify(It.IsAny<string>()));
        }

        [Fact]
        public void CreateAssetUrlReturnsCompileUrl()
        {
            var asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/test/asset.coffee");
            asset.SetupGet(a => a.Hash).Returns(new byte[] { 1, 2, 15, 16 });

            var url = UrlGenerator.CreateAssetUrl(asset.Object);

            url.ShouldEqual("_cassette/asset/test/asset.coffee?01020f10");
        }
    }

    public class UrlGenerator_CreateRawFileUrl_Tests : UrlGenerator_Tests
    {
        [Fact]
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndPathWithoutTildeAndHashAndExtensionDotConvertedToUnderscore()
        {
            var url = UrlGenerator.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldStartWith("_cassette/file/test_hash_png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("_cassette/file/test/foo_hash_png");
        }

        [Fact]
        public void ArgumentExceptionThrownWhenFilenameDoesNotStartWithTilde()
        {
            Assert.Throws<ArgumentException>(delegate
            {
                UrlGenerator.CreateRawFileUrl("fail.png", "hash");
            });
        }
    }
}