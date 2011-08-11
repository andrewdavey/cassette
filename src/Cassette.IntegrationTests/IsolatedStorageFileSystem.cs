using System.IO;
using System.IO.IsolatedStorage;
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

                var fileSystem = new IsolatedStorageFileSystem(store);
                fileSystem.FileExists("test.txt").ShouldBeTrue();
                using (var reader = new StreamReader(fileSystem.OpenFile("test.txt", FileMode.Open, FileAccess.Read)))
                {
                    reader.ReadToEnd().ShouldEqual("test");
                }

                fileSystem.DeleteAll();
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

                var fileSystem = new IsolatedStorageFileSystem(store);
                var subFileSystem = fileSystem.NavigateTo("sub", false);

                subFileSystem.FileExists("test.txt").ShouldBeTrue();
                using (var reader = new StreamReader(subFileSystem.OpenFile("test.txt", FileMode.Open, FileAccess.Read)))
                {
                    reader.ReadToEnd().ShouldEqual("test");
                }

                subFileSystem.DeleteAll();
                store.FileExists("sub\\test.txt").ShouldBeFalse();
            }
        }
    }
}
