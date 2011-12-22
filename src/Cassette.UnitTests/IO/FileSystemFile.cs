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
        readonly Mock<IDirectory> directory;
        readonly FileSystemFile file;

        public FileSystemFile_Tests()
        {
            filename = Path.GetTempFileName();
            directory = new Mock<IDirectory>();
            directory.SetupGet(d => d.FullPath).Returns("~/");
            file = new FileSystemFile(Path.GetFileName(filename), directory.Object, filename);

            File.WriteAllText(filename, "test");
        }

        [Fact]
        public void DirectoryReturnsDirectoryPassedToConstructor()
        {
            file.Directory.ShouldBeSameAs(directory.Object);
        }

        [Fact]
        public void FullPathReturnsCombinesNameWithParentDirectoryPath()
        {
            file.FullPath.ShouldEqual("~/" + Path.GetFileName(filename));
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
            using (var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.ReadToEnd().ShouldEqual("test");
            }
        }

        [Fact]
        public void DeleteRemovesFileFromDirectory()
        {
            file.Delete();
            File.Exists(filename).ShouldBeFalse();
        }

        public void Dispose()
        {
            if (File.Exists(filename)) File.Delete(filename);
        }
    }
}

