using System.Diagnostics;
using Cassette.Utilities;
using Cassette.Web;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class ExpandCssUrlsAssetTransformer_Tests
    {
        public ExpandCssUrlsAssetTransformer_Tests()
        {
            fileSystem = new FakeFileSystem
            {
                "~/styles/asset.css"
            };

            urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator.Setup(u => u.CreateRawFileUrl(It.IsAny<string>(), It.IsAny<string>()))
                        .Returns<string, string>((f, h) => "EXPANDED");
            
            transformer = new ExpandCssUrlsAssetTransformer(fileSystem, urlGenerator.Object);
            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/styles/asset.css");
        }

        readonly ExpandCssUrlsAssetTransformer transformer;
        readonly Mock<IAsset> asset;
        readonly Mock<IUrlGenerator> urlGenerator;
        readonly FakeFileSystem fileSystem;

        [Fact]
        public void GivenCssWithRelativeUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            fileSystem.Add("~/styles/test.png");

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssUrlFileIsNotFound_WhenTransform_ThenUrlIsExpandedToAbsolutePath()
        {
            urlGenerator
                .Setup(g => g.CreateAbsolutePathUrl(It.IsAny<string>()))
                .Returns("ABSOLUTE-URL");

            var output = TransformCssWhereUrlDoesNotExist();

            output.ShouldEqual("p { background-image: url(ABSOLUTE-URL); }");
        }

        [Fact]
        public void GivenCssUrlIsPathStartingWithSlash_WhenTransform_ThenUrlIsUnchanged()
        {
            var css = "p { background-image: url(/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(/test.png); }");
        }

        [Fact]
        public void GivenCssUrlIsFullUrl_WhenTransform_ThenUrlIsUnchanged()
        {
            var css = "p { background-image: url(http://example.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(http://example.com/test.png); }");
        }

        [Fact]
        public void GivenCssUrlIsFullHttpsUrl_WhenTransform_ThenUrlIsUnchanged()
        {
            var css = "p { background-image: url(https://example.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(https://example.com/test.png); }");
        }

        [Fact]
        public void GivenCssUrlIsFullProtocolRelativeUrl_WhenTransform_ThenUrlIsUnchanged()
        {
            var css = "p { background-image: url(//example.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(//example.com/test.png); }");
        }

        [Fact]
        public void GivenCssUrlFileIsNotFound_WhenTransform_ThenRawFileReferenceIsNotAddedToAsset()
        {
            TransformCssWhereUrlDoesNotExist();

            asset.Verify(a => a.AddRawFileReference(It.IsAny<string>()), Times.Never());
        }

        string TransformCssWhereUrlDoesNotExist()
        {
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            return getResult().ReadToEnd();
        }

        [Fact]
        public void GivenCssWithUrlWithFragment_WhenTransformed_ThenUrlIsExpanded()
        {
            fileSystem.Add("~/styles/test.png");
            var css = "p { background-image: url(test.png#fragment); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithWhitespaceAroundRelativeUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            fileSystem.Add("~/styles/test.png");
            var css = "p { background-image: url(\n test.png \n); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\n EXPANDED \n); }");
        }

        [Fact]
        public void GivenCssWithDoubleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedWithoutQuotes()
        {
            fileSystem.Add("~/styles/test.png");
            var css = "p { background-image: url(\"test.png\"); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\"EXPANDED\"); }");
        }

        [Fact]
        public void GivenCssWithSingleQuotedRelativeUrl_WhenTransformed_ThenUrlIsExpandedWithoutQuotes()
        {
            fileSystem.Add("~/styles/test.png");

            var css = "p { background-image: url('test.png'); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url('EXPANDED'); }");
        }

        [Fact]
        public void GivenCssWithHttpUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(http://test.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(http://test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithRelativeTopMostUrl_WhenTransformed_ThenUrlIsExpanded()
        {
            fileSystem.Add("~/styles/test.png");

            var css = "p { background-image: url(/styles/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithProtocolRelativeUrl_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(//test.com/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(//test.com/test.png); }");
        }

        [Fact]
        public void GivenCssWithDataUri_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(data:image/png;base64,abc); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(data:image/png;base64,abc); }");
        }

        [Fact]
        public void GivenCssWithDataUriInDoubleQuotes_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url(\"data:image/png;base64,abc\"); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(\"data:image/png;base64,abc\"); }");
        }

        [Fact]
        public void GivenCssWithDataUriInSingleQuotes_WhenTransformed_ThenUrlNotChanged()
        {
            var css = "p { background-image: url('data:image/png;base64,abc'); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url('data:image/png;base64,abc'); }");
        }

        [Fact]
        public void GivenCssWithUrlToDifferentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            fileSystem.Add("~/styles/images/test.png");

            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/images/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            fileSystem.Add("~/images/test.png");

            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/images/test.png", It.IsAny<string>()));
        }

        [Fact]
        public void GivenAssetInSubDirectoryAndCssWithUrlToParentDirectory_WhenTransformed_ThenUrlIsExpanded()
        {
            asset.SetupGet(a => a.Path).Returns("~/styles/sub/asset.css");
            fileSystem.Add("~/styles/sub/asset.css");
            fileSystem.Add("~/styles/images/test.png");

            var css = "p { background-image: url(../images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            var output = getResult().ReadToEnd();

            output.ShouldEqual("p { background-image: url(EXPANDED); }");

            urlGenerator.Verify(g => g.CreateRawFileUrl("~/styles/images/test.png", It.IsAny<string>()));
        }
    }
}