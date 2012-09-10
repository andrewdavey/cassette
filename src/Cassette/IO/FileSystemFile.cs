using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.IO
{
    public class FileSystemFile : IFile
    {
        readonly string systemAbsoluteFilename;
        readonly IDirectory directory;
        readonly string fullPath;

        public FileSystemFile(string name, IDirectory directory, string systemAbsoluteFilename)
        {
            this.directory = directory;
            this.systemAbsoluteFilename = systemAbsoluteFilename;
            fullPath = PathUtilities.CombineWithForwardSlashes(directory.FullPath, name);
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return fullPath; }
        }

        public Stream Open(FileMode mode, FileAccess access, FileShare fileShare)
        {
            return File.Open(systemAbsoluteFilename, mode, access, fileShare);
        }

        public bool Exists
        {
            get { return File.Exists(systemAbsoluteFilename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return File.GetLastWriteTimeUtc(systemAbsoluteFilename); }
        }

        public void Delete()
        {
            File.Delete(systemAbsoluteFilename);
        }
    }
}

