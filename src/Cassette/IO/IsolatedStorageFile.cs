using System;
using System.IO;
using Storage = System.IO.IsolatedStorage.IsolatedStorageFile;

namespace Cassette.IO
{
    public class IsolatedStorageFile : IFile
    {
        readonly string filename;
        readonly Func<Storage> getStorage;
        readonly IsolatedStorageDirectory directory;
        readonly string systemFilename;

        public IsolatedStorageFile(string filename, Storage storage, IsolatedStorageDirectory directory)
            : this(filename, () => storage, directory)
        {
        }

        public IsolatedStorageFile(string filename, Func<Storage> getStorage, IsolatedStorageDirectory directory)
        {
            this.filename = filename;
            this.getStorage = getStorage;
            this.directory = directory;
            systemFilename = filename.Substring(2); // Skip the "~/" prefix.
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public string FullSystemPath
        {
            get { return systemFilename; }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return Storage.OpenFile(systemFilename, mode, access, fileShare);
        }

        public bool Exists
        {
            get
            {
                return Storage.FileExists(systemFilename);
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return Storage.GetLastWriteTime(systemFilename).UtcDateTime; }
        }

        public void Delete()
        {
            Storage.DeleteFile(systemFilename);
        }

        Storage Storage
        {
            get { return getStorage(); }
        }
    }
}