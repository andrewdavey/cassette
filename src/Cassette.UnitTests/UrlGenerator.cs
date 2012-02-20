using System;
using System.Linq;
using Cassette.Scripts;
using Cassette.Stylesheets;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public abstract class UrlGenerator_Tests
    {
        protected readonly Mock<IUrlModifier> UrlModifier = new Mock<IUrlModifier>();
        internal readonly UrlGenerator UrlGenerator;

        protected UrlGenerator_Tests()
        {
            UrlModifier.Setup(m => m.Modify(It.IsAny<string>()))
                       .Returns<string>(url => url);

            var container = new Mock<ICassetteApplicationContainer<ICassetteApplication>>();
            container.SetupGet(c => c.Application.Bundles).Returns(Enumerable.Empty<Bundle>());
            UrlGenerator = new UrlGenerator(UrlModifier.Object, "_cassette");
        }
    }

    public class UrlGenerator_CreateBundleUrl_Tests : UrlGenerator_Tests
    {
        [Fact]
        public void CreateBundleUrlCallsBundleUrlProperty()
        {
            var bundle = new Mock<Bundle>("~");
            bundle.SetupGet(b => b.Url).Returns("url");

            var url = UrlGenerator.CreateBundleUrl(bundle.Object);

            url.ShouldEqual("_cassette/url");
        }

        [Fact]
        public void GivenBundleUrlIsExternal_ThenCreateBundleUrlDoesNotPrefixIt()
        {
            var bundle = new Mock<Bundle>("~");
            bundle.SetupGet(b => b.Url).Returns("http://example.org/");

            var url = UrlGenerator.CreateBundleUrl(bundle.Object);

            url.ShouldEqual("http://example.org/");
        }

        [Fact]
        public void UrlModifierModifyIsCalled()
        {
            UrlGenerator.CreateBundleUrl(StubScriptBundle("~/test"));
            UrlModifier.Verify(m => m.Modify("_cassette/scriptbundle/test_010203"));
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
            var bundle = new ScriptBundle(path)
            {
                Hash = new byte[] { 1, 2, 3 }
            };
            return bundle;
        }

        static StylesheetBundle StubStylesheetBundle(string path)
        {
            var bundle = new StylesheetBundle(path)
            {
                Hash = new byte[] { 1, 2, 3 }
            };
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

            UrlModifier.Verify(m => m.Modify(It.IsAny<string>()));
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
        public void CreateRawFileUrlReturnsUrlWithRoutePrefixAndPathWithoutTildeAndHash()
        {
            var url = UrlGenerator.CreateRawFileUrl("~/test.png", "hash");
            url.ShouldStartWith("_cassette/file/test_hash.png");
        }

        [Fact]
        public void ConvertsToForwardSlashes()
        {
            var url = UrlGenerator.CreateRawFileUrl("~\\test\\foo.png", "hash");
            url.ShouldEqual("_cassette/file/test/foo_hash.png");
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