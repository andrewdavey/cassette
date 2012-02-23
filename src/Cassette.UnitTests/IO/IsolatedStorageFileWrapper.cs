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

        // According to andrew, LastWriteTime is actually never really called in
        // code, so we should be able to safely ignore it (for FX35)
        //[Fact]
        //public void GetLastWriteTimeUtcReturnsFileWriteTime()
        //{
        //    var file = new IsolatedStorageFile("~/exists.js", storage, directory);

        //    // FX35 workaround: range of accepted values
        //    var low = storage.GetLastWriteTime("exists.js").UtcDateTime;
        //    var high = low.AddMilliseconds(500); // threshold, 500ms

        //    file.LastWriteTimeUtc.ShouldBeInRange(low, high);
        //    // FX40 file.LastWriteTimeUtc.ShouldEqual(low);
        //}

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
