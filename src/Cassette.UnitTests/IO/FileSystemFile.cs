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

