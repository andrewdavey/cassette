#region License
/*
Copyright 2011 Andrew Davey

This file is part of Cassette.

Cassette is free software: you can redistribute it and/or modify it under the 
terms of the GNU General Public License as published by the Free Software 
Foundation, either version 3 of the License, or (at your option) any later 
version.

Cassette is distributed in the hope that it will be useful, but WITHOUT ANY 
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS 
FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received a copy of the GNU General Public License along with 
Cassette. If not, see http://www.gnu.org/licenses/.
*/
#endregion

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

    public class FileSystemDirectory_GetDirectory_Tests
    {
        [Fact]
        public void GivenSubDirectoryDoesNotExist_WhenGetDirectoryWithCreateTrue_ThenDirectoryIsCreated()
        {
            using (var path = new TempDirectory())
            {
                var dir = new FileSystemDirectory(path);
                dir.GetDirectory("sub", true);

                Directory.Exists(Path.Combine(path, "sub")).ShouldBeTrue();
            }
        }
    }
}

