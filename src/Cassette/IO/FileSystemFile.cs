using System;
using System.IO;

namespace Cassette.IO
{
    public class FileSystemFile : IFile
    {
        readonly string filename;
        readonly IDirectory directory;

        public FileSystemFile(string filename, IDirectory directory)
        {
            this.filename = filename;
            this.directory = directory;
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            return File.Open(filename, mode, access);
        }

        public bool Exists
        {
            get { return File.Exists(filename); }
        }

        public DateTime LastWriteTimeUtc
        {
            get { return File.GetLastWriteTimeUtc(filename); }
        }
    }
}
