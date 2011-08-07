using System.IO;
using System.IO.IsolatedStorage;

namespace Cassette
{
    public class IsolatedStorageFileSystem : IFileSystem
    {
        public IsolatedStorageFileSystem(IsolatedStorageFile storage)
        {
            this.storage = storage;
        }

        readonly IsolatedStorageFile storage;

        public bool FileExists(string filename)
        {
            return storage.FileExists(filename);
        }

        public Stream OpenFile(string filename, FileMode mode, FileAccess access)
        {
            return storage.OpenFile(filename, mode, access);
        }

        public void DeleteAll()
        {
            foreach (var filename in storage.GetFileNames())
            {
                storage.DeleteFile(filename);
            }
        }
    }
}
