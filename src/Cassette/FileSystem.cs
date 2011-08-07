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

        public Stream OpenFile(string filename, FileMode mode, FileAccess access)
        {
            return File.Open(GetFullPath(filename), mode, access);
        }

        public bool FileExists(string filename)
        {
            return File.Exists(GetFullPath(filename));
        }

        public void DeleteAll()
        {
            foreach (var directory in Directory.GetDirectories(rootDirectory))
            {
                Directory.Delete(directory, true);
            }
            foreach (var filename in Directory.GetFiles(rootDirectory))
            {
                File.Delete(filename);
            }
        }

        string GetFullPath(string filename)
        {
            return Path.Combine(rootDirectory, filename);
        }
    }
}
