using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette
{
    public class ModuleDescriptorReader_Tests
    {
        readonly List<string> files = new List<string>();
        
        ModuleDescriptorReader GetReader(string descriptor)
        {
            return new ModuleDescriptorReader(descriptor.AsStream(), files);
        }

        void FilesExist(params string[] filenames)
        {
            files.AddRange(filenames);
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
            FilesExist("test1.js", "test2.js");
            var reader = GetReader("test1.js\ntest2.js");
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void ThrowsExceptionWhenFileNotFound()
        {
            FilesExist("test1.js");
            var reader = GetReader("test1.js\ntest2.js");
            Assert.Throws<FileNotFoundException>(delegate
            {
                reader.ReadFilenames().ToArray();
            });
        }

        [Fact]
        public void CommentAfterFilenameIgnored()
        {
            FilesExist("test.js");
            var reader = GetReader("test.js # comment");
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test.js" }).ShouldBeTrue();
        }

        [Fact]
        public void AsteriskIncludesAllFiles()
        {
            FilesExist("test1.js", "test2.js");
            var reader = GetReader("*");
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }

        [Fact]
        public void AsteriskIncludesAllFilesNotAlreadyadded()
        {
            FilesExist("test1.js", "test2.js");
            var reader = GetReader("test1.js\n*");
            var filenames = reader.ReadFilenames();
            filenames.SequenceEqual(new[] { "test1.js", "test2.js" }).ShouldBeTrue();
        }
    }
}
