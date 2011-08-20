using System;
using System.Collections.Generic;
using System.IO;

namespace Cassette
{
    class StubFileSystem : IFileSystem
    {
        private readonly Dictionary<string, Stream> fileStreams;

        public StubFileSystem(Dictionary<string, Stream> fileStreams)
        {
            this.fileStreams = fileStreams;
        }

        public Stream OpenFile(string filename, FileMode mode, FileAccess access)
        {
            return fileStreams[filename];
        }

        public bool FileExists(string filename)
        {
            return fileStreams.ContainsKey(filename);
        }

        public void DeleteAll()
        {
            fileStreams.Clear();
        }

        public DateTime GetLastWriteTimeUtc(string filename)
        {
            throw new NotImplementedException();
        }

        public IFileSystem NavigateTo(string path, bool createIfNotExists)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetDirectories(string relativePath)
        {
            throw new NotImplementedException();
        }

        public bool DirectoryExists(string relativePath)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFiles(string directory)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> GetFiles(string directory, string searchPattern)
        {
            throw new NotImplementedException();
        }


        public FileAttributes GetAttributes(string path)
        {
            throw new NotImplementedException();
        }


        public string GetAbsolutePath(string path)
        {
            throw new NotImplementedException();
        }
    }
}
