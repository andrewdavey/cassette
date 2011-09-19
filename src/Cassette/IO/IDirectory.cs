using System.Collections.Generic;
using System.IO;

namespace Cassette.IO
{
    public interface IDirectory
    {
        IDirectory NavigateTo(string path, bool createIfNotExists);
        void DeleteAll();
        IEnumerable<string> GetDirectories(string relativePath);
        IEnumerable<string> GetFiles(string directory, SearchOption searchOption);
        IEnumerable<string> GetFiles(string directory, SearchOption searchOption, string searchPattern);
        FileAttributes GetAttributes(string path);
        IFile GetFile(string filename);
    }
}