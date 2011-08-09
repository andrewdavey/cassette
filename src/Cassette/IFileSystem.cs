using System;
using System.IO;
using System.Collections.Generic;

namespace Cassette
{
    public interface IFileSystem
    {
        IFileSystem AtSubDirectory(string path, bool createIfNotExists);
        bool FileExists(string filename);
        Stream OpenFile(string filename, FileMode mode, FileAccess access);
        void DeleteAll();
        DateTime GetLastWriteTimeUtc(string filename);

        IEnumerable<string> GetDirectories(string relativePath);

        bool DirectoryExists(string relativePath);

        IEnumerable<string> GetFiles(string directory);
        IEnumerable<string> GetFiles(string directory, string searchPattern);

        FileAttributes GetAttributes(string path);
    }
}
