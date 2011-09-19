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
                using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read)))
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
                using (var reader = new StreamReader(file.Open(FileMode.Open, FileAccess.Read)))
                {
                    reader.ReadToEnd().ShouldEqual("test");
                }

                subDirectory.DeleteContents();
                store.FileExists("sub\\test.txt").ShouldBeFalse();
            }
        }
    }
}
