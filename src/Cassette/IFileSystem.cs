using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    public interface IFileSystem
    {
        IFileSystem NavigateTo(string path, bool createIfNotExists);
        bool FileExists(string filename);
        Stream OpenFile(string filename, FileMode mode, FileAccess access);
        void DeleteAll();
        DateTime GetLastWriteTimeUtc(string filename);
        IEnumerable<string> GetDirectories(string relativePath);
        IEnumerable<string> GetFiles(string directory);
        IEnumerable<string> GetFiles(string directory, string searchPattern);
        FileAttributes GetAttributes(string path);
        string GetAbsolutePath(string path);
    }
}