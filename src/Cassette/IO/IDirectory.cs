using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette.IO
{
    public interface IDirectory
    {
        FileAttributes Attributes { get; }
        string FullPath { get; }
        IFile GetFile(string filename);
        IDirectory GetDirectory(string path);
        bool DirectoryExists(string path);
        IEnumerable<IDirectory> GetDirectories();
        IEnumerable<IFile> GetFiles(string searchPattern, SearchOption searchOption);
        IDisposable WatchForChanges(
            Action<string> pathCreated,
            Action<string> pathChanged,
            Action<string> pathDeleted,
            Action<string, string> pathRenamed 
        );
    }
}