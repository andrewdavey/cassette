using System;
using System.IO;
using Cassette.Utilities;
using Moq;
using Should;
using Xunit;

namespace Cassette.IO
{
    public class FileSystemFile_Tests : IDisposable
    {
        readonly string filename;
        readonly IDirectory directory;
        readonly FileSystemFile file;

        public FileSystemFile_Tests()
        {
            filename = Path.GetTempFileName();
            directory = Mock.Of<IDirectory>();
            file = new FileSystemFile(filename, directory);

            File.WriteAllText(filename, "test");
        }

        [Fact]
        public void DirectoryReturnsDirectoryPassedToConstructor()
        {
            file.Directory.ShouldBeSameAs(directory);
        }

        [Fact]
        public void FullPathReturnsFilenamePassedToConstructor()
        {
            file.FullPath.ShouldEqual(filename);
        }

        [Fact]
        public void ExistsReturnsTrue()
        {
            file.Exists.ShouldBeTrue();
        }

        [Fact]
        public void LastWriteTimeUtcReturnsLastWriteTimeOfActualFile()
        {
            file.LastWriteTimeUtc.ShouldEqual(File.GetLastWriteTimeUtc(filename));
        }

        [Fact]
        public void OpenReturnsFileStream()
        {
            using (var stream = file.Open(FileMode.Open, FileAccess.Read))
            {
                stream.ReadToEnd().ShouldEqual("test");
            }
        }

        public void Dispose()
        {
            File.Delete(filename);
        }
    }
}
