using System.IO;
using System.Linq;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleDescriptorReader_Tests
    {
        Mock<IFileSystem> fileSystem = new Mock<IFileSystem>();

        ModuleDescriptorReader GetReader(string descriptor)
        {
            return new ModuleDescriptorReader(descriptor.AsStream(), fileSystem.Object);
        }

        void FilesExist(params string[] filenames)
        {
            foreach (var filename in filenames)
            {
                fileSystem.Setup(fs => fs.FileExists(filename)).Returns(true);
            }
        }

        [Fact]
        public void SkipsBlankLines()
        {
            var reader = GetReader("\r\n\t\n ");
            var filenames = reader.ReadFilenames();
            filenames.ShouldBeEmpty();
        }

        [Fact]
        public void SkipsComments()
        {
            var reader = GetReader("#comment");
            var filenames = reader.ReadFilenames();
            filenames.ShouldBeEmpty();
        }

        [Fact]
        public void SkipsCommentsWithLeadingWhitespace()
        {
            var reader = GetReader("#comment with leading space");
            var filenames = reader.ReadFilenames();
            filenames.ShouldBeEmpty();
        }

        [Fact]
        public void ReturnsFilesSpecified()
        {
            var reader = GetReader("test1.js\ntest2.js");
            FilesExist("test1.js", "test2.js");
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void ThrowsExceptionWhenFileNotFound()
        {
            var reader = GetReader("test1.js\ntest2.js");
            FilesExist("test1.js");
            Assert.Throws<FileNotFoundException>(delegate
            {
                reader.ReadFilenames().ToArray();
            });
        }

        [Fact]
        public void CommentAfterFilenameIgnored()
        {
            var reader = GetReader("test.js # comment");
            FilesExist("test.js");
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void AsteriskIncludesAllFiles()
        {
            var reader = GetReader("*");
            fileSystem.Setup(fs => fs.GetFiles(""))
                      .Returns(new[] { "test1.js", "test2.js" });
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void AsteriskIncludesAllFilesNotAlreadyadded()
        {
            var reader = GetReader("test1.js\n*");
            FilesExist("test1.js", "test2.js");
            fileSystem.Setup(fs => fs.GetFiles(""))
                      .Returns(new[] { "test1.js", "test2.js" });
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }
    }
}
