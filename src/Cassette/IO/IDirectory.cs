using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette.IO
{
    public interface IDirectory
    {
        IDirectory NavigateTo(string path, bool createIfNotExists);
        void DeleteAll();
        DateTime GetLastWriteTimeUtc(string filename);
        IEnumerable<string> GetDirectories(string relativePath);
        IEnumerable<string> GetFiles(string directory);
        IEnumerable<string> GetFiles(string directory, string searchPattern);
        FileAttributes GetAttributes(string path);
        string GetAbsolutePath(string path);
        bool DirectoryExists(string path);
        IFile GetFile(string filename);
    }
}