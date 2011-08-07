using System.IO;
using System.Collections.Generic;

namespace Cassette
{
    public interface IFileSystem
    {
        bool FileExists(string filename);
        Stream OpenRead(string filename);
        Stream OpenWrite(string filename);

        void DeleteAll();
    }
}
