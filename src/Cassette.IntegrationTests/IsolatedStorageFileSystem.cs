using System.IO;
using Cassette.IO;
using Should;
using Xunit;
using IsolatedStorageFile = System.IO.IsolatedStorage.IsolatedStorageFile;

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

                file.Delete();
                store.FileExists("test.txt").ShouldBeFalse();
            }
        }
    }
}