using System.IO;
using Should;
using Xunit;

namespace Cassette.Spriting
{
    public class ImageFileLoader_GetImageBytesTests
    {
        [Fact]
        public void ReadsBytesFromFile()
        {
            var directory = new FakeFileSystem
            {
                { "~/test.png", new byte[] { 1, 2, 3 } }
            };
            var loader = new ImageFileLoader(directory);
            var output = loader.GetImageBytes("test.png");
            output.ShouldEqual(new byte[] { 1, 2, 3 });
        }

        [Fact]
        public void ThrowsExceptionIfFileDoesntExist()
        {
            var directory = new FakeFileSystem();
            var loader = new ImageFileLoader(directory);
            var exception = Record.Exception(() => loader.GetImageBytes("test.png"));
            exception.ShouldBeType<FileNotFoundException>();
        }
    }
}