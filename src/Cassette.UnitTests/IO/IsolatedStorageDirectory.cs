using System;
using System.IO;
using System.Linq;
using Should;
using Xunit;

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

        [Fact]
        public void CanDeleteSubDirectoryWithContents()
        {
            storage.CreateDirectory("test");
            WriteFile("test/file.txt");
            storage.CreateDirectory("test/sub");
            WriteFile("test/sub/file.txt");

            var directory = new IsolatedStorageDirectory(() => storage);
            var subDirectory = directory.GetDirectory("test");
            subDirectory.Delete();

            storage.DirectoryExists("test").ShouldBeFalse();
        }

        void WriteFile(string filename)
        {
            using (var file = storage.CreateFile(filename))
            {
                file.Write(new byte[] { 1 }, 0, 1);
                file.Flush(true);
            }
        }

        [Fact]
        public void WhenGetFileInSubDirectoryFromRoot_ThenFilesDirectoryIsTheSubDirectory()
        {
            storage.CreateDirectory("test/sub");
            WriteFile("test/sub/file.txt");

            var root = new IsolatedStorageDirectory(() => storage);
            var file = root.GetFile("test/sub/file.txt");
            file.Directory.FullPath.ShouldEqual("~/test/sub");
        }
    }
}