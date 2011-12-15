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
using Should;
using Xunit;

namespace Cassette.IO
{
    public class IsolatedStorageFileWrapper_Tests : IDisposable
    {
        readonly System.IO.IsolatedStorage.IsolatedStorageFile storage;
        readonly IsolatedStorageDirectory directory;

        public IsolatedStorageFileWrapper_Tests()
        {
            storage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly();
            directory = new IsolatedStorageDirectory(storage);
            using (var stream = storage.CreateFile("exists.js"))
            {
                "content".AsStream().CopyTo(stream);
                stream.Flush();
            }
        }

        public void Dispose()
        {
            storage.Dispose();
        }

        [Fact]
        public void GivenFileDoesNotExist_WhenCallExists_ThenReturnFalse()
        {
            var file = new IsolatedStorageFile("~/not-exists.js", storage, directory);
            file.Exists.ShouldBeFalse();
        }

        [Fact]
        public void GivenFileDoesExist_WhenCallExists_ThenReturnTrue()
        {
            var file = new IsolatedStorageFile("~/exists.js", storage, directory);
            file.Exists.ShouldBeTrue();
        }

        [Fact]
        public void FullPathReturnsFilename()
        {
            var file = new IsolatedStorageFile("~/exists.js", storage, directory);
            file.FullPath.ShouldEqual("~/exists.js");            
        }

        [Fact]
        public void DirectoryReturnsDirectoryPassedToConstructor()
        {
            var file = new IsolatedStorageFile("~/exists.js", storage, directory);
            file.Directory.ShouldBeSameAs(directory);
        }

        [Fact]
        public void GetLastWriteTimeUtcReturnsFileWriteTime()
        {
            var file = new IsolatedStorageFile("~/exists.js", storage, directory);
            file.LastWriteTimeUtc.ShouldEqual(storage.GetLastWriteTime("exists.js").UtcDateTime);            
        }

        [Fact]
        public void OpenStreamReturnsFileStream()
        {
            var file = new IsolatedStorageFile("~/exists.js", storage, directory);
            var content = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read).ReadToEnd();
            content.ShouldEqual("content");
        }

        [Fact]
        public void DeleteRemovesFileFromStorage()
        {
            var file = new IsolatedStorageFile("~/exists.js", storage, directory);
            file.Delete();
            storage.FileExists("exists.js").ShouldBeFalse();
        }
    }
}
