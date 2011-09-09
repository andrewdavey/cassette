using System;
using System.IO;

namespace Cassette.IO
{
    public class NonExistentFile : IFile
    {
        readonly string filename;

        public NonExistentFile(string filename)
        {
            this.filename = filename;
        }

        public IDirectory Directory
        {
            get
            {
                ThrowFileNotFoundException();
                return null;
            }
        }

        public bool Exists
        {
            get { return false; }
        }

        public DateTime LastWriteTimeUtc
        {
            get
            {
                ThrowFileNotFoundException();
                return default(DateTime);
            }
        }

        public string FullPath
        {
            get { return filename; }
        }

        public Stream Open(FileMode mode, FileAccess access)
        {
            ThrowFileNotFoundException();
            return null;
        }

        void ThrowFileNotFoundException()
        {
            throw new FileNotFoundException("File not found \"" + filename + "\".", filename);
        }
    }
}