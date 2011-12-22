using System.IO;
using Should;
using Xunit;

namespace Cassette.IO
{
    public class NonExistentFile_Tests
    {
        readonly NonExistentFile file = new NonExistentFile("c:\\fail.txt");

        [Fact]
        public void DirectoryPropertyThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => file.Directory);
        }

        [Fact]
        public void LastWriteTimeUtcThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => file.LastWriteTimeUtc);
        }

        [Fact]
        public void OpenThrowsFileNotFoundException()
        {
            Assert.Throws<FileNotFoundException>(() => file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
        }

        [Fact]
        public void ExistsReturnsFalse()
        {
            file.Exists.ShouldEqual(false);
        }

        [Fact]
        public void FullPathReturnsPathPassedToConstructor()
        {
            file.FullPath.ShouldEqual("c:\\fail.txt");
        }
    }
}

