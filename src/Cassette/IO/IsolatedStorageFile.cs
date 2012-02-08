using System;
using System.IO;
using System.IO.IsolatedStorage;
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

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return new IsolatedStorageFileStream(systemFilename, mode, access, fileShare, Storage);
        }

        public bool Exists
        {
            get
            {
                return Storage.GetFileNames(systemFilename).Length > 0;
            }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                var source =
                        new IsolatedStorageFileStream(systemFilename, FileMode.OpenOrCreate,
                        Storage);

                

                return Storage.GetLastWriteTime(systemFilename).UtcDateTime;
            }
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