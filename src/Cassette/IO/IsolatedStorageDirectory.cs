using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Cassette.IO
{
    /// <remarks>
    /// This class only implements enough of IDirectory to support BundleCache.
    /// Other methods simply throw NotSupportException for now.
    /// </remarks>
    public class IsolatedStorageDirectory : IDirectory
    {
        public IsolatedStorageDirectory(System.IO.IsolatedStorage.IsolatedStorageFile storage)
            : this(storage, "~/")
        {
        }

        IsolatedStorageDirectory(System.IO.IsolatedStorage.IsolatedStorageFile storage, string basePath)
        {
            this.storage = storage;
            this.basePath = basePath;
        }

        readonly System.IO.IsolatedStorage.IsolatedStorageFile storage;
        readonly string basePath;

        public string FullPath
        {
            get { return basePath; }
        }

        public IFile GetFile(string filename)
        {
            return new IsolatedStorageFile(GetAbsolutePath(filename), storage, this);
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
            throw new NotSupportedException();
        }

        public IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption)
        {
            return storage.GetFileNames(searchPattern)
                          .Select(filename => new IsolatedStorageFile(GetAbsolutePath(filename), storage, this));
        }

        public IEnumerable<IDirectory> GetDirectories()
        {
            throw new NotSupportedException();
        }
    }
}