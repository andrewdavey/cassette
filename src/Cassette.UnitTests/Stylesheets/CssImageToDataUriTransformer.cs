using System.IO;
using System.Security.Cryptography;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class CssImageToDataUriTransformer_Tests
    {
        public CssImageToDataUriTransformer_Tests()
        {
            fileSystem = new FakeFileSystem
            {
                "~/asset.css"
            };

            asset = new Mock<IAsset>();
            asset.SetupGet(a => a.Path).Returns("~/asset.css");

            transformer = new CssImageToDataUriTransformer(url => true, fileSystem);
        }

        readonly Mock<IAsset> asset;
        readonly FakeFileSystem fileSystem;
        CssImageToDataUriTransformer transformer;

        [Fact]
        public void TransformInsertsImageUrlWithDataUriAfterTheExistingImage()
        {
            fileSystem.Add("~/test.png", new byte[] { 1, 2, 3 });
            
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            var expectedBase64 = Base64Encode(new byte[] { 1, 2, 3 });
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png);background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        [Fact]
        public void TransformInsertsImageUrlWithDataUriAfterEachExistingImage()
        {
            fileSystem.Add("~/test1.png", new byte[] { 1, 2, 3 });
            fileSystem.Add("~/test2.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test1.png); } " +
                      "a { background-image: url(test2.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            var expectedBase64 = Base64Encode(new byte[] { 1, 2, 3 });
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test1.png);background-image: url(data:image/png;base64," + expectedBase64 + "); } " +
                "a { background-image: url(test2.png);background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        [Fact]
        public void ImageUrlCanHaveSubDirectory()
        {
            asset.SetupGet(a => a.Path).Returns("~/styles/jquery-ui/jquery-ui.css");
            fileSystem.Add("~/styles/jquery-ui/jquery-ui.css");
            fileSystem.Add("~/styles/jquery-ui/images/test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(images/test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            var expectedBase64 = Base64Encode(new byte[] { 1, 2, 3 });
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(images/test.png);background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        [Fact]
        public void FileWithJpgExtensionCreatesImageJpegDataUri()
        {
            fileSystem.Add("~/test.jpg", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.jpg); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldContain("url(data:image/jpeg;");
        }

        [Fact]
        public void FileWithSvgExtensionCreatesImageSvgXmlDataUri()
        {
            fileSystem.Add("~/test.svg", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.svg); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldContain("url(data:image/svg+xml;");
        }

        [Fact]
        public void AssetAddRawFileReferenceIsCalled()
        {
            fileSystem.Add("~/test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            getResult();

            asset.Verify(a => a.AddRawFileReference("~/test.png"));
        }

        [Fact]
        public void GivenFileDoesNotExists_WhenTransform_ThenUrlIsNotChanged()
        {
            var css = "p { background-image: url(FILE_NOT_FOUND.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(FILE_NOT_FOUND.png); }"
            );
        }

        [Fact]
        public void GivenPredicateToTestImagePathReturnsFalse_WhenTransform_ThenImageIsNotTransformedToDataUri()
        {
            transformer = new CssImageToDataUriTransformer(path => false, fileSystem);

            fileSystem.Add("~/test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        [Fact]
        public void GivenFileIsLargerThan32768bytesAndIE8SupportEnabled_WhenTransform_ThenUrlIsNotTransformedIntoDataUri()
        {
            // IE 8 doesn't work with data-uris larger than 32768 bytes.
            fileSystem.Add("~/test.png", new byte[32768 + 1]);

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        [Fact]
        public void GivenFileIs32768bytes_WhenTransform_ThenUrlIsTransformedIntoDataUri()
        {
            // IE 8 will work with data-uris up to and including 32768 bytes in length.
            fileSystem.Add("~/test.png", new byte[32768]);

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            var expectedBase64 = Base64Encode(new byte[32768]);
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png);background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        string Base64Encode(byte[] bytes)
        {
            // We're not using Covert.ToBase64String(bytes) here because it pads the of the base64 string differently!
            // So use the same, stream-based, approach used by the transformer instead.
            using (var input = new MemoryStream(bytes))
            using (var output = new MemoryStream())
            using (var base64Stream = new CryptoStream(output, new ToBase64Transform(), CryptoStreamMode.Write))
            {
                input.CopyTo(base64Stream);
                base64Stream.FlushFinalBlock();
                base64Stream.Flush();
                output.Position = 0;
                var reader = new StreamReader(output);
                return reader.ReadToEnd();
            }
        }
    }
}