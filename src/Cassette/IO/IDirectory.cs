using System.Collections.Generic;
using System.IO;

namespace Cassette.IO
{
    public interface IDirectory
    {
        IFile GetFile(string filename);
        IDirectory GetDirectory(string path, bool createIfNotExists);

        IEnumerable<string> GetDirectoryPaths(string relativePath);
        IEnumerable<string> GetFilePaths(string directory, SearchOption searchOption, string searchPattern);
        FileAttributes GetAttributes(string path);

        void DeleteContents();
    }
}