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
using System.IO.IsolatedStorage;
using Cassette.IO;
using Should;
using Xunit;

namespace Cassette.IntegrationTests
{
    public class IsolatedStorageFileSystem_Tests
    {
        [Fact]
        public void IsolatedStorageFileSystem_AccessesIsolatedStorage()
        {
            using (var store = IsolatedStorageFile.GetMachineStoreForAssembly())
            {
                using (var writer = new StreamWriter(store.OpenFile("test.txt", FileMode.Create, FileAccess.Write)))
                {
                    writer.Write("test");
                    writer.Flush();
                }

                var directory = new IsolatedStorageDirectory(store);
                var file = directory.GetFile("test.txt");
                file.Exists.ShouldBeTrue();
                using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    reader.ReadToEnd().ShouldEqual("test");
                }

                directory.DeleteContents();
                store.FileExists("test.txt").ShouldBeFalse();
            }
        }

        [Fact]
        public void IsolatedStorageFileSystemAtSubDirectory_AccessesIsolatedStorageSubDirectory()
        {
            using (var store = IsolatedStorageFile.GetMachineStoreForAssembly())
            {
                store.CreateDirectory("sub");
                using (var writer = new StreamWriter(store.OpenFile("sub\\test.txt", FileMode.Create, FileAccess.Write)))
                {
                    writer.Write("test");
                    writer.Flush();
                }

                var directory = new IsolatedStorageDirectory(store);
                var subDirectory = directory.GetDirectory("sub", false);

                var file = subDirectory.GetFile("test.txt");
                file.Exists.ShouldBeTrue();
                using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
                {
                    reader.ReadToEnd().ShouldEqual("test");
                }

                subDirectory.DeleteContents();
                store.FileExists("sub\\test.txt").ShouldBeFalse();
            }
        }
    }
}

