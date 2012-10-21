using System.IO;
using Moq;
using Should;
using Xunit;

namespace Cassette.Spriting
{
    public class ImageFileLoader_GetImageBytesTests
    {
        readonly Mock<IUrlGenerator> urlGenerator;

        public ImageFileLoader_GetImageBytesTests()
        {
            urlGenerator = new Mock<IUrlGenerator>();
            urlGenerator
                .Setup(g => g.CreateRawFileUrl(It.IsAny<string>()))
                .Returns<string>(
                    filename => "/cassette.axd/file/" + filename.Substring(2) + "-hash.png"
                );
        }

        [Fact]
        public void ReadsBytesFromFile()
        {
            var directory = new FakeFileSystem
            {
                { "~/test.png", new byte[] { 1, 2, 3 } }
            };
            var loader = new ImageFileLoader(directory);
            var output = loader.GetImageBytes("/cassette.axd/file/test-hash123.png");
            output.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void ThrowsExceptionIfFileDoesntExist()
        {
            var directory = new FakeFileSystem();
            var loader = new ImageFileLoader(directory);
            var exception = Record.Exception(() => loader.GetImageBytes("/cassette.axd/file/test-hash123.png"));
            exception.ShouldBeType<FileNotFoundException>();
        }
    }
}