using System;
using System.IO.IsolatedStorage;
using System.Linq;
using Should;
using Xunit;
using System.IO;

namespace Cassette.IO
{
    public class IsolatedStorageDirectory_Tests : IDisposable
    {
        readonly IsolatedStorageFile storage;

        public IsolatedStorageDirectory_Tests()
        {
            storage = IsolatedStorageFile.GetUserStoreForAssembly();
            foreach (var filename in storage.GetFileNames())
            {
                storage.DeleteFile(filename);
            }
        }

        public void Dispose()
        {
            storage.Dispose();
        }

        [Fact]
        public void FullPathDefaultsToForwardSlash()
        {
            var directory = new IsolatedStorageDirectory(storage);
            directory.FullPath.ShouldEqual("~/");
        }

        [Fact]
        public void GetFileReturnsWrapper()
        {
            storage.CreateFile("test.js").Close();

            var directory = new IsolatedStorageDirectory(storage);
            var file = directory.GetFile("test.js");

            file.ShouldBeType<IsolatedStorageFileWrapper>();
            file.FullPath.ShouldEqual("~/test.js");
            file.Directory.ShouldBeSameAs(directory);
        }

        [Fact]
        public void GetFilesReturnsFileWrappers()
        {
            storage.CreateFile("test1.js").Close();
            storage.CreateFile("test2.js").Close();

            var directory = new IsolatedStorageDirectory(storage);
            var files = directory.GetFiles("*", SearchOption.AllDirectories).ToArray();

            files[0].ShouldBeType<IsolatedStorageFileWrapper>();
            files[0].FullPath.ShouldEqual("~/test1.js");
            files[1].ShouldBeType<IsolatedStorageFileWrapper>();
            files[1].FullPath.ShouldEqual("~/test2.js");
        }
    }
}