using System.IO;

namespace Cassette
{
    public class FileSystem : IFileSystem
    {
        public FileSystem(string rootDirectory)
        {
            this.rootDirectory = rootDirectory;
        }

        readonly string rootDirectory;

        public Stream OpenRead(string filename)
        {
            return File.OpenRead(GetFullPath(filename));
        }

        public Stream OpenWrite(string filename)
        {
            return File.Open(GetFullPath(filename), FileMode.OpenOrCreate, FileAccess.ReadWrite);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(GetFullPath(filename));
        }

        string GetFullPath(string filename)
        {
            return Path.Combine(rootDirectory, filename);
        }
    }
}
