﻿using System.IO;
using System.Linq;
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
                File.WriteAllText(Path.Combine(path, "test.txt"), "");

                var dir = new FileSystemDirectory(path);
                var file = dir.GetFile("test.txt");
                file.ShouldBeType<FileSystemFile>();
                PathUtilities.PathsEqual(file.FullPath, "~/test.txt").ShouldBeTrue();
            }
        }

        [Fact]
        public void GivenFileDoesNotExist_WhenGetFile_ThenReturnFileSystemFileThatDoesNotExist()
        {
            using (var path = new TempDirectory())
            {
                var dir = new FileSystemDirectory(path);
                var file = dir.GetFile("test.txt");
                file.ShouldBeType<FileSystemFile>();
                file.Exists.ShouldBeFalse();
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

    public class FileSystemDirectory_GetFiles_Tests
    {
        [Fact]
        public void ReturnsFilesWithApplicationAbsolutePaths()
        {
            using (var path = new TempDirectory())
            {
                File.WriteAllText(Path.Combine(path, "file.js"), "");
                var dir = new FileSystemDirectory(path);

                var files = dir.GetFiles("*", SearchOption.AllDirectories).ToArray();
                files[0].FullPath.ShouldEqual("~/file.js");
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

    public class FileSystemDirectory_GetDirectory_Tests
    {
        [Fact]
        public void GivenPathStartsWithTilde_WhenGetDirectoryFromSubDirectory_ThenPathIsFromRoot()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "sub1"));
                Directory.CreateDirectory(Path.Combine(path, "sub2"));
                var top = new FileSystemDirectory(path);
                var sub1 = top.GetDirectory("sub1");

                var sub2 = sub1.GetDirectory("~/sub2");

                sub2.FullPath.ShouldEqual("~/sub2");
            }
        }
    }

    public class FileSystemDirectory_DirectoryExists_Tests
    {
        [Fact]
        public void GivenSubDirectoryDoesNotExist_WhenDirectoryExists_ThenReturnFalse()
        {
            using (var path = new TempDirectory())
            {
                var dir = new FileSystemDirectory(path);
                dir.DirectoryExists("sub").ShouldBeFalse();
            }
        }
        
        [Fact]
        public void GivenSubDirectoryExists_WhenCallDirectoryExistsWithApplicationAbsolutePath_ThenReturnTrue()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "sub"));

                var dir = new FileSystemDirectory(path);
                dir.DirectoryExists("~/sub").ShouldBeTrue();
            }            
        }

        [Fact]
        public void WhenCallDirectoryExistsWithJustTilder_ThenReturnTrue()
        {
            using (var path = new TempDirectory())
            {
                var dir = new FileSystemDirectory(path);
                dir.DirectoryExists("~").ShouldBeTrue();
            }            
        }
    }

    public class FileSystemDirectory_GetFilePaths_Tests
    {
        [Fact]
        public void FullPathsReturned()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "test"));
                File.WriteAllText(PathUtilities.Combine(path, "test", "asset.js"), "");

                var dir = new FileSystemDirectory(path);
                var filePaths = dir.GetFiles("*.js", SearchOption.AllDirectories).ToArray();
                filePaths[0].FullPath.ShouldEqual("~/test/asset.js");
            }
        }

        [Fact]
        public void GivenNavigatedToSubDirectory_WhenGetFilePaths_ThenFullPathStillReturned()
        {
            using (var path = new TempDirectory())
            {
                Directory.CreateDirectory(Path.Combine(path, "test"));
                File.WriteAllText(PathUtilities.Combine(path, "test", "asset.js"), "");

                var testDir = new FileSystemDirectory(path).GetDirectory("test");
                var filePaths = testDir.GetFiles("*.js", SearchOption.AllDirectories).ToArray();
                filePaths[0].FullPath.ShouldEqual("~/test/asset.js");
            }
        }
    }
}

