using System;
using System.IO;
using Cassette.Utilities;

namespace Cassette.IO
{
    public class FileSystemFile : IFile
    {
        readonly string name;
        readonly string systemAbsoluteFilename;
        readonly IDirectory directory;

        public FileSystemFile(string name, IDirectory directory, string systemAbsoluteFilename)
        {
            this.name = name;
            this.directory = directory;
            this.systemAbsoluteFilename = systemAbsoluteFilename;
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return PathUtilities.CombineWithForwardSlashes(directory.FullPath, name); }
        }

        public string FullSystemPath
        {
            get { return systemAbsoluteFilename; }
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

