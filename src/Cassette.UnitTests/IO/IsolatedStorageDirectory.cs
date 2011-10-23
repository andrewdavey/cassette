using System;
using System.IO.IsolatedStorage;
using Should;
using Xunit;

namespace Cassette.IO
{
    public class IsolatedStorageDirectory_Tests : IDisposable
    {
        readonly IsolatedStorageFile storage;

        public IsolatedStorageDirectory_Tests()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
        }

        public void Dispose()
        {
            storage.Dispose();
        }

        [Fact]
        public void FullPathDefaultsToForwardSlash()
        {
            var directory = new IsolatedStorageDirectory(storage);
            directory.FullPath.ShouldEqual("/");
        }

        [Fact]
        public void GivenFileInStorage_WhenDeleteContents_FileIsDeleted()
        {
            storage.CreateFile("test.js").Close();

            var directory = new IsolatedStorageDirectory(storage);
            directory.DeleteContents();

            storage.FileExists("test.js").ShouldBeFalse();
        }

        [Fact]
        public void GetFileReturnsWrapper()
        {
            storage.CreateFile("test.js").Close();

            var directory = new IsolatedStorageDirectory(storage);
            var file = directory.GetFile("test.js");

            file.ShouldBeType<IsolatedStorageFileWrapper>();
            file.FullPath.ShouldEqual("/test.js");
            file.Directory.ShouldBeSameAs(directory);
        }
    }
}