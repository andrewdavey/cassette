using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Storage = System.IO.IsolatedStorage.IsolatedStorageFile;

namespace Cassette.IO
{
    /// <remarks>
    /// This class only implements enough of IDirectory to support BundleCache.
    /// Other methods simply throw NotSupportException for now.
    /// </remarks>
    public class IsolatedStorageDirectory : IDirectory
    {
        readonly Func<Storage> getStorage;
        readonly string basePath;

        public IsolatedStorageDirectory(Func<Storage> getStorage)
        {
            this.getStorage = getStorage;
            basePath = "~/";
        }

        IsolatedStorageDirectory(Func<Storage> getStorage, string basePath)
        {
            this.getStorage = getStorage;
            this.basePath = basePath;
        }

        public string FullPath
        {
            get { return basePath; }
        }

        public bool Exists
        {
            get { return getStorage().DirectoryExists(basePath); }
        }

        public IFile GetFile(string filename)
        {
            filename = GetAbsolutePath(filename);
            var parts = filename.Split('/', '\\');
            IsolatedStorageDirectory directory;
            if (parts.Length > 2)
            {
                var subDirectory = string.Join("/", parts.Reverse().Skip(1).Reverse());
                directory = new IsolatedStorageDirectory(getStorage, subDirectory);
            }
            else
            {
                directory = this;
            }
            return new IsolatedStorageFile(filename, getStorage, directory);
        }

        string GetAbsolutePath(string path)
        {
            return Path.Combine(basePath, path);
        }

        public bool DirectoryExists(string path)
        {
            throw new NotSupportedException();
        }

        public FileAttributes Attributes
        {
            get { throw new NotSupportedException(); }
        }

        public IDirectory GetDirectory(string path)
        {
            return new IsolatedStorageDirectory(getStorage, Path.Combine(basePath, path));
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            var storage = getStorage();
#if NET35
            return storage.GetFileNames(searchPattern).Select(
                filename => new IsolatedStorageFile(GetAbsolutePath(filename), getStorage, this)
            ).Cast<IFile>();
#else
            return storage.GetFileNames(searchPattern).Select(
                filename => new IsolatedStorageFile(GetAbsolutePath(filename), getStorage, this)
            );
#endif
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            throw new NotSupportedException();
        }

        public void Create()
        {
            getStorage().CreateDirectory(basePath.TrimStart('~', '/'));
        }

        public void Delete()
        {
            var storage = getStorage();
            var path = basePath.TrimStart('~', '/');
            RecursiveDirectoryDelete(storage, path);
        }

        void RecursiveDirectoryDelete(Storage storage, string path)
        {
            if (path.Length > 0) path += "/";
            foreach (var subDirectory in storage.GetDirectoryNames(path + "*"))
            {
                RecursiveDirectoryDelete(storage, path + subDirectory);
            }
            DeleteFiles(storage, path);
            storage.DeleteDirectory(path);
        }

        void DeleteFiles(Storage storage, string directoryPath)
        {
            foreach (var filename in storage.GetFileNames(directoryPath + "*"))
            {
                storage.DeleteFile(directoryPath + filename);
            }
        }

        public IDisposable WatchForChanges(Action<string> pathCreated, Action<string> pathChanged, Action<string> pathDeleted, Action<string, string> pathRenamed)
        {
            throw new NotSupportedException();
        }
    }
}