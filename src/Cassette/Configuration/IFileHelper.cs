using System;
using Cassette.IO;

namespace Cassette.Configuration
{
    public interface IFileHelper
    {
        void Write(string filename, string writeText);
        string ReadAllText(string filename);
        bool Exists(string fileName); 
        DateTime GetLastAccessTime(string filename);
        void Delete(string fileName);
        void PrepareCachingDirectory(string cacheDirectory, string cacheVersion);
        void CreateDirectory(string directory);
        IFile GetFileSystemFile(IDirectory directory, string systemAbsoluteFilename, string cacheDirectory);
    }
}