using System.IO;

namespace Cassette
{
    public interface IFileSystem
    {
        bool FileExists(string filename);
        Stream OpenRead(string filename);
        Stream OpenWrite(string filename);
    }
}
