using System;
using System.IO;
using System.IO.IsolatedStorage;
using Cassette.Utilities;
using Should;
using Xunit;

namespace Cassette.IO
{
    public class IsolatedStorageFileWrapper_Tests : IDisposable
    {
        readonly IsolatedStorageFile storage;
        readonly IsolatedStorageDirectory directory;

        public IsolatedStorageFileWrapper_Tests()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
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
            var file = new IsolatedStorageFileWrapper("~/not-exists.js", storage, directory);
            file.Exists.ShouldBeFalse();
        }

        [Fact]
        public void GivenFileDoesExist_WhenCallExists_ThenReturnTrue()
        {
            var file = new IsolatedStorageFileWrapper("~/exists.js", storage, directory);
            file.Exists.ShouldBeTrue();
        }

        [Fact]
        public void FullPathReturnsFilename()
        {
            var file = new IsolatedStorageFileWrapper("~/exists.js", storage, directory);
            file.FullPath.ShouldEqual("~/exists.js");            
        }

        [Fact]
        public void DirectoryReturnsDirectoryPassedToConstructor()
        {
            var file = new IsolatedStorageFileWrapper("~/exists.js", storage, directory);
            file.Directory.ShouldBeSameAs(directory);
        }

        [Fact]
        public void GetLastWriteTimeUtcReturnsFileWriteTime()
        {
            var file = new IsolatedStorageFileWrapper("~/exists.js", storage, directory);
            file.LastWriteTimeUtc.ShouldEqual(storage.GetLastWriteTime("exists.js").UtcDateTime);            
        }

        [Fact]
        public void OpenStreamReturnsFileStream()
        {
            var file = new IsolatedStorageFileWrapper("~/exists.js", storage, directory);
            var content = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read).ReadToEnd();
            content.ShouldEqual("content");
        }

        [Fact]
        public void DeleteRemovesFileFromStorage()
        {
            var file = new IsolatedStorageFileWrapper("~/exists.js", storage, directory);
            file.Delete();
            storage.FileExists("exists.js").ShouldBeFalse();
        }
    }
}