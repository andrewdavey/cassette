using System;
using System.IO;
using Cassette.IO;

namespace Cassette
{
    /// <summary>
    /// Stub implementation of IFile for use by ResourceAsset.
    /// Only the FullPath property really matters.
    /// </summary>
    class ResourceFile : IFile
    {
        readonly string fullPath;

        public ResourceFile(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public IDirectory Directory
        {
            get { throw new NotImplementedException(); }
        }

        public bool Exists
        {
            get { return true; }
        }

        public DateTime LastWriteTimeUtc
        {
            get { throw new NotImplementedException(); }
        }

        public string FullPath
        {
            get { return fullPath; }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
    }
}