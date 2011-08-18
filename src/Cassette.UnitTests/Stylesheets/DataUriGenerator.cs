using System;
using System.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.Stylesheets
{
    public class DataUriGenerator_Tests
    {
        [Fact]
        public void TransformReplacesImageUrlWithDataUri()
        {
            var asset = new Mock<IAsset>();
            var directory = new Mock<IFileSystem>();
            asset.SetupGet(a => a.Directory)
                 .Returns(directory.Object);
            directory.Setup(d => d.OpenFile("test.png", FileMode.Open, FileAccess.Read))
                     .Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));

            var transformer = new DataUriGenerator();
            var css = "p { background-image: url(test.png); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);

            var expectedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
            getResult().ReadToEnd().ShouldEqual(
                "p { background-image: url(data:image/png;base64," + expectedBase64 + "); }"
            );
        }

        [Fact]
        public void FileWithJpgExtensionCreatesImageJpegDataUri()
        {
            var asset = new Mock<IAsset>();
            var directory = new Mock<IFileSystem>();
            asset.SetupGet(a => a.Directory)
                 .Returns(directory.Object);
            directory.Setup(d => d.OpenFile("test.jpg", FileMode.Open, FileAccess.Read))
                     .Returns(() => new MemoryStream());

            var transformer = new DataUriGenerator();
            var css = "p { background-image: url(test.jpg); }";
            var getResult = transformer.Transform(() => css.AsStream(), asset.Object);

            getResult().ReadToEnd().ShouldContain("image/jpeg");
        }
    }
}
