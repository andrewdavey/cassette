using System;
using System.Linq;
using Should;
using Xunit;
using System.IO;
#if NET35
using Cassette.Utilities;
#endif

namespace Cassette.IO
{
    public class IsolatedStorageDirectory_Tests : IDisposable
    {
        readonly System.IO.IsolatedStorage.IsolatedStorageFile storage;

        public IsolatedStorageDirectory_Tests()
        {
            storage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForAssembly();
            foreach (var filename in storage.GetFileNames("*"))
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
            var directory = new IsolatedStorageDirectory(() => storage);
            directory.FullPath.ShouldEqual("~/");
        }

        [Fact]
        public void GetFileReturnsWrapper()
        {
            storage.CreateFile("test.js").Close();

            var directory = new IsolatedStorageDirectory(() => storage);
            var file = directory.GetFile("test.js");

            file.ShouldBeType<IsolatedStorageFile>();
            file.FullPath.ShouldEqual("~/test.js");
            file.Directory.ShouldBeSameAs(directory);
        }

        [Fact]
        public void GetFilesReturnsFileWrappers()
        {
            storage.CreateFile("test1.js").Close();
            storage.CreateFile("test2.js").Close();

            var directory = new IsolatedStorageDirectory(() => storage);
            var files = directory.GetFiles("*", SearchOption.AllDirectories).ToArray();

            files[0].ShouldBeType<IsolatedStorageFile>();
            files[0].FullPath.ShouldEqual("~/test1.js");
            files[1].ShouldBeType<IsolatedStorageFile>();
            files[1].FullPath.ShouldEqual("~/test2.js");
        }
    }
}
