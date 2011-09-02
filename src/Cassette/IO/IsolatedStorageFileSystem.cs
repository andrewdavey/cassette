using System;
using System.IO;
using System.IO.IsolatedStorage;

namespace Cassette.IO
{
    public class IsolatedStorageFileWrapper : IFile
    {
        readonly string filename;
        readonly IsolatedStorageFile storage;
        readonly IsolatedStorageDirectory directory;

        public IsolatedStorageFileWrapper(string filename, IsolatedStorageFile storage, IsolatedStorageDirectory directory)
        {
            this.filename = filename;
            this.storage = storage;
            this.directory = directory;
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            return storage.OpenFile(filename, mode, access);
        }

        public bool Exists
        {
            get { return storage.FileExists(filename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return storage.GetLastWriteTime(filename).UtcDateTime; }
        }
    }
}
