using System.IO;
using System.Security.Cryptography;
using Cassette.IO;
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
            transformer = new CssImageToDataUriTransformer(url => true);

            directory = new Mock<IDirectory>();
            asset = new Mock<IAsset>();
            var file = new Mock<IFile>();
            asset.SetupGet(a => a.SourceFile.FullPath)
                 .Returns("asset.css");
            asset.SetupGet(a => a.SourceFile)
                 .Returns(file.Object);
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
        }

        readonly Mock<IAsset> asset;
        readonly Mock<IDirectory> directory;
        CssImageToDataUriTransformer transformer;

        [Fact]
        public void TransformInsertsImageUrlWithDataUriAfterTheExistingImage()
        {
            StubFile("test.png", new byte[] { 1, 2, 3 });
            
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
            StubFile("test1.png", new byte[] { 1, 2, 3 });
            StubFile("test2.png", new byte[] { 1, 2, 3 });

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
            asset.SetupGet(a => a.SourceFile.FullPath).Returns("~/styles/jquery-ui/jquery-ui.css");
            asset.SetupGet(a => a.SourceFile.Directory).Returns(directory.Object);
            StubFile("images/test.png", new byte[] { 1, 2, 3 });

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
            StubFile("test.jpg", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.jpg); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldContain("url(data:image/jpeg;");
        }

        [Fact]
        public void AssetAddRawFileReferenceIsCalled()
        {
            StubFile("test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);
            getResult();

            asset.Verify(a => a.AddRawFileReference("test.png"));
        }

        [Fact]
        public void GivenFileDoesNotExists_WhenTransform_ThenUrlIsNotChanged()
        {
            var file = new Mock<IFile>();
            file.SetupGet(f => f.Exists).Returns(false);
            directory.Setup(d => d.GetFile(It.IsAny<string>()))
                     .Returns(file.Object);

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        [Fact]
        public void GivenPredicateToTestImagePathReturnsFalse_WhenTransform_ThenImageIsNotTransformedToDataUri()
        {
            transformer = new CssImageToDataUriTransformer(path => false);

            StubFile("test.png", new byte[] { 1, 2, 3 });

            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(css.AsStream, asset.Object);

            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(test.png); }"
            );
        }

        [Fact]
        public void GivenFileIsLargerThan32768bytes_WhenTransform_ThenUrlIsNotTransformedIntoDataUri()
        {
            // IE 8 doesn't work with data-uris larger than 32768 bytes.
            StubFile("test.png", new byte[32768 + 1]);

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
            StubFile("test.png", new byte[32768]);

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
                base64Stream.Flush();
                output.Position = 0;
                var reader = new StreamReader(output);
                return reader.ReadToEnd();
            }
        }

        void StubFile(string filename, byte[] bytes)
        {
            var file = new Mock<IFile>();
            directory.Setup(d => d.GetFile(filename))
                .Returns(file.Object);
            file.SetupGet(f => f.Directory)
                .Returns(directory.Object);
            file.SetupGet(f => f.Exists)
                .Returns(true);
            file.Setup(d => d.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                .Returns(() => new MemoryStream(bytes));
        }
    }
}