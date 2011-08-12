using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.ModuleProcessing
{
    public class ExpandCssUrlsAssetTransformer_Tests
    {
        public ExpandCssUrlsAssetTransformer_Tests()
        {
            var module = new Module("styles", Mock.Of<IFileSystem>());
            application = new Mock<ICassetteApplication>();
            transformer = new ExpandCssUrlsAssetTransformer(module, application.Object);
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.SourceFilename).Returns("asset.css");
            GivenVirtualDirectory("/");
        }

        readonly ExpandCssUrlsAssetTransformer transformer;
        readonly Mock<ICassetteApplication> application;
        readonly Mock<IAsset> asset;

        [Fact]
        public void GivenCssWithRelativeUrl_WhenTransformed_ThenUrlIsExpandedToRootedPath()
        {
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/styles/test.png); }");
        }

        [Fact]
        public void GivenCssWithWhitespaceAroundRelativeUrl_WhenTransformed_ThenUrlIsExpandedToRootedPath()
        {
            var css = "p { background-image: url(\n test.png \n); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\n /styles/test.png \n); }");
        }

        [Fact]
        public void GivenCssWithDoubleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedToRootedPathWithoutQuotes()
        {
            var css = "p { background-image: url(\"test.png\"); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/styles/test.png); }");
        }

        [Fact]
        public void GivenCssWithSingleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedToRootedPathWithoutQuotes()
        {
            var css = "p { background-image: url('test.png'); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/styles/test.png); }");
        }

        [Fact]
        public void GivenCssWithHttpUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(http://test.com/test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(http://test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithProtocolRelativeUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(//test.com/test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(//test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithUrlToDifferentDirectory_WhenTransformed_ThenUrlIsExpandedToRootedPath()
        {
            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/styles/images/test.png); }");
        }

        [Fact]
        public void GivenCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpandedToRootedPath()
        {
            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/images/test.png); }");
        }

        [Fact]
        public void GivenAssetInSubDirectoryAndCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpandedToRootedPath()
        {
            asset.SetupGet(a => a.SourceFilename).Returns("sub/asset.css");
            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/styles/images/test.png); }");
        }

        void GivenVirtualDirectory(string path)
        {
            application.Setup(a => a.CreateAbsoluteUrl(It.IsAny<string>()))
                       .Returns<string>(s => path + s.Replace('\\', '/'));
        }
    }
}
