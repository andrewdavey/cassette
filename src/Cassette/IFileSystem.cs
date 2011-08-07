using System.IO;
using System.Collections.Generic;

namespace Cassette
{
    public interface IFileSystem
    {
        bool FileExists(string filename);
        Stream OpenFile(string filename, FileMode mode, FileAccess access);
        void DeleteAll();
    }
}
