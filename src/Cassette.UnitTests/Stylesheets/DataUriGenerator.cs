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
        

        // TODO: Add legacy IE support for data-uris.
        // This is hard because we need to add a css star-hack property after the property with the image.
        // e.g. p { background: #fff url(blah.png); }
        //   -> p { background: #fff url(data:image/png;base64,xxxx); *background-image: url(mhtml:http://test.com/this.css!image0); }
        // So the transformer must understand how to expand compact CSS rules into "-image" versions.

        //[Fact]
        //public void GivenLegacyIESupport_ThenTransformConvertsCssToMultipartDocument()
        //{
        //    var asset = new Mock<IAsset>();
        //    var directory = new Mock<IFileSystem>();
        //    asset.SetupGet(a => a.Directory)
        //         .Returns(directory.Object);
        //    directory.Setup(d => d.OpenFile("test.png", FileMode.Open, FileAccess.Read))
        //             .Returns(() => new MemoryStream(new byte[] { 1, 2, 3 }));

        //    var application = new Mock<ICassetteApplication>();
        //    var transformer = new DataUriGenerator(application.Object);
        //    //transformer.LegacyIESupport = true;
        //    var css = "p { background-image: url(test.png); }";
        //    var getResult = transformer.Transform(() => css.AsStream(), asset.Object);

        //    var expectedBase64 = Convert.ToBase64String(new byte[] { 1, 2, 3 });
        //    getResult().ReadToEnd().ShouldEqual(
        //        "/*" +
        //        "\r\n" + 
        //        "Content-Type: multipart/related; boundary=\"CASSETTE-IMAGE-BOUNDARY\"" +
        //        "\r\n\r\n" + 
        //        "--CASSETTE-IMAGE-BOUNDARY"+
        //        "\r\n" + 
        //        "Content-Location: i0" +
        //        "\r\n" +
        //        "Content-Transfer-Encoding: base64" +
        //        "\r\n\r\n" + 
        //        expectedBase64 +
        //        "\r\n\r\n" + 
        //        "--CASSETTE-IMAGE-BOUNDARY--" +
        //        "\r\n" + 
        //        "*/" +
        //        "\r\n" + 
        //        "p { background-image: url(data:image/png;base64," + expectedBase64 + "); *background-image:url(mhtml:http://test.com/styles.css!i0); }"
        //    );
        //}
        
    }
}
