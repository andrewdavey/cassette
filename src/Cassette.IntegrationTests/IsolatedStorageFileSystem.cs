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
    }
}
