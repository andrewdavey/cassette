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

        public Stream Open(FileMode mode, FileAccess access)
        {
            return directory.OpenFile(filename, mode, access);
        }
    }
}
