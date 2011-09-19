using System.IO;
using Should;
using Xunit;
using Cassette.Utilities;

namespace Cassette.IO
{
    public class FileSystemDirectory_GetFile_Tests
    {
        [Fact]
        public void GivenFileExists_WhenGetFile_ThenReturnFileSystemFile()
        {
            using (var path = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(path, "test.txt"), "test");

                var dir = new FileSystemDirectory(path);
                var file = dir.GetFile("test.txt");
                file.ShouldBeType<FileSystemFile>();
                PathUtilities.PathsEqual(file.FullPath, Path.Combine(path, "test.txt")).ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenFileDoesNotExist_WhenGetFile_ThenReturnFileSystemFile()
        {
            using (var path = new TempDirectory())
            {
                var dir = new FileSystemDirectory(path);
                var file = dir.GetFile("test.txt");
                file.ShouldBeType<FileSystemFile>();
            }
        }

        [Fact]
        public void GivenSubDirectoryDoesNotExist_WhenGetFile_ThenReturnNonExistentFile()
        {
            using (var path = new TempDirectory())
            {
                var dir = new FileSystemDirectory(path);
                var file = dir.GetFile("sub\\test.txt");
                file.ShouldBeType<NonExistentFile>();
            }
        }
    }

    public class FileSystemDirectory_DeleteAll_Tests
    {
        [Fact]
        public void GivenFilesAndSubDirectories_WhenDeleteAll_ThenEverythingIsDeleted()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "sub"));
                File.WriteAllText(Path.Combine(path, "file1.txt"), "1");
                File.WriteAllText(Path.Combine(path, "sub\\file2.txt"), "2");

                var dir = new FileSystemDirectory(path);
                dir.DeleteContents();

                Directory.GetFiles(path).Length.ShouldEqual(0);
                Directory.GetDirectories(path).Length.ShouldEqual(0);
            }
        }
    }
}
